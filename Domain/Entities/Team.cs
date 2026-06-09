namespace poll_api.Domain.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
