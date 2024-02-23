using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Movie.Api.Dtos.Response;
using Movie.Api.Utility;



namespace Movie.Api.Controller
{
    [ApiController]
    [Route("api/[Controller]")]
    public class MoviesController(HttpClient httpClientFactory,
        OmdConfig omdConfig,
        IMemoryCache cache) : BaseController
    {
        private readonly HttpClient _httpClientFactory = httpClientFactory;
        private readonly IMemoryCache _cache = cache;
        private readonly OmdConfig _omdConfig = omdConfig;
        private const int MAX_CACHE_SIZE = 5;

        [HttpGet("search")]
        [ProducesResponseType(200, Type = typeof(ApiRecordResponse<MovieInfoResponseDto>))]
        [ProducesResponseType(404, Type = typeof(ApiResponse))]
        [ProducesResponseType(400, Type = typeof(ApiResponse))]
        public async Task<IActionResult> SearchMovies([FromQuery] string title)

        {
            try
            {
                var cacheKey = $"MovieSearch_{title}";
                var searchesQueue = _cache.GetOrCreate("SearchesQueue", entry => new Queue<string>());

                if (!_cache.TryGetValue(cacheKey, out MovieInfoResponseDto? cachedResponse))
                {

                    _httpClientFactory.BaseAddress = new Uri(_omdConfig.BaseUrl);
                    _httpClientFactory.DefaultRequestHeaders.Accept.Clear();
                    _httpClientFactory.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _httpClientFactory.DefaultRequestHeaders.Add("apikey", $"{_omdConfig.ApiKey}");

                    var response = await _httpClientFactory.GetAsync($"{_omdConfig.BaseUrl}/?t={title}&apikey={_omdConfig.ApiKey}");

                    response.EnsureSuccessStatusCode(); // Throw exception if not success status code

                    string jsonString = await response.Content.ReadAsStringAsync();
                    cachedResponse = JsonSerializer.Deserialize<MovieInfoResponseDto>(jsonString);
                    if (cachedResponse!.Response.ToString() == "False" ) throw new Exception($"Movie Title \"{title}\" does not exist");

                    if (searchesQueue?.Count >= MAX_CACHE_SIZE)
                    {
                        var keyToRemove = searchesQueue.Dequeue();
                        _cache.Remove(keyToRemove);
                    }

                    searchesQueue?.Enqueue(cacheKey);
                    _cache.Set("SearchesQueue", searchesQueue);
                    _cache.Set(cacheKey, cachedResponse, TimeSpan.FromMinutes(10)); // Cache for 100 minutes
                    
                }

                return Ok(cachedResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cached-data")]
        public IActionResult GetCachedData()
        {
            try
            {
                var cachedData = new List<object>();
                if (_cache.Get("SearchesQueue") is Queue<string> searchesQueue)
                {
                    foreach (var cacheKey in searchesQueue)
                    {
                        if (_cache.TryGetValue(cacheKey, out MovieInfoResponseDto? cachedResponse))
                        {
                            cachedData.Add(cachedResponse!);
                        }
                    }
                }
                return Ok(cachedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving cached data: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }

}