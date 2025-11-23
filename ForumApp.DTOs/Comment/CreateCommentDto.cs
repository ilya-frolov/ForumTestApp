using System.ComponentModel.DataAnnotations;

namespace ForumApp.DTOs.Comment
{
    /// <summary>
    /// Data transfer object for creating a comment
    /// </summary>
    public class CreateCommentDto
    {
        [Required]
        [MaxLength(1000)]
        [MinLength(1, ErrorMessage = "Content must not be empty.")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int PostId { get; set; }
    }
}

