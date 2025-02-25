using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Host.Services
{
    /// <summary>
    /// 在根目录生成一个密码
    /// </summary>
    public class AuthService
    {
        public string GenerateToken()
        {
            var token = new JwtSecurityToken(
                issuer:"cosmic_dotnet",
                claims: [new Claim(ClaimTypes.NameIdentifier, "admin")],
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthService.GetAuthCode())), SecurityAlgorithms.HmacSha256));
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        public string CheckAuth(string pwd)
        {
            if (pwd == AuthCode)
                return GenerateToken();

            return "error";
        }

        public static string? AuthCode { get; set; }
        public static string GetAuthCode()
        {
            if (!string.IsNullOrEmpty(AuthCode))
                return AuthCode;

            var rootPwd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "authentication");
            if (!File.Exists(rootPwd))
            {
                AuthCode = Guid.NewGuid().ToString();
                File.WriteAllText(rootPwd, AuthCode);
            }
            else
                AuthCode = File.ReadAllText(rootPwd);

            return AuthCode;

        }
    }

    public record AuthModel(string Password);
}
