using Microsoft.AspNetCore.Mvc;
using Auth.Contracts.RequestContracts;
using AuthApi.Logic;
using Auth.Contracts.ResponseContracts;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Logging;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController(ISessionService _sessionService) : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (authToken, refreshToken) = await _sessionService.Login(request.UserName, request.Password, token).ConfigureAwait(false);

                SetCookies(refreshToken, request.UserName);

                AuthLogger.LogInformation<SessionController>($"Login successful for {request.UserName}");
                return Ok(new TokenResponse { Token = authToken });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                AuthLogger.LogError<SessionController>($"Login failed for {request.UserName}");
                return Unauthorized("Username or Password is incorrect");
            }
        }

        [HttpGet]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken token)
        {
            try
            {
                HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
                HttpContext.Request.Cookies.TryGetValue("userName", out var userName);

                var (authToken, newRefreshToken) = await _sessionService.RefreshAccessToken(userName!, refreshToken!, token).ConfigureAwait(false);

                SetCookies(newRefreshToken, userName!);

                return Ok(new TokenResponse { Token = authToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken token)
        {
            var user = HttpContext.User;

            try
            {
                await _sessionService.Logout(user.Claims, token);
            }
            catch(ApplicationException ex)
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
