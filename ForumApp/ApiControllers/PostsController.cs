using ForumApp.BL.Managers;
using ForumApp.DTOs.Error;
using ForumApp.DTOs.Post;
using ForumApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ForumApp.ApiControllers
{
    /// <summary>
    /// API controller for Post operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly PostManager _postManager;
        private readonly ForumManager _forumManager;

        public PostsController(PostManager postManager, ForumManager forumManager)
        {
            _postManager = postManager;
            _forumManager = forumManager;
        }

        /// <summary>
        /// Gets the first posts (cached)
        /// </summary>
        /// <param name="count">Number of posts to return.</param>
        [HttpGet("first")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PostDto>>> GetFirst([FromQuery] int count = 10)
        {
            var posts = await _postManager.GetFirstPostsAsync(count);
            var dtos = posts.Select(DtoMapper.MapPostToDto).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Gets posts by forum ID with pagination
        /// </summary>
        /// <param name="forumId">Forum ID to filter posts.</param>
        /// <param name="skip">Number of posts to skip for pagination.</param>
        /// <param name="take">Number of posts to take for pagination.</param>
        [HttpGet("forum/{forumId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PostDto>>> GetByForumId(int forumId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            // Verify forum exists
            var forum = await _forumManager.GetForumByIdAsync(forumId);
            if (forum == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = 404,
                    Error = "FORUM_NOT_FOUND",
                    Details = new[] { $"Forum with ID {forumId} not found" }
                });
            }

            var posts = await _postManager.GetPostsByForumIdAsync(forumId, skip, take);
            var dtos = posts.Select(DtoMapper.MapPostToDto).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Gets a post by ID
        /// </summary>
        /// <param name="id">Post ID to retrieve.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var post = await _postManager.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = 404,
                    Error = "POST_NOT_FOUND",
                    Details = new[] { $"Post with ID {id} not found" }
                });
            }

            return Ok(DtoMapper.MapPostToDto(post));
        }

        /// <summary>
        /// Creates a new post
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PostDto>> Create(CreatePostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Extra server-side validation (defensive)
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length > 500)
                return BadRequest("Title must be between 1 and 500 characters.");

            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 4000)
                return BadRequest("Content must be between 1 and 4000 characters.");

            // Verify forum exists
            var forum = await _forumManager.GetForumByIdAsync(dto.ForumId);
            if (forum == null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = 400,
                    Error = "FORUM_NOT_FOUND",
                    Details = new[] { $"Forum with ID {dto.ForumId} not found" }
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var createdPost = await _postManager.CreatePostAsync(DtoMapper.MapCreatePostDtoToEntity(dto, userId));
            var resultDto = DtoMapper.MapPostToDto(createdPost);

            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        /// <summary>
        /// Updates a post (only by owner)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreatePostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length > 500)
                return BadRequest("Title must be between 1 and 500 characters.");

            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 4000)
                return BadRequest("Content must be between 1 and 4000 characters.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var post = await _postManager.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = 404,
                    Error = "POST_NOT_FOUND",
                    Details = new[] { $"Post with ID {id} not found" }
                });
            }

            if (post.CreatedByUserId != userId)
            {
                return Forbid();
            }

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _postManager.UpdatePostAsync(post);

            return NoContent();
        }
    }
}

