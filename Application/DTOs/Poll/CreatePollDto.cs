using System.ComponentModel.DataAnnotations;

namespace poll_api.Application.DTOs.Poll
{
    public class CreatePollDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "A poll must have at least two teams.")]
        public List<CreateTeamDto> Teams { get; set; } = new List<CreateTeamDto>();

        public bool IsAnonymous { get; set; } = true;
    }
}
