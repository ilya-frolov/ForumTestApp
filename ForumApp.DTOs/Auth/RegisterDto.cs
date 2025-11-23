using System.ComponentModel.DataAnnotations;

namespace ForumApp.DTOs.Auth
{
    /// <summary>
    /// Data transfer object for user registration
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be with min length of 6 characters, and contains capital letter, small letter and nonAlphaNumeric character")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}

