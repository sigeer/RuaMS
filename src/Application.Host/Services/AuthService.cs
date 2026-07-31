using Application.Core.Login;
using Application.Host.Models;

namespace Application.Host.Services
{
    /// <summary>
    /// 在根目录生成一个密码
    /// </summary>
    public class AuthService
    {
        public const string IssuerSigningKey = "38395FD9-2E84-452B-8244-C5D78B175BCA";
        readonly MasterServer _server;

        public AuthService(MasterServer server)
        {
            _server = server;
        }

        public TokenModel? Login(LoginForm loginForm)
        {
            var acc = _server.AccountManager.Find(x => x.Name == loginForm.UserName, x => x.Name.Equals(loginForm.UserName, StringComparison.OrdinalIgnoreCase));
            if (acc == null)
                return null;
            if (acc.Password.Equals(loginForm.Password,  StringComparison.OrdinalIgnoreCase))
            {
                return TokenModel.CreateToken(
                    2 * 3600,
                    30 * 24 * 3600, 
                    "ruams", 
                    _server.Name, 
                    acc.Id, 
                    acc.GetRole(), 
                    IssuerSigningKey);
            }

            return null;
        }

        public TokenModel? LoginById(int accId)
        {
            var acc = _server.AccountManager.Find(accId);
            if (acc != null)
            {
                return TokenModel.CreateToken(
                    2 * 3600,
                    30 * 24 * 3600,
                    "ruams",
                    _server.Name,
                    acc.Id,
                    acc.GetRole(),
                    IssuerSigningKey);
            }

            return null;
        }
    }
}
