using Microsoft.EntityFrameworkCore;
using SUPPLY_API;

public class CollaboratorSystemContext : DbContext
{
    public CollaboratorSystemContext(DbContextOptions<CollaboratorSystemContext> options)
        : base(options)
    {
    }

    public DbSet<CollaboratorSystemDb> CollaboratorSystem { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CollaboratorSystemDb>(pc =>
        {
            pc.HasKey(u => u.Id);
            pc.ToTable("CollaboratorSystem");
        });
    }

    internal object Find(string email)
    {
        throw new NotImplementedException();
    }
}