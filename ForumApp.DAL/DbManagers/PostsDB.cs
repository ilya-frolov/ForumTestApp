using ForumApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ForumApp.DAL.DbManagers
{
    /// <summary>
    /// Repository implementation for Post data access
    /// </summary>
    public class PostsDB : IPostsDB
    {
        private readonly ForumDbContext _context;

        public PostsDB(ForumDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetPostsByForumIdAsync(int forumId, int skip = 0, int take = 10)
        {
            return await _context.Posts
                .Where(p => p.ForumId == forumId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(p => p.Comments)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Forum)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<Post> UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<List<Post>> GetFirstPostsAsync(int count = 10)
        {
            return await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Include(p => p.Forum)
                .Include(p => p.Comments)
                .ToListAsync();
        }
    }
}

