using Microsoft.AspNetCore.Mvc;
using Auth.Contracts.RequestContracts;
using AuthApi.Logic;
using Auth.Contracts.ResponseContracts;
using Microsoft.AspNetCore.Authorization;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController(ISessionService _sessionService) : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (token, refreshToken) = await _sessionService.Login(request.UserName, request.Password).ConfigureAwait(false);

                SetCookies(refreshToken, request.UserName);

                return Ok(new TokenResponse { Token = token });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized("Username or Password is incorrect");
            }
            catch(Exception ex)
            {
                return Problem("An internal error occurred", statusCode: 500);
            }
        }

        [HttpGet]
        [Route("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
                HttpContext.Request.Cookies.TryGetValue("userName", out var userName);

                var (token, newRefreshToken) = await _sessionService.RefreshAccessToken(userName, refreshToken).ConfigureAwait(false);

                SetCookies(newRefreshToken, userName);

                return Ok(new TokenResponse { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem("An internal error occurred", statusCode: 500);
            }
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = HttpContext.User;

            try
            {
                await _sessionService.Logout(user.Claims);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        private void SetCookies(string newRefreshToken, string userName)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Domain = "localhost",
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            // return username in cookie for auto-auth on front end refresh
            var usernameCookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                Domain = "localhost",
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.MaxValue
            };

            Response.Cookies.Append("userName", userName, usernameCookieOptions);
        }
    }
}
