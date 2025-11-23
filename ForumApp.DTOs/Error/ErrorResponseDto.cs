namespace ForumApp.DTOs.Error
{
    /// <summary>
    /// Standardized error response returned by API controllers.
    /// Ensures consistent error format across endpoints.
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// Numeric status code (e.g., 400, 401, 404).
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Short error identifier (e.g., VALIDATION_FAILED, POST_NOT_FOUND).
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error messages (e.g., validation errors, exception details).
        /// </summary>
        public IEnumerable<string> Details { get; set; } = new List<string>();
    }
}

