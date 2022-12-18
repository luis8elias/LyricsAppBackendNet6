using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using LyricsApp.Application.Domain;
using LyricsApp.Application.Domain.Entities;

namespace LyricsApp.Application.Infrastructure.Persistence;

public partial class ApiDbContext : DbContext
{
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<User> Users => Set<User>();
}