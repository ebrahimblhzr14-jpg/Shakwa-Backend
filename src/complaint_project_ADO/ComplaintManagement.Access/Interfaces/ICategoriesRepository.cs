using ComplaintManagement.Access.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Access.Interfaces
{
    public interface ICategoriesRepository
    {
        Task<int> CreateAsync(ComplaintCategory category);

        Task<IEnumerable<ComplaintCategory>> GetAllAsync();

        Task<ComplaintCategory?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(ComplaintCategory category);

        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);
    }
}
