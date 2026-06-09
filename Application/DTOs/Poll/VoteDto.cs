using System.ComponentModel.DataAnnotations;

namespace poll_api.Application.DTOs.Poll
{
    public class VoteDto
    {
        [Required]
        public int PollId { get; set; }
        
        [Required]
        public int TeamId { get; set; }
    }
}
