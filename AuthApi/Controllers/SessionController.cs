using Microsoft.AspNetCore.Mvc;
using Auth.Contracts.RequestContracts;
using AuthApi.Logic;
using Auth.Contracts.ResponseContracts;
using Azure.Core;

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

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };

                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

                return Ok(new TokenResponse { Token = token });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return Problem("An internal error occurred", statusCode: 500);
            }
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (token, refreshToken) = await _sessionService.RefreshAccessToken(request.UserId, request.RefreshToken).ConfigureAwait(false);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };

                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

                return Ok(new TokenResponse { Token = token });
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
    }
}
