using Movie.Api.Utility;

var builder = WebApplication.CreateBuilder(args);

IConfiguration Configuration = builder.Configuration;
IServiceCollection services = builder.Services;
Settings setting = Configuration.Get<Settings>()!;
if (setting == null)
{
    throw new ArgumentNullException(nameof(setting));
}

OmdConfig? omdConfig = setting.OmdConfig;
if (omdConfig == null)
{
    throw new ArgumentNullException(nameof(omdConfig));
}

services.AddHttpClient();
services.AddSingleton(setting);
services.AddSingleton(omdConfig);
// Other service configurations

services.AddMemoryCache(); // Add MemoryCache service





builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(policyBuilder =>
                policyBuilder.AddDefaultPolicy(policy =>
                policy.WithOrigins("*")
                .AllowAnyHeader()
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader().SetIsOriginAllowed(origin => true)));

var app = builder.Build();
app.UseCors(x => x
           .AllowAnyMethod()
           .AllowAnyHeader()
           .SetIsOriginAllowed(origin => true)
           .AllowCredentials());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();
app.Run();
