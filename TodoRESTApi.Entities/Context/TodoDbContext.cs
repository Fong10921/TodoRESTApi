using Microsoft.EntityFrameworkCore;
using TodoRESTApi.Entities.Entities;

namespace TodoRESTApi.Entities.Context;

public class TodoDbContext: DbContext
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