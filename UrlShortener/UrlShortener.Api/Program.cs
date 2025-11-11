using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using UrlShortener.Api.Data;
using UrlShortener.Api.Data.Entities;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// === Services ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost", "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

var app = builder.Build();

// === PIPELINE ===
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// auto migrate database
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine($"Migration error: {ex.Message}");
}

// === HashPassword (SHA256) ===
string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}

// === Create API Key ===
string GenerateApiKey()
{
    return Nanoid.Generate(size: 32);
}

// ==================================================================
// === Check API Key ===
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
    {
        var apiKey = apiKeyHeader.ToString();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
            if (user != null)
            {
                context.Items["User"] = user;
            }
        }
    }

    await next();
});

// ==================================================================
// === REGISTER (create user + API Key) ===
app.MapPost("/api/auth/register", async (RegisterRequest request, ApplicationDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Username == request.Username))
        return Results.Conflict("Username already exists");

    if (await db.Users.AnyAsync(u => u.Email == request.Email))
        return Results.Conflict("Email is already in use");

    var user = new UserAuthen
    {
        Id = Guid.NewGuid(),
        Username = request.Username,
        Email = request.Email,
        PasswordHash = HashPassword(request.Password),
        ApiKey = GenerateApiKey(),
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "Registration successful!",
        user.Id,
        user.Username,
        user.Email,
        apiKey = user.ApiKey
    });
})
.WithTags("Auth");

// === LOGIN ===
app.MapPost("/api/auth/login", async (LoginRequest request, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    if (user == null || user.PasswordHash != HashPassword(request.Password))
        return Results.Unauthorized();

    return Results.Ok(new
    {
        message = "Login successful!",
        user.Id,
        user.Username,
        user.Email,
        apiKey = user.ApiKey
    });
})
.WithTags("Auth");

// === UPDATE CURRENT USER PROFILE ===
// === UPDATE ACCOUNT (API KEY Required) ===
app.MapPut("/api/account", async (UpdateAccountRequest req, ApplicationDbContext db, HttpContext http) =>
{
    // Get API Key
    if (!http.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader) ||
        string.IsNullOrWhiteSpace(apiKeyHeader))
    {
        return Results.Unauthorized();
    }

    var apiKey = apiKeyHeader.ToString();

    // check user
    var user = await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    // Validate
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Email))
    {
        return Results.BadRequest("Username and email are required.");
    }

    // check username 
    var existUserName = await db.Users
        .AnyAsync(u => u.Username == req.Username && u.Id != user.Id);
    if (existUserName)
    {
        return Results.Conflict("Username is already taken.");
    }

    // check email
    var existEmail = await db.Users
        .AnyAsync(u => u.Email == req.Email && u.Id != user.Id);
    if (existEmail)
    {
        return Results.Conflict("Email is already in use.");
    }

    // update info
    user.Username = req.Username;
    user.Email = req.Email;

    // if have new password -> update password
    if (!string.IsNullOrWhiteSpace(req.NewPassword))
    {
        if (req.NewPassword.Length < 6)
        {
            return Results.BadRequest("New password must be at least 6 characters.");
        }

        // using local function to hash password
        string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        user.PasswordHash = HashPassword(req.NewPassword);
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "Account updated successfully.",
        user.Id,
        user.Username,
        user.Email
    });
})
.WithTags("Account");


// ==================================================================
// === Create Short URL ===
app.MapPost("/api/url", async (ShortenUrlRequest request, ApplicationDbContext db, HttpContext http) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        return Results.BadRequest("Invalid URL.");

    var shortCode = Nanoid.Generate(size: 7);

    // Get user from middleware if available
    var user = http.Items["User"] as UserAuthen;

    var mapping = new UrlMapping
    {
        Id = Guid.NewGuid(),
        OriginalUrl = request.Url,
        ShortCode = shortCode,
        CreatedAt = DateTime.UtcNow,
        UserId = user?.Id ?? Guid.Empty
    };

    await db.UrlMappings.AddAsync(mapping);
    await db.SaveChangesAsync();

    var shortUrl = $"{http.Request.Scheme}://{http.Request.Host}/{shortCode}";
    return Results.Ok(new { shortUrl, shortCode });
})
.WithTags("URLs");

// === History URL of user ===
app.MapGet("/api/url/history", async (ApplicationDbContext db, HttpContext http) =>
{
    var user = http.Items["User"] as UserAuthen;

    if (user == null)
    {
        var apiKey = http.Request.Headers["X-API-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Results.Unauthorized(); // Missing API Key
        }

        user = await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
        if (user == null)
        {
            return Results.Unauthorized(); // Invalid API Key
        }
    }

    // 3. Get URLs belonging to this user
    var urls = await db.UrlMappings
        .Where(x => x.UserId == user.Id)
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new
        {
            x.Id,
            x.OriginalUrl,
            x.ShortCode,
            x.CreatedAt,
            x.ClickCount
        })
        .ToListAsync();

    return Results.Ok(urls);
})
.WithTags("URLs");

// === DELETE 1 SHORT URL (require API KEY, only own link) ===
app.MapDelete("/api/url/history/{id:guid}", async (Guid id, ApplicationDbContext db, HttpContext http) =>
{
    if (!http.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader) ||
        string.IsNullOrWhiteSpace(apiKeyHeader))
    {
        return Results.Json(new { error = "Missing API Key." }, statusCode: 401);
    }

    var apiKey = apiKeyHeader.ToString();
    var user = await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user == null)
    {
        return Results.Json(new { error = "Invalid API Key." }, statusCode: 401);
    }

    var mapping = await db.UrlMappings
        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

    if (mapping == null)
    {
        return Results.NotFound(new { error = "URL not found." });
    }

    db.UrlMappings.Remove(mapping);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Short URL deleted." });
})
.WithTags("URLs");

// ===CLEAR ALL SHORT URL HISTORY OF CURRENT USER ===
app.MapDelete("/api/url/history", async (ApplicationDbContext db, HttpContext http) =>
{
    if (!http.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader) ||
        string.IsNullOrWhiteSpace(apiKeyHeader))
    {
        return Results.Json(new { error = "Missing API Key." }, statusCode: 401);
    }

    var apiKey = apiKeyHeader.ToString();
    var user = await db.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user == null)
    {
        return Results.Json(new { error = "Invalid API Key." }, statusCode: 401);
    }

    var urls = await db.UrlMappings
        .Where(x => x.UserId == user.Id)
        .ToListAsync();

    if (!urls.Any())
    {
        return Results.Ok(new { message = "No URLs to delete." });
    }

    db.UrlMappings.RemoveRange(urls);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "All history deleted." });
})
.WithTags("URLs");



// === REDIRECT ===
app.MapGet("/{shortCode}", async (string shortCode, ApplicationDbContext db) =>
{
    var mapping = await db.UrlMappings.FirstOrDefaultAsync(m => m.ShortCode == shortCode);
    if (mapping == null)
        return Results.NotFound("Link not found.");

    mapping.ClickCount++;
    await db.SaveChangesAsync();

    return Results.Redirect(mapping.OriginalUrl, permanent: false);
})
.WithTags("Redirect");

app.Run();

// ==================================================================
// === DTOs ===
public class RegisterRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class ShortenUrlRequest
{
    [Required] public string Url { get; set; } = string.Empty;
}

public class UpdateAccountRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
}

