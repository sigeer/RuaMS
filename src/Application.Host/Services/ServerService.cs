using Application.Core.Managers;
using Application.EF;
using Application.Host.Models;
using AutoMapper;
using net.server;

namespace Application.Host.Services
{
    public class ServerService
    {
        readonly DBContext _dbContext;
        readonly IMapper _mapper;

        public ServerService(DBContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public List<WorldServerDto> GetWorldServerList()
        {
            var allWorlds = ServerManager.LoadAllWorld();

            var srv = Server.getInstance();
            var allRunningWorlds = srv.getWorlds();

            var allDto = _mapper.Map<List<WorldServerDto>>(allWorlds);
            if (srv.RunningWorlds.Count > 0)
            {
                allDto.ForEach(w =>
                {
                    if (srv.RunningWorlds.TryGetValue(w.Id, out var world))
                    {
                        w.Channels = world.Channels.Select(x => new WorldChannelServerDto
                        {
                            Id = x.getId(),
                            Port = x.Port,
                            IsRunning = x.IsRunning
                        }).ToList();
                    }
                });
            }
            return allDto;
        }

        public bool Apply()
        {
            ServerManager.ApplyWorldServer();
            return true;
        }
    }
}
