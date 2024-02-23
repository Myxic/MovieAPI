﻿namespace Movie.Api.Utility;

public record ApiRecordResponse<T>(bool IsSuccessful, string? Message, T? Data) where T : BaseRecord;

public record ApiResponse<T>(bool IsSuccessful, string? Message, T? Data) where T : class;

public record ApiResponse(bool IsSuccessful, string? Message);
