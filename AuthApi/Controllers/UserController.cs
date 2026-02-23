using Auth.Contracts;
using Auth.Contracts.RequestContracts;
using Auth.Contracts.ResponseContracts;
using AuthApi.Data;
using AuthApi.Logging;
using AuthApi.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IUserService _userService;
        private readonly IVerificationCodeService _verificationCodeService;

        public UserController(
            UserDbContext context,
            IUserService userService,
            IVerificationCodeService verification)
        {
            _context = context;
            _userService = userService;
            _verificationCodeService = verification;
        }

        [HttpGet]
        [Route("all")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.User.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                AuthLogger.LogInformation<UserController>($"GET: User not found with id: {id}");
                return NotFound();
            }

            user.Password = "";

            return user;
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetUser([FromQuery] string username)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => string.Equals(u.UserName, username));

            if (user == null)
            {
                AuthLogger.LogInformation<UserController>($"GET: User not found with username: {username}");
                return NotFound();
            }

            user.Password = "";

            return user;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserRequest user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            var userToUpdate = await _context.User.FindAsync(id);
            if (userToUpdate == null)
            {
                AuthLogger.LogWarning<UserController>($"PUT: User not found with id: {id}");
                return NotFound();
            }

            _context.Entry(userToUpdate).CurrentValues.SetValues(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                AuthLogger.LogError<UserController>($"Update User (Id: {id}) Concurrency Exception: {ex.Message}");
                return Problem("An internal error occurred, please try again later", statusCode: (int)HttpStatusCode.InternalServerError);
            }

            return NoContent();
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UpdateUserRequest user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.RegisterUser(user).ConfigureAwait(false);
                }
                catch (ApplicationException ex)
                {
                    AuthLogger.LogError<UserController>($"Application Exception on register for username {user.Username}: {ex.Message}");
                    return BadRequest(ex.Message);
                }

                return NoContent();
            }

            return BadRequest("The request was invalid");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                AuthLogger.LogWarning<UserController>($"DELETE: User not found with id: {id}");
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("sendCode")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationCode([FromQuery] string email)
        {
            var sendSuccess = await _verificationCodeService.SendVerificationCodeByEmail(email).ConfigureAwait(false);

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
        public async Task<IActionResult> ValidateVerificationCode([FromQuery] string email, [FromQuery] string code)
        {
            var token = await _verificationCodeService.ValidateVerificationCode(email, code);

            return Ok(new TokenResponse { Token = token });
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _userService.UpdatePassword(request).ConfigureAwait(false);

            return Ok("Password updated successfully");
        }
    }
}
