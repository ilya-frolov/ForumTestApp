using ForumApp.BL.Managers;
using ForumApp.DTOs.Comment;
using ForumApp.DTOs.Error;
using ForumApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ForumApp.ApiControllers
{
    // <summary>
    /// API controller for Comment operations.
    /// Provides endpoints to view, create, and delete comments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly CommentManager _commentManager;
        private readonly PostManager _postManager;

        public CommentsController(CommentManager commentManager, PostManager postManager)
        {
            _commentManager = commentManager;
            _postManager = postManager;
        }

        /// <summary>
        /// Gets all comments for a post
        /// </summary>
        /// <param name="postId">Post ID to retrieve comments for.</param>
        [HttpGet("post/{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CommentDto>>> GetAllByPostId(int postId)
        {
            // Verify post exists
            var post = await _postManager.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = 404,
                    Error = "POST_NOT_FOUND",
                    Details = new[] { $"Post with ID {postId} not found" }
                });
            }

            var comments = await _commentManager.GetCommentsByPostIdAsync(postId);
            var dtos = comments.Select(DtoMapper.MapCommentToDto).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Gets a comment by ID
        /// </summary>
        /// <param name="id">Comment ID to retrieve.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CommentDto>> GetById(int id)
        {
            var comment = await _commentManager.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = 404,
                    Error = "COMMENT_NOT_FOUND",
                    Details = new[] { $"Comment with ID {id} not found" }
                });
            }

            return Ok(DtoMapper.MapCommentToDto(comment));
        }

        /// <summary>
        /// Creates a new comment
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CommentDto>> Create(CreateCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 1000)
                return BadRequest(new ErrorResponseDto
                {
                    Code = 400,
                    Error = "VALIDATION_FAILED",
                    Details = new[] { "Content must be between 1 and 1000 characters." }
                });

            // Verify post exists
            var post = await _postManager.GetPostByIdAsync(dto.PostId);
            if (post == null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = 400,
                    Error = "POST_NOT_FOUND",
                    Details = new[] { $"Post with ID {dto.PostId} not found" }
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var createdComment = await _commentManager.CreateCommentAsync(DtoMapper.MapCreateCommentDtoToEntity(dto, userId));
            var resultDto = DtoMapper.MapCommentToDto(createdComment);

            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        /// <summary>
        /// Deletes a comment (only by post owner)
        /// </summary>
        /// <param name="id">Comment ID to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var comment = await _commentManager.GetCommentByIdAsync(id);
            if (comment == null) return NotFound(new ErrorResponseDto
            {
                Code = 404,
                Error = "COMMENT_NOT_FOUND",
                Details = new[] { $"Comment with ID {id} not found" }
            });

            var post = await _postManager.GetPostByIdAsync(comment.PostId);
            if (post == null) return NotFound(new ErrorResponseDto
            {
                Code = 404,
                Error = "POST_NOT_FOUND",
                Details = new[] { $"Post with ID {comment.PostId} not found" }
            });

            // Enforce requirement: only the post creator can delete comments
            if (post.CreatedByUserId != userId)
            {
                return StatusCode(403, new ErrorResponseDto
                {
                    Code = 403,
                    Error = "FORBIDDEN",
                    Details = new[] { "Only the post creator can delete comments" }
                });
            }

            await _commentManager.DeleteCommentAsync(id, userId);

            return NoContent();
        }
    }
}

