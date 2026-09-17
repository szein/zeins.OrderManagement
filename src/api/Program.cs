using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddOpenApi();

//DbContext
var connectionString = builder.Configuration.GetConnectionString($"{builder.Configuration["DefaultConnectionName"]}")
                      ?? "Data Source=DB-not-from-appsettings.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

//Authentication
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
});

builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();

//Services and Controllers
builder.Services.AddControllers();

//TODO: try to manage CORS from Entra Id App regestration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedCORS",
        policy => policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigin").Get<string[]>() ?? [])
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});



var app = builder.Build();

//Seed
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.OpenConnectionAsync();
    if (builder.Configuration["DefaultConnectionName"]?.ToLowerInvariant() == "SqliteConnection".ToLowerInvariant())
    {
        //Helps container to manage sqlite database file by clean up
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=DELETE;");
    }

    await dbContext.Database.MigrateAsync();

    if (!await dbContext.Boards.AnyAsync() && !await dbContext.ComponentTypes.AnyAsync())
    {
        await SeedDatabaseAsync(dbContext);
        await dbContext.SaveChangesAsync();
    }
    await dbContext.Database.CloseConnectionAsync();
}

//TODO: move the seeding to better place
async Task SeedDatabaseAsync(AppDbContext dbContext)
{
    var order1Id = Guid.NewGuid();
    var order2Id = Guid.NewGuid();
    var order3Id = Guid.NewGuid();
    var order1BoardId = Guid.NewGuid();
    var order2BoardId = Guid.NewGuid();
    var order3BoardId = Guid.NewGuid();

    dbContext.Boards.AddRange(
        new Board { Id = 1, Name = "Board 1", Description = "Description for Board 1", Length = 10.5, Width = 8.5 },
        new Board { Id = 2, Name = "Board 2", Description = "Description for Board 2", Length = 12.0, Width = 9.0 }
    );
    dbContext.ComponentTypes.AddRange(
        new ComponentType { Id = 1, Name = "Type A", Description = "Description for Type A" },
        new ComponentType { Id = 2, Name = "Type B", Description = "Description for Type B" },
        new ComponentType { Id = 3, Name = "Type C", Description = "Description for Type C" }
    );
    dbContext.Components.AddRange(
        new Component { Id = 1, ComponentTypeId = 1, Quantity = 5 },
        new Component { Id = 2, ComponentTypeId = 2, Quantity = 10 },
        new Component { Id = 3, ComponentTypeId = 3, Quantity = 0 }
    );
    dbContext.Orders.AddRange(
        new Order { Id = order1Id, Name = "Order 1", OrderDate = DateTime.UtcNow },
        new Order { Id = order2Id, Name = "Order 2", OrderDate = DateTime.UtcNow },
        new Order { Id = order3Id, Name = "Order 3", OrderDate = DateTime.UtcNow, Status = OrderStatus.Completed },
        new Order { Id = Guid.NewGuid(), Name = "Order 4", OrderDate = DateTime.UtcNow, Status = OrderStatus.Cancelled }
    );
    dbContext.OrderBoards.AddRange(
        new OrderBoard { Id = order1BoardId, OrderId = order1Id, BoardId = 1 },
        new OrderBoard { Id = order2BoardId, OrderId = order2Id, BoardId = 2 },
        new OrderBoard { Id = order3BoardId, OrderId = order3Id, BoardId = 2 }
    );
    dbContext.OrderBoardComponents.AddRange(
        new OrderBoardComponent { OrderBoardId = order1BoardId, ComponentId = 1, Quantity = 2 },
        new OrderBoardComponent { OrderBoardId = order1BoardId, ComponentId = 2, Quantity = 1 },
        new OrderBoardComponent { OrderBoardId = order2BoardId, ComponentId = 2, Quantity = 4 },
        new OrderBoardComponent { OrderBoardId = order2BoardId, ComponentId = 3, Quantity = 3 },
        new OrderBoardComponent { OrderBoardId = order3BoardId, ComponentId = 1, Quantity = 5 },
        new OrderBoardComponent { OrderBoardId = order3BoardId, ComponentId = 2, Quantity = 2 }
    );
}

app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    Console.WriteLine(app.Environment.EnvironmentName);
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowedCORS");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();

app.Run();
