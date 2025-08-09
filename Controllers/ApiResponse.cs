using System;

namespace dotnet_utcareers.Controllers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        // Static methods untuk kemudahan penggunaan
        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>(true, message, data);
        }

        public static ApiResponse<T> SuccessResponse(string message = "Success")
        {
            return new ApiResponse<T>(true, message);
        }

        public static ApiResponse<T> ErrorResponse(string message, T? data = default)
        {
            return new ApiResponse<T>(false, message, data);
        }
    }

    // Non-generic version untuk response tanpa data
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ApiResponse SuccessResponse(object? data = null, string message = "Success")
        {
            return new ApiResponse(true, message, data);
        }

        public static ApiResponse ErrorResponse(string message, object? data = null)
        {
            return new ApiResponse(false, message, data);
        }
    }
}