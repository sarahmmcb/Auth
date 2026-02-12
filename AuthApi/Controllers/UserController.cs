using Auth.Contracts;
using Auth.Contracts.RequestContracts;
using Auth.Contracts.ResponseContracts;
using AuthApi.Data;
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
        private ILogger<UserController> _logger;

        public UserController(
            UserDbContext context,
            IUserService userService,
            IVerificationCodeService verification,
            ILogger<UserController> logger)
        {
            _context = context;
            _userService = userService;
            _verificationCodeService = verification;
            _logger = logger;
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
                _logger.LogInformation($"GET: User not found with id: {id}");
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
                _logger.LogInformation($"GET: User not found with username: {username}");
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
                _logger.LogWarning($"PUT: User not found with id: {id}");
                return NotFound();
            }

            _context.Entry(userToUpdate).CurrentValues.SetValues(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"Update User (Id: {id}) Concurrency Exception: {ex.Message}");
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
                    _logger.LogError($"Application Exception on register for username {user.Username}: {ex.Message}");
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error on register for username {user.Username}: {ex.Message}");
                    return StatusCode(500, "An Internal Error occurred");
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
                _logger.LogWarning($"DELETE: User not found with id: {id}");
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
                _logger.LogError($"Error sending verificaiton code to {email}");
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
