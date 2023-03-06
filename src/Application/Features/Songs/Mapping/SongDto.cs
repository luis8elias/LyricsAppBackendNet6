namespace LyricsApp.Application.Features.Songs.Mapping
{
    public class SongDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Lyric { get; set; }
        public string Genre { get; set; }
        public IList<string> Tags { get; set; }
    }
}