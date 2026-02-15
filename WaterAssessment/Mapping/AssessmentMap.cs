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
                .Property(p => p.Echelon).IsRequired(false);

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
                .HasForeignKey(f => f.CurrentMeterID)
                .IsRequired(false);

            builder
                .HasOne(p => p.Propeller)
                .WithMany(t => t.Assessments)
                .HasForeignKey(f => f.PropellerID)
                .IsRequired(false);

            builder.HasMany(a => a.GateOpenings)
                .WithOne(g => g.Assessment)
                .HasForeignKey(g => g.AssessmentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
