using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Application.Host.Controllers
{
    [Route("api/auth/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public TokenModel? Login([FromBody] LoginForm loginForm)
        {
            return _authService.Login(loginForm);
        }

        [HttpPost]
        public ActionResult<TokenModel> Refresh()
        {
            var userId = User.Identity.GetIntValue("Refresh");
            if (userId == 0)
            {
                return Unauthorized();
            }
            var token = _authService.LoginById(userId);
            if (token == null)
            {
                return Unauthorized();
            }
            return token;
        }
    }
}
