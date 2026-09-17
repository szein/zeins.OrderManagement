using System.CodeDom.Compiler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    public HealthController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    public string Get() => "I'm Alive!";

    [HttpGet("db")]
    public string GetDb() => _context.Database?.GetConnectionString() ?? "NOT CONNECTED";

    [HttpGet("env")]
    public string GetEnv() => _env?.EnvironmentName ?? "Environement Name is Empty!";
}