using Application.Core.Login.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData
{
    public class TeamManager
    {
        ConcurrentDictionary<int, TeamModel> _dataSource = new();
        private int _currentId = 0;

        readonly MasterServer _server;
        IMapper _mapper;
        ILogger<TeamManager> _logger;

        public TeamManager(MasterServer server, IMapper mapper, ILogger<TeamManager> logger)
        {
            _server = server;
            _mapper = mapper;
            _logger = logger;
        }

        public TeamModel CreateTeam(int playerId)
        {
            return new TeamModel()
            {
                Id = Interlocked.Increment(ref _currentId),
                LeaderId = playerId
            };
        }
        public bool RemoveTeam(int teamId)
        {
            return _dataSource.TryRemove(teamId, out _);
        }

        public bool RemoveMember(int teamId, int playerId)
        {
            return _dataSource.TryGetValue(teamId, out var team) && team.Members.Remove(playerId);
        }
    }
}
