using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace LyricsApp.Application.Infrastructure.Persistence;

public static class ModelBuilderExtension
{

    public static void ApplyGlobalFilters<T>(this ModelBuilder modelBuilder, string propertyName, T value)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var foundProperty = entityType.FindProperty(propertyName);
            if (foundProperty != null && foundProperty.ClrType == typeof(T))
            {
                var newParam = Expression.Parameter(entityType.ClrType);
                var filter = Expression.Lambda(Expression.Equal(Expression.Property(newParam, propertyName), Expression.Constant(value)), newParam);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}