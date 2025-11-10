namespace UrlShortener.Api.Data.Entities
{
    public class UserAuthen
    {
            public Guid Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;

            public string ApiKey { get; set; } = string.Empty; // API Key
    }
}
