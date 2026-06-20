using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class EducationDto
    {
        public int EducationId { get; set; }
        public string Degree { get; set; } = null!;
        public string Institution { get; set; } = null!;
        public int? YearOfPassing { get; set; }
    }
}
