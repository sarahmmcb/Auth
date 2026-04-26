using Auth.Contracts;
using Auth.Contracts.RequestContracts;
using Auth.Contracts.ResponseContracts;
using AuthApi.Logging;
using AuthApi.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVerificationCodeService _verificationCodeService;

        public UserController(
            IUserService userService,
            IVerificationCodeService verification)
        {
            _userService = userService;
            _verificationCodeService = verification;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id, CancellationToken token)
        {
            var user = await _userService.GetUserByUserId(id, token);

            if (user == null)
            {
                AuthLogger.LogInformation<UserController>($"GET: User not found with id: {id}");
                return NotFound();
            }

            user.Password = "[REDACTED]"; // TODO: Implement some kind of Middleware for this

            return user;
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetUser([FromQuery] string username, CancellationToken token)
        {
            var user = await _userService.GetUserByUserName(username, token);

            if (user == null)
            {
                AuthLogger.LogInformation<UserController>($"GET: User not found with username: {username}");
                return NotFound();
            }

            user.Password = "[REDACTED]";

            return user;
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest user, CancellationToken token)
        {
            try
            {
                await _userService.UpdateUser(user, token);
            }
            catch (Exception ex)
            {
                AuthLogger.LogError<UserController>($"Error on updating User: {ex.Message}");
                return NotFound();
            }

            return NoContent();
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CreateUserRequest user, CancellationToken token)
        {
            if (ModelState.IsValid)
            {
                var newUserId = 0;
                try
                {
                   newUserId = await _userService.RegisterUser(user, token).ConfigureAwait(false);
                }
                catch (ApplicationException ex)
                {
                    AuthLogger.LogError<UserController>($"Application Exception on register for username {user.Username}: {ex.Message}");
                    return Problem(ex.Message);
                }

                return Ok(newUserId);
            }

            return BadRequest("The request was invalid");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken token)
        {
            var user = await _userService.GetUserByUserId(id, token);
            if (user == null)
            {
                AuthLogger.LogWarning<UserController>($"DELETE: User not found with id: {id}");
                return NotFound();
            }

            await _userService.DeleteUser(id, token); // TODO: get UpdateUserId

            return NoContent();
        }

        [HttpPost("sendCode")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationCode([FromQuery] string email, CancellationToken token)
        {
            var sendSuccess = await _verificationCodeService.SendVerificationCodeByEmail(email, token).ConfigureAwait(false);

            if (sendSuccess)
            {
                return Ok();
            }
            else
            {
                AuthLogger.LogError<UserController>($"Error sending verificaiton code to {email}");
                return Problem("There was a problem sending the verification code. Please check the email address and try again.");
            }
        }

        [HttpPost("validateCode")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateVerificationCode([FromQuery] string email, [FromQuery] string code, CancellationToken token)
        {
            var authToken = await _verificationCodeService.ValidateVerificationCode(email, code, token);

            return Ok(new TokenResponse { Token = authToken });
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UpdatePasswordRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _userService.UpdatePassword(request, token).ConfigureAwait(false);

            return Ok("Password updated successfully");
        }
    }
}
