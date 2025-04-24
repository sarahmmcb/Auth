using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auth.Contracts;
using AuthApi.Data;
using Auth.Contracts.RequestContracts;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Logic;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireAdmin")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IUserService _userService;

        public UserController(UserDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.User.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
                return NotFound();
            }

            _context.Entry(userToUpdate).CurrentValues.SetValues(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(UpdateUserRequest user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.RegisterUser(user).ConfigureAwait(false);
                }
                catch (ApplicationException appEx)
                {
                    return BadRequest(appEx.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "An Internal Error occurred");
                }

                return NoContent();
            }

            return BadRequest("The request was invalid");
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => string.Equals(e.Id,id));
        }
    }
}
