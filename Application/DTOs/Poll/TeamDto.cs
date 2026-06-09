namespace poll_api.Application.DTOs.Poll
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int VoteCount { get; set; }
        public double VotePercentage { get; set; }
    }
}
