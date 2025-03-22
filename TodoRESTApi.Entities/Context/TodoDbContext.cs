using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Entities;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.Entities.Context;

public class TodoDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public virtual DbSet<Todo> Todo { get; set; }
    
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}