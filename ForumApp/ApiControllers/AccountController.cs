using ForumApp.DTOs.Auth;
using ForumApp.DTOs.Error;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ForumApp.ApiControllers
{
    /// <summary>
    /// API controller for user registration and authentication, and account management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user and generates an email confirmation link.
        /// </summary>
        /// <param name="model">Registration details (email, password).</param>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = 400,
                    Error = "VALIDATION_FAILED",
                    Details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = 400,
                    Error = "REGISTRATION_FAILED",
                    Details = result.Errors.Select(e => e.Description)
                });
            }

            // Generate confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);

            _logger.LogInformation("Confirmation link for {Email}: {Link}", user.Email, confirmationLink);

            // For demo purposes, return the link in response
            return Ok(new
            {
                Message = "User registered successfully. Please confirm your email.",
                ConfirmationLink = confirmationLink
            });
        }

        /// <summary>
        /// Confirms user email
        /// </summary>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid user");
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Email confirmed successfully" });
            }

            return BadRequest(new ErrorResponseDto
            {
                Code = 400,
                Error = "EMAIL_CONFIRMATION_FAILED",
                Details = result.Errors.Select(e => e.Description)
            });
        }

        /// <summary>
        /// Logd in s user
        /// </summary>
        /// <param name="model">Login details (email, password, rememberMe).</param>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = 400,
                    Error = "VALIDATION_FAILED",
                    Details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.EmailConfirmed)
            {
                return Unauthorized(new ErrorResponseDto
                {
                    Code = 401,
                    Error = "INVALID_LOGIN",
                    Details = new[] { "Invalid login attempt: user doesn't exist or email is not confirmed" }
                });
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized(new ErrorResponseDto 
                { 
                    Code = 401, 
                    Error = "INVALID_LOGIN", 
                    Details = new[] { "Invalid login attempt: incorrect password" } 
                });
            }

            _logger.LogInformation("User {Email} logged in", model.Email);

            // NOTE: For demo purposes returns UserId. In production, issue a JWT token.
            return Ok(new { Message = "Login successful", UserId = user.Id });
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out");

            return Ok(new { message = "Logout successful" });
        }
    }
}

