using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data.Entities;

namespace UrlShortener.Api.Data;

public class ApplicationDbContext : DbContext
{
    // Constructor này bắt buộc để 'Dependency Injection' hoạt động
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Khai báo bảng CSDL của chúng ta
    public DbSet<UrlMapping> UrlMappings { get; set; }
    public DbSet<UserAuthen> Users { get; set; }

    // (Tùy chọn) Cấu hình thêm cho model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Chúng ta nên thêm một 'Index' (chỉ mục) cho cột ShortCode
        // để việc tìm kiếm (GET /{shortCode}) nhanh hơn rất nhiều.
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