namespace LyricsApp.Application.Domain.Entities
{
    public class Tag
    {
        private Tag() { }

        public Tag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; set; }

    }
}
