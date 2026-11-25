using ComplaintManagement.Access.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IDepartmentsRepository
    {
        Task<int> CreateAsync(Department department);

        Task<IEnumerable<Department>> GetAllAsync();

        Task<Department?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(Department department);

        Task<bool> DeleteAsync(int id);
    }
}
