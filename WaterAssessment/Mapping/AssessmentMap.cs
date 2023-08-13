using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WaterAssessment.Mapping
{
    public class AssessmentMap : IEntityTypeConfiguration<Assessment>
    {
        public void Configure(EntityTypeBuilder<Assessment> builder)
        {
            builder
                .HasKey(a => a.AssessmentID);
            builder
                .Property(p => p.Timer).IsRequired();
            builder
                .Property(p => p.Echelon).IsRequired();
            builder
                .Property(p => p.Openness).IsRequired();
            builder
                .Property(p => p.IsCanal).IsRequired();
            builder
                .Property(b => b.Inserted)
                .HasDefaultValueSql("getdate()");
            builder
                .HasOne(p => p.Location)
                .WithMany(t => t.Assessments)
                .HasForeignKey(f => f.LocationID);
            builder
                .HasOne(p => p.CurrentMeter)
                .WithMany(t => t.Assessments)
                .HasForeignKey(f => f.CurrentMeterID);
            builder
                .HasOne(p => p.Propeller)
                .WithMany(t => t.Assessments)
                .HasForeignKey(f => f.PropellerID);
        }
    }
}
