using LyricsApp.Application.Domain.Base;

namespace LyricsApp.Application.Domain.Entities
{

    public class Song : EntityTracking
    {
        private readonly List<Tag> _tags;
        private Song() { }

        public Song(string title, string lyric, Guid genreId)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
            }

            if (string.IsNullOrEmpty(lyric))
            {
                throw new ArgumentException($"'{nameof(lyric)}' cannot be null or empty.", nameof(lyric));
            }

            if (genreId == default)
            {
                throw new ArgumentNullException(nameof(genreId));
            }

            Id = Guid.NewGuid();
            Title = title;
            Lyric = lyric;
            GenreId = genreId;
            _tags = new();
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Lyric { get; private set; }

        public IReadOnlyList<Tag> Tags => _tags;

        public Guid GenreId { get; private set; }

        public Genre Genre { get; private set; }


        public void AddTag(Tag tag)
        {
            if (!_tags.Contains(tag))
            {
                _tags.Add(tag);
            }
        }

        public void RemoveTag(Tag tag)
        {
            if (_tags.Contains(tag))
            {
                _tags.Remove(tag);
            }
        }

        public bool UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
            }

            if (Title == title)
            {
                return false;
            }

            Title = title;

            return true;
        }

        public bool UpdateLyric(string lyric)
        {
            if (string.IsNullOrWhiteSpace(lyric))
            {
                throw new ArgumentException($"'{nameof(lyric)}' cannot be null or whitespace.", nameof(lyric));
            }

            if (Lyric == lyric)
            {
                return false;
            }

            Lyric = lyric;

            return true;
        }

        public bool UpdateGenre(Guid genreId)
        {
            if (GenreId == genreId)
            {
                return false;
            }

            GenreId = genreId;

            return true;
        }

        public bool UpdateTags(IList<string> tags)
        {
            bool updatedValues = false;
            if (tags is null)
            {
                throw new ArgumentNullException(nameof(tags));
            }

            if (tags.Count == 0 && _tags.Count > 0)
            {
                _tags.Clear();
                return true;
            }

            var removeTags = _tags.Where(x => !tags.Any(t => t == x.Name));

            if (_tags.RemoveAll(tag => !tags.Any(t => t == tag.Name)) > 0)
            {
                updatedValues = true;
            }

            var itemsToAdd = new List<string>();

            foreach (var item in tags)
            {
                if (!_tags.Any(x => x.Name == item))
                {
                    itemsToAdd.Add(item);
                }
            }

            if (itemsToAdd.Count > 0)
            {
                _tags.AddRange(itemsToAdd.Select(s => new Tag(s)));
                updatedValues = true;
            }

            return updatedValues;
        }
    }
}