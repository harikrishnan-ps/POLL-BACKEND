namespace poll_api.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
