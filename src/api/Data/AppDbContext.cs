using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    // private readonly ICurrentUserService _currentUserService; // Service to get logged-in user
    private readonly IUserContext _userContext;
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderBoard> OrderBoards { get; set; }
    public DbSet<OrderBoardComponent> OrderBoardComponents { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Component> Components { get; set; }
    public DbSet<ComponentType> ComponentTypes { get; set; }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IUserContext userContext) : base(options)
    {
        _userContext = userContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderBoard>()
            .HasOne(ob => ob.Order)
            .WithMany(o => o.OrderBoards)
            .HasForeignKey(ob => ob.OrderId);

        modelBuilder.Entity<OrderBoardComponent>()
            .HasOne(obc => obc.OrderBoard)
            .WithMany(ob => ob.OrderBoardComponents)
            .HasForeignKey(obc => obc.OrderBoardId);

        modelBuilder.Entity<OrderBoardComponent>()
            .HasOne(obc => obc.Component)
            .WithMany()
            .HasForeignKey(obc => obc.ComponentId);

        modelBuilder.Entity<OrderBoardComponent>()
            .HasIndex(obc => new { obc.OrderBoardId, obc.ComponentId })
            .IsUnique();

        modelBuilder.Entity<ComponentType>()
            .HasOne(ct=> ct.Component)
            .WithOne(ct=> ct.ComponentType)
            .HasForeignKey<Component>(c=> c.Id);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Auditable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = _userContext?.UserId ?? "";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = DateTime.UtcNow;
                entry.Entity.LastModifiedBy = _userContext?.UserId ?? "";
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}