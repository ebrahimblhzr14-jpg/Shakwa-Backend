using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ComplaintManagement.Access.Models;
using System.Threading.Tasks;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IRolesRepository
    {
        Task<int> CreateAsync(Role role);

        Task<IEnumerable<Role>> GetAllAsync();

        Task<Role?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(Role role);

        Task<bool> DeleteAsync(int id);
    }
}
