using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using WaterAssessment.Core;
using WaterAssessment.Mapping;

namespace WaterAssessment.Models;

public class WaterAssessmentContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Server=(local);Database=WaterAssessmentDB;Trusted_Connection=True;Encrypt=False;");
            //optionsBuilder.UseLazyLoadingProxies();
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // پر کردن تاریخ و کاربر ویرایش‌کننده
            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                entry.Property("UpdatedByUserID").CurrentValue = AppSession.CurrentUser?.UserID;
            }

            // پر کردن تاریخ و کاربر ایجادکننده (فقط هنگام افزودن)
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.Now;
                    entry.Property("CreatedByUserID").CurrentValue = AppSession.CurrentUser?.UserID;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
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

        modelBuilder.Entity<LocationType>().HasData(
            new LocationType { LocationTypeID = 1, Title = "کانال" },
            new LocationType { LocationTypeID = 2, Title = "زهکش" }
        );

        modelBuilder.Entity<AssessmentPump>()
            .HasOne(ap => ap.Assessment)
            .WithMany(a => a.PumpStates)
            .HasForeignKey(ap => ap.AssessmentID)
            .OnDelete(DeleteBehavior.Cascade); // حذف اندازه گیری منجر به حذف وضعیت پمپ‌ها می‌شود

        modelBuilder.Entity<AssessmentPump>()
            .HasOne(ap => ap.LocationPump)
            .WithMany() // یا اگر در مدل LocationPump لیستی دارید، نام آن را اینجا بگذارید
            .HasForeignKey(ap => ap.LocationPumpID)
            .OnDelete(DeleteBehavior.NoAction); // از حذف آبشاری موازی جلوگیری می‌کند

        modelBuilder.Entity<AssessmentGate>()
            .HasOne(ag => ag.Assessment)
            .WithMany(a => a.GateOpenings)
            .HasForeignKey(ag => ag.AssessmentID)
            .OnDelete(DeleteBehavior.Cascade); // حذف اندازه گیری منجر به حذف ردیف‌های دریچه می‌شود

        modelBuilder.Entity<Location>()
            .HasMany(l => l.LocationPumps)        // یک Location می‌تواند چندین LocationPump داشته باشد
            .WithOne(p => p.Location)             // هر LocationPump به یک Location تعلق دارد
            .HasForeignKey(p => p.LocationID)  // کلید خارجی در جدول LocationPump
            .OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<Location>()
        //    .HasOne(l => l.CreatedBy)
        //    .WithMany()
        //    .HasForeignKey(l => l.CreatedByUserID)
        //    .OnDelete(DeleteBehavior.Restrict);

        //modelBuilder.Entity<Location>()
        //    .HasOne(l => l.UpdatedBy)
        //    .WithMany()
        //    .HasForeignKey(l => l.UpdatedByUserID)
        //    .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Assessment_Employee> AssessmentEmployees { get; set; }
    public DbSet<CurrentMeter> CurrentMeters { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<FormValue> FormValues { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationType> LocationTypes { get; set; }
    public DbSet<Propeller> Propellers { get; set; }
    public DbSet<AssessmentGate> AssessmentGates { get; set; }
    public DbSet<LocationPump> LocationPumps { get; set; }
    public DbSet<AssessmentPump> AssessmentPumps { get; set; }
    public DbSet<User> Users { get; set; }
}