using System.ComponentModel.DataAnnotations;

namespace ForumApp.Data
{
    /// <summary>
    /// Represents a forum category
    /// </summary>
    public class Forum
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}

