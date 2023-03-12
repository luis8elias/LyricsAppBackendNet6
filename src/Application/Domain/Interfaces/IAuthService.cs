using LyricsApp.Application.Domain.Entities;

namespace LyricsApp.Application.Domain.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwt(User user);
        void GenerateRefreshToken(User user);
    }
}