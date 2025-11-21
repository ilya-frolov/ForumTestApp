using ForumApp.Data;

namespace ForumApp.DAL.DbManagers
{
    /// <summary>
    /// Repository interface for Post data access
    /// </summary>
    public interface IPostsDB
    {
        Task<List<Post>> GetPostsByForumIdAsync(int forumId, int skip = 0, int take = 10);

        Task<Post?> GetPostByIdAsync(int id);

        Task<Post> CreatePostAsync(Post post);

        Task<Post> UpdatePostAsync(Post post);

        Task<List<Post>> GetFirstPostsAsync(int count = 10);
    }
}

