using ForumApp.Data;
using ForumApp.DTOs.Comment;
using ForumApp.DTOs.Forum;
using ForumApp.DTOs.Post;

namespace ForumApp.Utils
{
    /// <summary>
    /// Utility class for mapping domain entities to DTOs.
    /// Keeps controllers and managers clean by centralizing mapping logic.
    /// </summary>
    public class DtoMapper
    {
        #region Forum

        public static ForumDto MapForumToDto(Forum forum) => new ForumDto
        {
            Id = forum.Id,
            Name = forum.Name,
            Description = forum.Description,
            CreatedAt = forum.CreatedAt,
            UpdatedAt = forum.UpdatedAt
        };

        #endregion

        #region Post

        public static PostDto MapPostToDto(Post post) => new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            ForumId = post.ForumId,
            CreatedByUserId = post.CreatedByUserId,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        public static Post MapCreatePostDtoToEntity(CreatePostDto dto, string userId) => new Post
        {
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            ForumId = dto.ForumId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        #endregion

        #region Comment

        public static CommentDto MapCommentToDto(Comment comment) => new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            PostId = comment.PostId,
            CreatedByUserId = comment.CreatedByUserId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        public static Comment MapCreateCommentDtoToEntity(CreateCommentDto dto, string userId) => new Comment
        {
            Content = dto.Content.Trim(),
            PostId = dto.PostId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        #endregion
    }
}

