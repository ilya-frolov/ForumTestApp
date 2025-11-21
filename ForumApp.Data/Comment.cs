using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumApp.Data
{
    /// <summary>
    /// Represents a comment on a post
    /// </summary>
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int PostId { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; } = null!;
    }
}

