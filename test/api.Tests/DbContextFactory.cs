using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FakeItEasy;

public class DbContextFactory
{
    public DbContextFactory()
    {
    }

    public AppDbContext CreateFakeDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
         .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
         .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
         .Options;

        var context = new AppDbContext(options, A.Fake<IUserContext>());

        // Forces EF Core to initialize and build the Identity model
        context.Database.EnsureCreated();

        return context;
    }
}