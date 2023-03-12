using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using LyricsApp.Application.Common.Configurations;
using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Domain.Interfaces;
using LyricsApp.Application.Features.Auth.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace LyricsApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtConfiguration _jwtConfiguration;
        private readonly IHttpContextService _httpContextService;

        public AuthService(JwtConfiguration jwtConfiguration, IHttpContextService httpContextService)
        {
            _jwtConfiguration = jwtConfiguration;
            _httpContextService = httpContextService;
        }

        public string GenerateJwt(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecurityKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtConfiguration.Issuer,
                audience: _jwtConfiguration.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtConfiguration.JwtExpirationInMinutes),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return jwt;
        }

        public void GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshTokenResponse
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(_jwtConfiguration.RefreshTokenExpirationInDays),
                Created = DateTime.Now
            };

            SetRefreshToken(refreshToken, user);
        }

        private void SetRefreshToken(RefreshTokenResponse newRefreshToken, User user)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };

            _httpContextService.SetCookie("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenCreated = newRefreshToken.Created;
            user.RefreshTokenExpires = newRefreshToken.Expires;
        }
    }
}