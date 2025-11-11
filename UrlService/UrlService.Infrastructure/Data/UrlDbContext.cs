using Microsoft.EntityFrameworkCore;
using UrlService.Domain.Entities;

namespace UrlService.Infrastructure.Data;

public class UrlDbContext : DbContext
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalUrl).IsRequired();
            entity.Property(e => e.ShortCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.UserId).IsRequired(false); // Nullable UserId
            entity.HasIndex(e => e.ShortCode).IsUnique();
        });
    }
}
