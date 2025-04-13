using AuthServer.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AuthServer.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // replace table name to match the naming convention of postgres
            entity.SetTableName(entity.GetTableName()?.Underscore());

            var objectIdentifier = StoreObjectIdentifier.Table(
                entity.GetTableName()?.Underscore()!,
                entity.GetSchema()
            );

            // replace column names
            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName(objectIdentifier)?.Underscore());

            foreach (var key in entity.GetKeys()) key.SetName(key.GetName()?.Underscore());

            foreach (var key in entity.GetForeignKeys()) key.SetConstraintName(key.GetConstraintName()?.Underscore());
        }

        builder.Entity<ApplicationUser>().Property(u => u.FirstName).HasMaxLength(50);
        builder.Entity<ApplicationUser>().Property(u => u.LastName).HasMaxLength(50);
        builder.Entity<ApplicationRole>().Property(u => u.Description).HasMaxLength(255);

        // Data seeder
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}