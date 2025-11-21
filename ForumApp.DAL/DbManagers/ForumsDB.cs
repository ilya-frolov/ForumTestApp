using ForumApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.DAL.DbManagers
{
    /// <summary>
    /// Repository implementation for Forum data access
    /// </summary>
    public class ForumsDB : IForumsDB
    {
        private readonly ForumDbContext _context;

        public ForumsDB(ForumDbContext context)
        {
            _context = context;
        }

        public async Task<List<Forum>> GetAllForumsAsync()
        {
            return await _context.Forums
                .Include(f => f.Posts.Where(p => !p.IsDeleted))
                .ToListAsync();
        }

        public async Task<Forum?> GetForumByIdAsync(int id)
        {
            return await _context.Forums
                .Include(f => f.Posts.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Forum> CreateForumAsync(Forum forum)
        {
            _context.Forums.Add(forum);
            await _context.SaveChangesAsync();

            return forum;
        }
    }
}

