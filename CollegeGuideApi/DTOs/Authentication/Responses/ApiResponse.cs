namespace CollegeGuideApi.DTOs.Authentication.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public static ApiResponse<T> SuccessResponse( string message = "", string token = null) 
        {
            return new ApiResponse<T> { Success = true, Message = message, Token = token };
        }

        public static ApiResponse<T> FailureResponse(string message, List<string> errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors ?? new List<string>() };
        }
    }
}
