using LyricsApp.Application.Domain.Entities;
using LyricsApp.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Infrastructure.Seeds;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApiDbContext context;
    private Guid AdminId;

    public DatabaseInitializer(ApiDbContext context)
    {
        this.context = context;
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
        var adminUser = context.Users.FirstOrDefault(x => x.Email == "admin@lyrics.com");
        if (adminUser == null)
        {
            var password = BCrypt.Net.BCrypt.HashPassword("admin123");
            var user = new User("AdminUser", "admin@lyrics.com", password, null);
            AdminId = user.Id;
            context.Users.Add(user);
            context.SaveChanges();
        }
        else
        {
            AdminId = adminUser.Id;
        }
    }

    private void SeedGenres()
    {
        if (!context.Genres.Any())
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

            context.Genres.AddRange(genres);
            context.SaveChanges();
        }
    }

    private void SeedGroups()
    {
        if (!context.Groups.Any())
        {
            var groups = new List<Group>()
            {
                new Group("Tutys de la Sierra", AdminId),
            };

            context.Groups.AddRange(groups);
            context.SaveChanges();
        }
    }
}