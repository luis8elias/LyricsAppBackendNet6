using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using LyricsApp.Application.Domain;
using LyricsApp.Application.Domain.Entities;

namespace LyricsApp.Application.Infrastructure.Persistence;

public partial class ApiDbContext : DbContext
{
    public DbSet<Tag> Tags { get; set;}
    public DbSet<User> Users { get; set; }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupAssignment> GroupAssignments { get; set; }
    public DbSet<Song> Songs { get; set; }

}