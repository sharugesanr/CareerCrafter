using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<List<User>> GetAllUsersAsync();

        Task<List<JobListing>> GetAllJobsAsync();

        Task<int> CountUsersByRoleAsync(string role);

        Task<int> CountActiveJobsAsync();

        Task<int> CountTotalApplicationsAsync();
    }
}
