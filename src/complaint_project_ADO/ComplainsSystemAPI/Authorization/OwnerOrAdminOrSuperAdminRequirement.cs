using Microsoft.AspNetCore.Authorization;

namespace ComplainsSystemAPI.Authorization
{
    public class OwnerOrAdminOrSuperAdminRequirement : IAuthorizationRequirement
    {
    }
}
