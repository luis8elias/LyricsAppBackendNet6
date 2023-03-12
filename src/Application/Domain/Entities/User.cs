
namespace LyricsApp.Application.Domain.Entities
{
    public class User
    {

        public User(string name, string email, string password, string? phoneNumber)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Password = password;
            PhoneNumber = phoneNumber;
        }

        public Guid Id { get; set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string? PhoneNumber { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenCreated { get; set; }
        public DateTime RefreshTokenExpires { get; set; }

        public ICollection<GroupAssignment> Groups { get; private set; }
    }
}
