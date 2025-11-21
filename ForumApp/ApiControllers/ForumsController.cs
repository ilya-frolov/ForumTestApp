using ForumApp.BL.Managers;
using ForumApp.DTOs.Forum;
using ForumApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumApp.ApiControllers
{
    /// <summary>
    /// API controller for Forum operations
    /// Provides endpoints to retrieve forums.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ForumsController : ControllerBase
    {
        private readonly ForumManager _forumManager;

        public ForumsController(ForumManager forumManager)
        {
            _forumManager = forumManager;
        }

        /// <summary>
        /// Gets all forums
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ForumDto>>> GetAll()
        {
            var forums = await _forumManager.GetAllForumsAsync();
            var dtos = forums.Select(DtoMapper.MapForumToDto).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Gets a forum by ID
        /// </summary>
        /// <param name="id">Forum ID to retrieve.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ForumDto>> GetById(int id)
        {
            var forum = await _forumManager.GetForumByIdAsync(id);
            if (forum == null)
            {
                return NotFound();
            }

            return Ok(DtoMapper.MapForumToDto(forum));
        }
    }
}

