namespace LyricsApp.Application.Domain.Entities
{
    public class Tag
    {
        private Tag() { }

        public Tag(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; private set; }
        public string Name { get; set; }

    }
}
