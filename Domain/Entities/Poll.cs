namespace poll_api.Domain.Entities
{
    public class Poll
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool ResultsRevealed { get; set; } = false;
        public bool IsAnonymous { get; set; } = true;

        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
