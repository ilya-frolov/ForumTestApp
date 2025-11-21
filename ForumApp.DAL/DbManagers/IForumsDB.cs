using ForumApp.Data;

namespace ForumApp.DAL.DbManagers
{
    /// <summary>
    /// Repository interface for Forum data access
    /// </summary>
    public interface IForumsDB
    {
        Task<List<Forum>> GetAllForumsAsync();

        Task<Forum?> GetForumByIdAsync(int id);

        Task<Forum> CreateForumAsync(Forum forum);
    }
}

