using Application.Core.Login;
using Application.Host.Models;

namespace Application.Host.Services
{
    public class WebUserService
    {
        readonly MasterServer _server;

        public WebUserService(MasterServer server)
        {
            _server = server;
        }

        public WebUserInfo? GetUserInfo(int id)
        {
            var acc = _server.AccountManager.Find(id);
            return new WebUserInfo
            {
                UserId = acc.Id,
                RealName = string.IsNullOrEmpty(acc.NickName) ? acc.Name : acc.NickName,
                Roles = [acc.GetRole()]
            };
        }
    }
}
