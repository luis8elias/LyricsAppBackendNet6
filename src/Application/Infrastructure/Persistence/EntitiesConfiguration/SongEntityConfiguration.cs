using LyricsApp.Application.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LyricsApp.Application.Infrastructure.Persistence.EntitiesConfiguration
{

    public class SongEntityConfiguration : IEntityTypeConfiguration<Song>
    {
        public void Configure(EntityTypeBuilder<Song> builder)
        {
            builder.ToTable("Songs");

            builder.HasKey(nameof(Song.Id));

            builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.Lyric)
            .HasColumnType("Text")
            .IsRequired();

            builder.Navigation(nameof(Song.Genre))
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

            builder.OwnsMany<Tag>(nameof(Song.Tags), songBuilder =>
            {
                songBuilder.ToTable("SongTags");

                const string foreignKey = "SongId";

                songBuilder.Property<Guid>(foreignKey);
                songBuilder.WithOwner().HasForeignKey(foreignKey);
                songBuilder.HasKey(foreignKey, nameof(Tag.Name));

                songBuilder.Property(x => x.Name)
                .HasMaxLength(20)
                .IsRequired();



            });
        }
    }

}