using Microsoft.EntityFrameworkCore;
using System.Data;
using TP1.Models;

namespace TP1.DataLayer
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        // public DbSet<User> Users { get; set; } = default!;
        // public DbSet<Movie> Movie { get; set; } = default!;
        // public DbSet<MovieImage> MovieImages { get; set; } = default!;
        public DbSet<TP1.Models.Product> Product { get; set; } = default!;

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Apply the generic configuration to all entities inheriting from Auditable
        //    //modelBuilder.ApplyConfiguration(new AuditableEntityConfiguration<User>());

        //}

        //public override int SaveChanges()
        //{
        //    UpdateAuditFields();
        //    return base.SaveChanges();
        //}

        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    UpdateAuditFields();
        //    return base.SaveChangesAsync(cancellationToken);
        //}

        //private void UpdateAuditFields()
        //{
            //var entries = ChangeTracker.Entries()
            //    .Where(e => e.Entity is Auditable && (e.State == EntityState.Added || e.State == EntityState.Modified));

            //foreach (var entry in entries)
            //{
            //    var entity = (Auditable)entry.Entity;

            //    if (entry.State == EntityState.Added)
            //    {
            //        entity.Created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            //        entity.Created_by = _currentUserService.GetCurrentUserId();
            //        entity.Updated_at = null;
            //        entity.Updated_by = null;
            //        entity.RowVersion = 1;
            //    }
            //    else if (entry.State == EntityState.Modified)
            //    {
            //        entity.Updated_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            //        entity.Updated_by = _currentUserService.GetCurrentUserId();
            //        entity.RowVersion++;

            //        // Detach CreatedBy and Created_at to prevent updates
            //        entry.Property(nameof(Auditable.Created_by)).IsModified = false;
            //        entry.Property(nameof(Auditable.Created_at)).IsModified = false;
            //    }
            //}
        //}
    }
}