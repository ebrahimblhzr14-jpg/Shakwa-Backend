using ComplaintManagement.Access.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IPrioritiesRepository
    {
        Task<int> CreateAsync(Priority priority);

        Task<IEnumerable<Priority>> GetAllAsync();

        Task<Priority?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(Priority priority);

        Task<bool> DeleteAsync(int id);

        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);
    }
}
