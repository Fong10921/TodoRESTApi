using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Entities;
using TodoRESTApi.identity.Attributes;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.Entities.Context;

public class TodoDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public virtual DbSet<Todo> Todo { get; set; }
    public virtual DbSet<MetaRole> MetaRoles { get; set; }
    public virtual DbSet<MetaRoleClaimsPivot> MetaRoleClaimsPivots { get; set; }
    public virtual DbSet<IdentityRoleClaim<Guid>> IdentityRoleClaims { get; set; }
    
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<MetaRoleClaimsPivot>()
            .HasOne(mrcp => mrcp.MetaRole)
            .WithMany(mr => mr.MetaRoleClaimsPivots)
            .HasForeignKey(mrcp => mrcp.MetaRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MetaRoleClaimsPivot>()
            .HasOne(mrcp => mrcp.IdentityRoleClaim)
            .WithMany() // No navigation in IdentityRoleClaim<Guid>
            .HasForeignKey(mrcp => mrcp.RoleClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        // Remove unnecessary navigation that causes extra column creation
        modelBuilder.Entity<MetaRole>()
            .Ignore(mr => mr.RoleClaims);
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            foreach (var property in clrType.GetProperties())
            {
                if (Attribute.IsDefined(property, typeof(UniqueAttribute)))
                {
                    modelBuilder.Entity(clrType)
                        .HasIndex(property.Name)
                        .IsUnique();
                }
            }
        }
    }
}