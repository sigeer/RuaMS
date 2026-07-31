using Application.Host.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;
using System.Security.Claims;

namespace Application.Host.Middlewares
{
    public class UserAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.Identity.GetUserId();
            if (userIdClaim == 0)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 角色检查（支持多个角色）
            if (!string.IsNullOrEmpty(Roles))
            {
                var requiredRoles = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(r => r.Trim());
                var userRole = context.HttpContext.User.Identity.GetStringValue(ClaimTypes.Role);
                if (!requiredRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                {
                    context.Result = new ForbidResult(); // 权限不足应返回 403
                }
            }
        }
    }
}
