using Application.Core.Login;
using Application.Host.Models;
using Application.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Host.Controllers
{
    public class UserController : BaseApiController
    {
        MasterServer _server;
        WebUserService _userService;

        public UserController(MasterServer server, WebUserService userService)
        {
            _server = server;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<WebUserInfo> Info()
        {
            var model = _userService.GetUserInfo(User.Identity.GetUserId());
            if (model == null)
            {
                return Unauthorized();
            }

            return model;
        }

        [HttpPost]
        public bool CreateAccount([FromBody]LoginForm form )
        {
            _server.AccountManager.CreateAccount(form.UserName, form.Password);
            return true;
        }
    }
}
