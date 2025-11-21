using ForumApp.Data;

namespace ForumApp.DAL.DbManagers
{
    /// <summary>
    /// Repository interface for Comment data access
    /// </summary>
    public interface ICommentsDB
    {
        Task<List<Comment>> GetCommentsByPostIdAsync(int postId);

        Task<Comment?> GetCommentByIdAsync(int id);

        Task<Comment> CreateCommentAsync(Comment comment);

        Task<bool> DeleteCommentAsync(int id);

        Task<bool> IsCommentOwnerAsync(int commentId, string userId);
    }
}

