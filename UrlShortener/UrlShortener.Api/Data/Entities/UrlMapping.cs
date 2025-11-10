namespace UrlShortener.Api.Data.Entities;

public class UrlMapping
{
    public Guid Id { get; set; }

    public string OriginalUrl { get; set; } = string.Empty;

    public string ShortCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid? UserId { get; set; }

    public int ClickCount { get; set; }
}