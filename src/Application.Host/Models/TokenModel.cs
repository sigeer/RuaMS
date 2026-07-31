using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Host.Models
{
    public class TokenModel
    {
        public string access_token { get; set; }
        public long expires_in { get; set; }
        public string refresh_token { get; set; }

        public static TokenModel CreateToken(int expireSeconds, int refreshExpireSeconds, string issuer, string audienece, int userId, string role, string issuserSigningKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.Now;
            var expires = now.Add(TimeSpan.FromSeconds(expireSeconds));
            var refreshExpire = now.Add(TimeSpan.FromSeconds(refreshExpireSeconds));
            return new TokenModel
            {
                access_token = tokenHandler.WriteToken(CreateTokenData(now, expires, issuer, audienece, GetAccessClaims(userId, role), issuserSigningKey)),
                expires_in = new DateTimeOffset(expires).ToUnixTimeMilliseconds(),
                refresh_token = tokenHandler.WriteToken(CreateTokenData(now, refreshExpire, issuer, audienece, GetRefreshClaims(userId), issuserSigningKey))
            };
        }

        public static Claim[] GetAccessClaims(int userId, string role)
        {
            return new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role) };
        }

        public static Claim[] GetRefreshClaims(int userId)
        {
            return new Claim[] { new Claim("Refresh", userId.ToString()) };
        }

        public static JwtSecurityToken CreateTokenData(DateTime now, DateTime expired, string issuer, string audienece, Claim[] claims, string issuserSigningKey)
        {
            return new JwtSecurityToken(
                issuer: issuer,
                audience: audienece,
                claims: claims,
                notBefore: now,
                expires: expired,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuserSigningKey)), SecurityAlgorithms.HmacSha256));
        }
    }
}
