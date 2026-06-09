namespace poll_api.Domain.Entities
{
    public class Vote
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
