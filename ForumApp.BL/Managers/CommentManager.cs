using ForumApp.DAL.DbManagers;
using ForumApp.Data;

namespace ForumApp.BL.Managers
{
    /// <summary>
    /// Business logic manager for Comment operations
    /// </summary>
    public class CommentManager
    {
        private readonly ICommentsDB _commentsDB;
        private readonly IPostsDB _postsDB;

        public CommentManager(ICommentsDB commentsDB, IPostsDB postsDB)
        {
            _commentsDB = commentsDB;
            _postsDB = postsDB;
        }

        /// <summary>
        /// Gets all comments for a post
        /// </summary>
        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _commentsDB.GetCommentsByPostIdAsync(postId);
        }

        /// <summary>
        /// Gets a comment by ID
        /// </summary>
        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _commentsDB.GetCommentByIdAsync(id);
        }

        /// <summary>
        /// Creates a new comment
        /// </summary>
        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            return await _commentsDB.CreateCommentAsync(comment);
        }

        /// <summary>
        /// Deletes a comment if the user is the post owner
        /// </summary>
        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            // Get the comment to find the post
            var comment = await _commentsDB.GetCommentByIdAsync(commentId);
            if (comment == null)
                return false;

            // Get the post to check ownership
            var post = await _postsDB.GetPostByIdAsync(comment.PostId);
            if (post == null)
                return false;

            // Only the post creator can delete comments
            if (post.CreatedByUserId != userId)
                return false;

            return await _commentsDB.DeleteCommentAsync(commentId);
        }

        /// <summary>
        /// Checks if a user can delete a comment (must be post owner)
        /// </summary>
        public async Task<bool> CanDeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _commentsDB.GetCommentByIdAsync(commentId);
            if (comment == null)
                return false;

            var post = await _postsDB.GetPostByIdAsync(comment.PostId);

            return post != null && post.CreatedByUserId == userId;
        }
    }
}

