using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IEmployerRepository
    {
        Task<EmployerProfile?> GetByUserIdAsync(int userId);
        Task UpdateProfileAsync(EmployerProfile profile);
        Task SaveChangesAsync();
    }
}
