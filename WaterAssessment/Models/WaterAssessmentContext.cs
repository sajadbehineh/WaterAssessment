using Microsoft.EntityFrameworkCore;
using WaterAssessment.Mapping;

namespace WaterAssessment.Models;

public class WaterAssessmentContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(local);Database=WaterAssessmentDB;Trusted_Connection=True;Encrypt=False;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AssessmentMap());
        modelBuilder.ApplyConfiguration(new Assessment_EmployeeMap());
        modelBuilder.ApplyConfiguration(new LocationMap());

        modelBuilder.Entity<Area>()
            .HasKey(a => a.AreaID);

        modelBuilder.Entity<FormValue>()
            .HasKey(f => f.FormValueID);

        modelBuilder.Entity<FormValue>()
            .HasOne(p => p.Assessment)
            .WithMany(t => t.FormValues)
            .HasForeignKey(f => f.AssessmentID);

        modelBuilder.Entity<Employee>()
            .HasKey(e => e.EmployeeID);

        modelBuilder.Entity<CurrentMeter>()
            .HasKey(c => c.CurrentMeterID);

        modelBuilder.Entity<Propeller>()
            .HasKey(p => p.PropellerID);
    }

    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Assessment_Employee> AssessmentEmployees { get; set; }
    public DbSet<CurrentMeter> CurrentMeters { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<FormValue> FormValues { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Propeller> Propellers { get; set; }
}