using ComplaintManagement.Access.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IUsersRepository
    {
        Task<int> CreateAsync(User user);

        Task<IEnumerable<User>> GetAllAsync();

        Task<User?> GetByIdAsync(int id);

        Task<User?> GetByEmailAsync(string email);

        Task<bool> UpdateAsync(User user);

        Task<bool> PutDepartmentAsync(int UserID,int DepID);

        Task<bool> UpdatePasswordAsync(int id, string passwordHash);

        //Task<bool> DeleteAsync(int id);

        Task<bool> DeactivateAsync(int id);

        Task<bool> ActivateAsync(int id);


    }
}
