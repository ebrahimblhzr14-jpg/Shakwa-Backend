using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ComplainsSystemAPI.Authorization
{
    public class OwnerOrSuperAdminHandler : AuthorizationHandler<OwnerOrSuperAdminRequirement,int>
    {
        protected override Task HandleRequirementAsync(
       AuthorizationHandlerContext context,
       OwnerOrSuperAdminRequirement requirement,
       int UserId)
        {
            // Admin override
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Ownership check
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userId, out int authenticatedUserId) &&
                authenticatedUserId == UserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
