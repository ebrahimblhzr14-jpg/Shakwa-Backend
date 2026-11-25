using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IRefreshTokensRepository
    {
        Task<int> CreateAsync(
            RefreshToken refreshToken);

        Task<RefreshToken?> GetByTokenAsync(
            string token);

        Task<bool> RevokeAsync(int id);
    }
}