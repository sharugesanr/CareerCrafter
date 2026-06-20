using CareerCrafter.Core.Models;
using CareerCrafter.Data;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Implementations
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly AppDbContext _context;

        public EmployerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployerProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.EmployerProfiles
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task UpdateProfileAsync(EmployerProfile profile)
        {
            _context.EmployerProfiles.Update(profile);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
