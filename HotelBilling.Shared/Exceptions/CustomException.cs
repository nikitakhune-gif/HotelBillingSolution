using System;

namespace HotelBilling.Shared.Exceptions
{
    public class CustomException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public CustomException(string message)
            : base(message)
        {
            StatusCode = 400;
            ErrorCode = "BAD_REQUEST";
        }

        public CustomException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = GetDefaultErrorCode(statusCode);
        }

        public CustomException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        private string GetDefaultErrorCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "BAD_REQUEST",
                401 => "UNAUTHORIZED",
                403 => "FORBIDDEN",
                404 => "NOT_FOUND",
                500 => "INTERNAL_SERVER_ERROR",
                _ => "UNKNOWN_ERROR"
            };
        }
    }
}