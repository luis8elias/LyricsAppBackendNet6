
using LyricsApp.Application.Features.Tags.Commands;

namespace LyricsApp.Application.Domain.Entities
{
    public class Tag
    {
        public Tag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

    }
}
