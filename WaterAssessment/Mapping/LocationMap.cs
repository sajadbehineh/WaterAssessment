using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Location = WaterAssessment.Models.Location;

namespace WaterAssessment.Mapping
{
    public class LocationMap : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder
                .HasKey(l => l.LocationID);
            builder
                .HasOne(p => p.Area)
                .WithMany(t => t.Locations)
                .HasForeignKey(f => f.AreaID);
        }
    }
}
