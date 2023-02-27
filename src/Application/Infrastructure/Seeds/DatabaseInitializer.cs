using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Infrastructure.Seeds;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApiDbContext _context;
    private Guid _adminId;

    public DatabaseInitializer(ApiDbContext context)
    {
        _context = context;
        context.Database.Migrate();
    }

    public void Run()
    {
        SeedUsers();
        SeedGenres();
        SeedGroups();
    }

    private void SeedUsers()
    {
        var adminUser = _context.Users.FirstOrDefault(x => x.Email == "admin@lyrics.com");
        if (adminUser == null)
        {
            var password = BCrypt.Net.BCrypt.HashPassword("admin123");
            var user = new User("AdminUser", "admin@lyrics.com", password, null);
            _adminId = user.Id;
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        else
        {
            _adminId = adminUser.Id;
        }
    }

    private void SeedGenres()
    {
        if (!_context.Genres.Any())
        {
            var genres = new List<Genre> {
                new Genre("Balada"),
                new Genre("Pop"),
                new Genre("Cumbia"),
                new Genre("Corridita"),
                new Genre("Ranchera"),
                new Genre("Corrido"),
                new Genre("Bolero"),
                new Genre("Romántica")
            };

            _context.Genres.AddRange(genres);
            _context.SaveChanges();
        }
    }

    private void SeedGroups()
    {
        if (!_context.Groups.Any())
        {
            var groups = new List<Group>()
            {
                new Group("Tutys de la Sierra", _adminId),
            };

            _context.Groups.AddRange(groups);
            _context.SaveChanges();
        }
    }
}