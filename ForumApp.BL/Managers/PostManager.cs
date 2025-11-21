using ForumApp.DAL.DbManagers;
using ForumApp.Data;
using Microsoft.Extensions.Caching.Memory;

namespace ForumApp.BL.Managers
{
    /// <summary>
    /// Business logic manager for Post operations with caching support
    /// </summary>
    public class PostManager
    {
        private readonly IPostsDB _postDB;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

        public PostManager(IPostsDB postsDB, IMemoryCache cache)
        {
            _postDB = postsDB;
            _cache = cache;
        }

        private string GetCacheKey(int count) => $"FirstPosts_{count}";

        /// <summary>
        /// Gets posts by forum ID with pagination
        /// </summary>
        public async Task<List<Post>> GetPostsByForumIdAsync(int forumId, int skip = 0, int take = 10)
        {
            return await _postDB.GetPostsByForumIdAsync(forumId, skip, take);
        }

        /// <summary>
        /// Gets a post by ID
        /// </summary>
        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _postDB.GetPostByIdAsync(id);
        }

        /// <summary>
        /// Creates a new post and invalidates cache
        /// </summary>
        public async Task<Post> CreatePostAsync(Post post)
        {
            var result = await _postDB.CreatePostAsync(post);
            InvalidateCache();

            return result;
        }

        /// <summary>
        /// Updates a post and invalidates cache
        /// </summary>
        public async Task<Post> UpdatePostAsync(Post post)
        {
            post.UpdatedAt = DateTime.UtcNow;
            var result = await _postDB.UpdatePostAsync(post);
            InvalidateCache();

            return result;
        }

        /// <summary>
        /// Gets the first posts with caching mechanism
        /// </summary>
        public async Task<List<Post>> GetFirstPostsAsync(int count = 10)
        {
            var cacheKey = GetCacheKey(count);

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out List<Post>? cachedPosts) && cachedPosts != null)
                return cachedPosts;

            var posts = await _postDB.GetFirstPostsAsync(count);

            // Store in cache
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CACHE_DURATION,
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _cache.Set(cacheKey, posts, cacheOptions);

            return posts;
        }

        /// <summary>
        /// Invalidates the first posts cache
        /// </summary>
        private void InvalidateCache()
        {
            // If it is needed to clear all variations of count, keys can be tracked separately.
            // For simplicity, remove the default key.
            _cache.Remove(GetCacheKey(10));
        }

        /// <summary>
        /// Checks if a user is the owner of a post
        /// </summary>
        public async Task<bool> IsPostOwnerAsync(int postId, string userId)
        {
            var post = await _postDB.GetPostByIdAsync(postId);

            return post != null && post.CreatedByUserId == userId;
        }
    }
}

