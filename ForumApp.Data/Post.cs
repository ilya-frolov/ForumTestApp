using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumApp.Data
{
    /// <summary>
    /// Represents a post in a forum
    /// </summary>
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int ForumId { get; set; }

        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ForumId))]
        public virtual Forum Forum { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

