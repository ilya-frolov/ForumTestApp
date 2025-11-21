using ForumApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.DAL.DbManagers
{
    /// <summary>
    /// Repository implementation for Comment data access
    /// </summary>
    public class CommentsDB : ICommentsDB
    {
        private readonly ForumDbContext _context;

        public CommentsDB(ForumDbContext context)
        {
            _context = context;
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return false;

            comment.IsDeleted = true;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsCommentOwnerAsync(int commentId, string userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            return comment != null && !comment.IsDeleted && comment.CreatedByUserId == userId;
        }
    }
}

