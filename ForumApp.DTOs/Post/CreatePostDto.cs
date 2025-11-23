using System.ComponentModel.DataAnnotations;

namespace ForumApp.DTOs.Post
{
    /// <summary>
    /// Data transfer object for creating a post
    /// </summary>
    public class CreatePostDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int ForumId { get; set; }
    }
}

