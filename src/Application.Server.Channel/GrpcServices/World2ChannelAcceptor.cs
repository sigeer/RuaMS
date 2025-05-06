using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using server.life;
using World2Channel;

namespace Application.Server.Channel.GrpcServices
{
    public class World2ChannelAcceptor : World2ChannelService.World2ChannelServiceBase
    {
        readonly IWorldChannel worldChannel;

        public World2ChannelAcceptor(IWorldChannel worldChannel)
        {
            this.worldChannel = worldChannel;
        }

        public override Task<Empty> AddMonsterSpawn(AddMonsterSpawnMessage request, ServerCallContext context)
        {
            var mob = LifeFactory.getMonster(request.MobId);
            if (mob != null)
            {
                mob.setPosition(new System.Drawing.Point(request.PosX, request.PoxY));
                mob.setCy(request.Cy);
                mob.setRx0(request.Rx0);
                mob.setRx1(request.Rx1);
                mob.setFh(request.Fh);

                var map = worldChannel.getMapFactory().getMap(request.MapId);
                map.addMonsterSpawn(mob, request.MobTime, -1);
                map.addAllMonsterSpawn(mob, request.MobTime, -1);
            }
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SendDueyNotification(SendDueyNotificationMessage request, ServerCallContext context)
        {
            var player = worldChannel.Players.getCharacterByName(request.Name);
            if (player != null && player.isLoggedinWorld())
                ItemManager.ShowDueyNotification(player);

            return Task.FromResult(new Empty());
        }
    }
}
