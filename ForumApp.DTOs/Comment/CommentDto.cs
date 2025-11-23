namespace ForumApp.DTOs.Comment
{
    /// <summary>
    /// Data transfer object for existing comment
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public int PostId { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

