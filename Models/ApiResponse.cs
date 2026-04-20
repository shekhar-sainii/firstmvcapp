namespace FirstMvcApp.Models;

/// <summary>
/// Generic API response wrapper for all API endpoints.
/// Provides consistent success/error response format across the application.
/// </summary>
/// <typeparam name="T">Type of data returned in response</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; } = 200;
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Private constructor for factory methods
    private ApiResponse() { }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T? data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, T? data = default, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a server error response (500)
    /// </summary>
    public static ApiResponse<T> ServerErrorResponse(string message = "An internal server error occurred")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 500,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a validation error response with detailed errors
    /// </summary>
    public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 422,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a not found response (404)
    /// </summary>
    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an unauthorized response (401)
    /// </summary>
    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized access")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 401,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a forbidden response (403)
    /// </summary>
    public static ApiResponse<T> ForbiddenResponse(string message = "Access forbidden")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 403,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Non-generic API response for simple responses without typed data
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public int StatusCode { get; set; } = 200;
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Private constructor for factory methods
    private ApiResponse() { }

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse SuccessResponse(object? data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponse ErrorResponse(string message, int statusCode = 400, object? data = null, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a server error response (500)
    /// </summary>
    public static ApiResponse ServerErrorResponse(string message = "An internal server error occurred")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 500,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a validation error response with detailed errors
    /// </summary>
    public static ApiResponse ValidationErrorResponse(Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 422,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a not found response (404)
    /// </summary>
    public static ApiResponse NotFoundResponse(string message = "Resource not found")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an unauthorized response (401)
    /// </summary>
    public static ApiResponse UnauthorizedResponse(string message = "Unauthorized access")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 401,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a forbidden response (403)
    /// </summary>
    public static ApiResponse ForbiddenResponse(string message = "Access forbidden")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 403,
            Timestamp = DateTime.UtcNow
        };
    }
}
