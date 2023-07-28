using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WaterAssessment.Mapping
{
    public class Assessment_EmployeeMap : IEntityTypeConfiguration<Assessment_Employee>
    {
        public void Configure(EntityTypeBuilder<Assessment_Employee> builder)
        {
            builder
                .HasKey(t => new { t.AssessmentID, t.EmployeeID });
            builder
                .HasOne(p => p.Assessment)
                .WithMany(t => t.AssessmentEmployees)
                .HasForeignKey(f => f.AssessmentID);
            builder
                .HasOne(p => p.Employee)
                .WithMany(t => t.AssessmentEmployees)
                .HasForeignKey(f => f.EmployeeID);
        }
    }
}
