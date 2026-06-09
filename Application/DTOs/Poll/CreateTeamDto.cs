namespace poll_api.Application.DTOs.Poll
{
    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }
}
