namespace LyricsApp.Application.Common.Configurations
{
    public class JwtConfiguration 
    {
        public string SecurityKey { get; set; } = "wlzu1lBeq*NOqlGAzOvofroBrl#HEnIT";
        public string Issuer { get; set; } = "lyricsApp";
        public string Audience { get; set; } = "localhost";
        public int JwtExpirationInMinutes { get; set; } = 30;
        public int RefreshTokenExpirationInDays { get; set; } = 7;
    }
}