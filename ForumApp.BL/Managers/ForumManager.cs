using ForumApp.DAL.DbManagers;
using ForumApp.Data;

namespace ForumApp.BL.Managers
{
    /// <summary>
    /// Business logic manager for Forum operations
    /// </summary>
    public class ForumManager
    {
        private readonly IForumsDB _forumsDB;

        public ForumManager(IForumsDB forumsDB)
        {
            _forumsDB = forumsDB;
        }

        /// <summary>
        /// Gets all forums
        /// </summary>
        public async Task<List<Forum>> GetAllForumsAsync()
        {
            return await _forumsDB.GetAllForumsAsync();
        }

        /// <summary>
        /// Gets a forum by ID
        /// </summary>
        public async Task<Forum?> GetForumByIdAsync(int id)
        {
            return await _forumsDB.GetForumByIdAsync(id);
        }

        /// <summary>
        /// Creates a new forum (used for initialization)
        /// </summary>
        public async Task<Forum> CreateForumAsync(Forum forum)
        {
            return await _forumsDB.CreateForumAsync(forum);
        }
    }
}

