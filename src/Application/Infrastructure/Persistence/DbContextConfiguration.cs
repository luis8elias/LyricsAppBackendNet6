using LyricsApp.Application.Domain;
using LyricsApp.Application.Domain.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LyricsApp.Application.Infrastructure.Persistence;

public partial class ApiDbContext : DbContext
{
    private readonly IPublisher _publisher;
    private readonly ILogger<ApiDbContext> _logger;
    private IDbContextTransaction? _currentTransaction;

    public ApiDbContext(DbContextOptions<ApiDbContext> options, IPublisher publisher, ILogger<ApiDbContext> logger) : base(options)
    {
        _publisher = publisher;
        _logger = logger;

        _logger.LogDebug("DbContext created.");
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            _logger.LogInformation("A transaction with ID {ID} is already created", _currentTransaction.TransactionId);
            return;
        }


        _currentTransaction = await Database.BeginTransactionAsync();
        _logger.LogInformation("A new transaction was created with ID {ID}", _currentTransaction.TransactionId);
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction is null)
        {
            return;
        }

        _logger.LogInformation("Commiting Transaction {ID}", _currentTransaction.TransactionId);

        await _currentTransaction.CommitAsync();

        _currentTransaction.Dispose();
        _currentTransaction = null;
    }

    public async Task RollbackTransaction()
    {
        if (_currentTransaction is null)
        {
            return;
        }

        _logger.LogDebug("Rolling back Transaction {ID}", _currentTransaction.TransactionId);

        await _currentTransaction.RollbackAsync();

        _currentTransaction.Dispose();
        _currentTransaction = null;
    }

    public override int SaveChanges()
    {
        SetEntityTrackingValues();
        var result = base.SaveChanges();
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetEntityTrackingValues();

        var result = await base.SaveChangesAsync(cancellationToken);
        await PublishDominEvents(cancellationToken);
        return result;
    }

    private void SetEntityTrackingValues()
    {
        var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IEntityTracking && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {

            if (entityEntry.State == EntityState.Modified)
            {
                ((IEntityTracking)entityEntry.Entity).UpdatedAt = DateTime.Now;
                // ((IEntityTracking)entityEntry.Entity).UpdatedBy = currentUserId;
            }

            if (entityEntry.State == EntityState.Added)
            {
                ((IEntityTracking)entityEntry.Entity).CreatedAt = DateTime.Now;
                // ((IEntityTracking)entityEntry.Entity).CreatedBy = currentUserId;
            }
        }
    }

    private async Task PublishDominEvents(CancellationToken cancellationToken)
    {
        var events = ChangeTracker.Entries<IHasDomainEvent>()
                        .Select(x => x.Entity.DomainEvents)
                        .SelectMany(x => x)
                        .Where(domainEvent => !domainEvent.IsPublished)
                        .ToArray();

        foreach (var @event in events)
        {
            @event.IsPublished = true;

            _logger.LogInformation("New domain event {Event}", @event.GetType().Name);

            // Note: If an unhandled exception occurs, all the saved changes will be rolled back
            // by the TransactionBehavior. All the operations related to a domain event finish
            // successfully or none of them do.
            // Reference: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation#what-is-a-domain-event
            await _publisher.Publish(@event, cancellationToken);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
    }

}