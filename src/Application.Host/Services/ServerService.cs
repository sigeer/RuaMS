using Application.Core.EF.Entities.SystemBase;
using Application.Core.Managers;
using Application.EF;
using Application.Host.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public ServerInfoDto GetServerInfo()
        {
            var srv = Server.getInstance();
            return new ServerInfoDto
            {
                State = srv.IsStarting ? 1: (srv.IsOnline ? 2 : 0),
                IsOnline = srv.IsOnline,
                RunningWorldCount = srv.RunningWorlds.Count
            };
        }

        //public List<WorldServerDto> GetWorldServerList()
        //{
        //    var allWorlds = ServerManager.LoadAllWorld();

        //    var srv = Server.getInstance();
        //    var allDto = _mapper.Map<List<WorldServerDto>>(allWorlds);
        //    var allConfigs = _mapper.Map<List<WorldServerConfig>>(allWorlds);
        //    allDto.ForEach(w =>
        //    {
        //        w.Config = allConfigs.FirstOrDefault(x => x.Id == w.Id)!;
        //        if (srv.RunningWorlds.TryGetValue(w.Id, out var world))
        //        {
        //            w.ActualConfig = new WorldServerConfig
        //            {
        //                Id = world.Id,
        //                StartPort = world.Channels.FirstOrDefault()?.Port ?? 0,
        //                Name = world.Name,
        //                ExpRate = world.ExpRate,
        //                BossDropRate = world.BossDropRate,
        //                EventMessage = world.EventMessage,
        //                MobRate = world.MobRate,
        //                QuestRate = world.QuestRate,
        //                ServerMessage = world.ServerMessage,
        //                RecommendMessage = world.WhyAmIRecommended,
        //                TravelRate = world.TravelRate,
        //                DropRate = world.DropRate,
        //                MesoRate = world.MesoRate,
        //                FishingRate = world.FishingRate,
        //                ChannelCount = world.Channels.Count
        //            };
        //            w.Channels = world.Channels.Select(x => new WorldChannelServerDto
        //            {
        //                Id = x.getId(),
        //                Port = x.Port,
        //                IsRunning = x.IsRunning
        //            }).ToList();
        //        }
        //    });
        //    return allDto;
        //}

        public async Task<bool> ToggleWorldServerState(WorldServerState data)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateConfig(WorldServerConfig data)
        {
            return await Task.FromResult(true);
        }

        public bool Apply()
        {
            if (!Server.getInstance().IsOnline)
                return false;
            ServerManager.ApplyWorldServer();
            return true;
        }

        public bool Apply(int id)
        {
            if (!Server.getInstance().IsOnline)
                return false;

            var config = ServerManager.GetWorld(id);
            if (config == null)
                return false;

            return true;
        }
    }
}
