using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ComplainsSystemAPI.Authorization
{
    public class OwnerOrAdminOrSuperAdminHandler : AuthorizationHandler<OwnerOrAdminOrSuperAdminRequirement, int>
    {
        protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      OwnerOrAdminOrSuperAdminRequirement requirement,
      int UserId)
        {
            // Admin override
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Ownership check
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = context.User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("User Claims: " + string.Join(", ", claims));

            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? context.User.FindFirstValue("sub");

            Console.WriteLine($"Token UserId: '{userIdClaim}' | Complaint OwnerId: '{UserId}'");
            if (int.TryParse(userId, out int authenticatedUserId) &&
                authenticatedUserId == UserId)
            {
                context.Succeed(requirement);
            }
            else
            {
                Console.WriteLine("Authorization Failed: User IDs do not match or parse failed.");
            }

            return Task.CompletedTask;
        }
    }
}
