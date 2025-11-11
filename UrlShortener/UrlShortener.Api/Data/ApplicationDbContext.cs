using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data.Entities;

namespace UrlShortener.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UrlMapping> UrlMappings { get; set; }
    public DbSet<UserAuthen> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UrlMapping>(builder =>
        {
            builder.HasIndex(x => x.ShortCode).IsUnique();
        });

        modelBuilder.Entity<UserAuthen>(builder =>
        {
            builder.HasIndex(x => x.ApiKey).IsUnique();
        });
    }
}