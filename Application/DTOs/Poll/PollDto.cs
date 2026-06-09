namespace poll_api.Application.DTOs.Poll
{
    public class PollDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool ResultsRevealed { get; set; }
        public bool IsAnonymous { get; set; }
        public int TotalVotes { get; set; }
        
        // This will only be populated if results are revealed or user is Admin
        public List<TeamDto> Teams { get; set; } = new List<TeamDto>();
        
        // Only populated for Admin when !IsAnonymous
        public List<VoteDetailDto>? VoteDetails { get; set; }

        // Tells the user if they've voted, and for which team.
        public bool HasVoted { get; set; }
        public int? VotedTeamId { get; set; }
    }
}
