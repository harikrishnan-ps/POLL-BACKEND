using poll_api.Application.DTOs.Poll;
using poll_api.Application.Interfaces;
using poll_api.Domain.Entities;
using poll_api.Domain.Exceptions;

namespace poll_api.Application.Services
{
    public class PollService : IPollService
    {
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<Team> _teamRepository;
        private readonly IRepository<Vote> _voteRepository;
        private readonly IRepository<User> _userRepository;

        public PollService(IRepository<Poll> pollRepository, IRepository<Team> teamRepository, IRepository<Vote> voteRepository, IRepository<User> userRepository)
        {
            _pollRepository = pollRepository;
            _teamRepository = teamRepository;
            _voteRepository = voteRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PollDto>> GetAllPollsAsync(int? userId, bool isAdmin)
        {
            var polls = await _pollRepository.GetAllAsync(); // Needs Include(Teams) and Include(Votes) for real data.
            // Since we're using generic repository without Include, we'll fetch explicitly or expand our IRepository.
            // To simplify, let's fetch all related data manually.
            var teams = await _teamRepository.GetAllAsync();
            var votes = await _voteRepository.GetAllAsync();

            var result = new List<PollDto>();
            foreach (var p in polls)
            {
                var pollTeams = teams.Where(t => t.PollId == p.Id).ToList();
                var pollVotes = votes.Where(v => v.PollId == p.Id).ToList();

                var dto = new PollDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive,
                    ResultsRevealed = p.ResultsRevealed,
                    IsAnonymous = p.IsAnonymous,
                    TotalVotes = pollVotes.Count,
                    HasVoted = userId.HasValue && pollVotes.Any(v => v.UserId == userId),
                    VotedTeamId = userId.HasValue ? pollVotes.FirstOrDefault(v => v.UserId == userId)?.TeamId : null
                };

                if (p.ResultsRevealed || isAdmin)
                {
                    dto.Teams = pollTeams.Select(t => {
                        var teamVotes = pollVotes.Count(v => v.TeamId == t.Id);
                        return new TeamDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            LogoUrl = t.LogoUrl,
                            VoteCount = teamVotes,
                            VotePercentage = pollVotes.Count > 0 ? Math.Round((double)teamVotes / pollVotes.Count * 100, 2) : 0
                        };
                    }).ToList();
                }
                else
                {
                    // If not revealed, just send team basics without counts
                    dto.Teams = pollTeams.Select(t => new TeamDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        LogoUrl = t.LogoUrl,
                        VoteCount = 0,
                        VotePercentage = 0
                    }).ToList();
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task<PollDto> GetPollByIdAsync(int id, int? userId, bool isAdmin)
        {
            var poll = await _pollRepository.GetByIdAsync(id);
            if (poll == null) throw new NotFoundException("Poll not found.");

            var teams = await _teamRepository.FindAsync(t => t.PollId == id);
            var votes = await _voteRepository.FindAsync(v => v.PollId == id);

            var pollVotes = votes.ToList();
            var pollTeams = teams.ToList();

            var dto = new PollDto
            {
                Id = poll.Id,
                Title = poll.Title,
                Description = poll.Description,
                CreatedAt = poll.CreatedAt,
                IsActive = poll.IsActive,
                ResultsRevealed = poll.ResultsRevealed,
                IsAnonymous = poll.IsAnonymous,
                TotalVotes = pollVotes.Count,
                HasVoted = userId.HasValue && pollVotes.Any(v => v.UserId == userId.Value),
                VotedTeamId = userId.HasValue ? pollVotes.FirstOrDefault(v => v.UserId == userId.Value)?.TeamId : null
            };

            if (poll.ResultsRevealed || isAdmin)
            {
                dto.Teams = pollTeams.Select(t => {
                    var teamVotes = pollVotes.Count(v => v.TeamId == t.Id);
                    return new TeamDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        LogoUrl = t.LogoUrl,
                        VoteCount = teamVotes,
                        VotePercentage = pollVotes.Count > 0 ? Math.Round((double)teamVotes / pollVotes.Count * 100, 2) : 0
                    };
                }).ToList();
            }
            else
            {
                dto.Teams = pollTeams.Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    LogoUrl = t.LogoUrl,
                    VoteCount = 0,
                    VotePercentage = 0
                }).ToList();
            }

            if (isAdmin && !poll.IsAnonymous)
            {
                var users = await _userRepository.GetAllAsync();
                dto.VoteDetails = pollVotes.Select(v => new VoteDetailDto
                {
                    Username = users.FirstOrDefault(u => u.Id == v.UserId)?.Username ?? "Unknown",
                    TeamName = pollTeams.FirstOrDefault(t => t.Id == v.TeamId)?.Name ?? "Unknown"
                }).ToList();
            }

            return dto;
        }

        public async Task<PollDto> CreatePollAsync(CreatePollDto createPollDto)
        {
            var poll = new Poll
            {
                Title = createPollDto.Title,
                Description = createPollDto.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ResultsRevealed = false,
                IsAnonymous = createPollDto.IsAnonymous
            };

            await _pollRepository.AddAsync(poll);
            await _pollRepository.SaveChangesAsync();

            foreach (var teamDto in createPollDto.Teams)
            {
                var team = new Team
                {
                    PollId = poll.Id,
                    Name = teamDto.Name,
                    LogoUrl = teamDto.LogoUrl
                };
                await _teamRepository.AddAsync(team);
            }
            await _teamRepository.SaveChangesAsync();

            return await GetPollByIdAsync(poll.Id, null, true);
        }

        public async Task RevealResultsAsync(int id)
        {
            var poll = await _pollRepository.GetByIdAsync(id);
            if (poll == null) throw new NotFoundException("Poll not found.");

            poll.ResultsRevealed = true;
            poll.IsActive = false; // Usually revealing closes the poll
            _pollRepository.Update(poll);
            await _pollRepository.SaveChangesAsync();
        }

        public async Task SubmitVoteAsync(VoteDto voteDto, int userId)
        {
            var poll = await _pollRepository.GetByIdAsync(voteDto.PollId);
            if (poll == null) throw new NotFoundException("Poll not found.");
            
            if (!poll.IsActive) throw new BadRequestException("Poll is closed.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.RoleId == 1) throw new BadRequestException("Admins cannot vote.");

            var existingVote = await _voteRepository.FindAsync(v => v.PollId == voteDto.PollId && v.UserId == userId);
            if (existingVote.Any()) throw new BadRequestException("You have already voted in this poll.");

            var team = await _teamRepository.GetByIdAsync(voteDto.TeamId);
            if (team == null || team.PollId != voteDto.PollId) throw new BadRequestException("Invalid team.");

            var vote = new Vote
            {
                PollId = voteDto.PollId,
                UserId = userId,
                TeamId = voteDto.TeamId,
                CreatedAt = DateTime.UtcNow
            };

            await _voteRepository.AddAsync(vote);
            await _voteRepository.SaveChangesAsync();
        }

        public async Task DeletePollAsync(int id)
        {
            var poll = await _pollRepository.GetByIdAsync(id);
            if (poll == null) throw new NotFoundException("Poll not found.");

            // Remove related votes
            var votes = await _voteRepository.FindAsync(v => v.PollId == id);
            foreach (var vote in votes)
            {
                _voteRepository.Delete(vote);
            }
            await _voteRepository.SaveChangesAsync();

            // Remove related teams
            var teams = await _teamRepository.FindAsync(t => t.PollId == id);
            foreach (var team in teams)
            {
                _teamRepository.Delete(team);
            }
            await _teamRepository.SaveChangesAsync();

            // Remove poll
            _pollRepository.Delete(poll);
            await _pollRepository.SaveChangesAsync();
        }
    }
}
