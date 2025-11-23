namespace ForumApp.DTOs.Post
{
    /// <summary>
    /// Data transfer object for existing post
    /// </summary>
    public class PostDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int ForumId { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

