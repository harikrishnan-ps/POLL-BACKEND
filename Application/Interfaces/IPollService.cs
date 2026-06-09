using poll_api.Application.DTOs.Poll;

namespace poll_api.Application.Interfaces
{
    public interface IPollService
    {
        Task<IEnumerable<PollDto>> GetAllPollsAsync(int? userId, bool isAdmin);
        Task<PollDto> GetPollByIdAsync(int id, int? userId, bool isAdmin);
        Task<PollDto> CreatePollAsync(CreatePollDto createPollDto);
        Task RevealResultsAsync(int id);
        Task SubmitVoteAsync(VoteDto voteDto, int userId);
        Task DeletePollAsync(int id);
    }
}
