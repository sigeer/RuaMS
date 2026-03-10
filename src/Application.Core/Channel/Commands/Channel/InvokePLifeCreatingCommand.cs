using LifeProto;
using server.life;
using tools;

namespace Application.Core.Channel.Commands.Channel
{
    internal class InvokePLifeCreatingCommand : IWorldChannelCommand
    {
        LifeProto.CreatePLifeRequest data;

        public InvokePLifeCreatingCommand(CreatePLifeRequest data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            Player? chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.MasterId);

            if (ctx.WorldChannel.getMapFactory().isMapLoaded(data.Data.MapId))
            {
                var map = ctx.WorldChannel.getMapFactory().getMap(data.Data.MapId);
                if (data.Data.Type == LifeType.NPC)
                {
                    var npc = LifeFactory.Instance.getNPC(data.Data.LifeId);
                    if (npc != null && npc.getName() == "MISSINGNO")
                    {
                        npc.setPosition(new Point(data.Data.X, data.Data.Y));
                        npc.setCy(data.Data.Cy);
                        npc.setRx0(data.Data.Rx0);
                        npc.setRx1(data.Data.Rx1);
                        npc.setFh(data.Data.Fh);

                        map.addMapObject(npc);
                        map.broadcastMessage(PacketCreator.spawnNPC(npc));

                    }
                }
                else if (data.Data.Type == LifeType.Monster)
                {
                    var mob = LifeFactory.Instance.getMonsterStats(data.Data.LifeId);
                    if (mob != null && !mob.Stats.getName().Equals("MISSINGNO"))
                    {
                        map.addMonsterSpawn(data.Data.LifeId, new Point(data.Data.X, data.Data.Y),
                            data.Data.Cy, data.Data.F, data.Data.Fh, data.Data.Rx0, data.Data.Rx1, data.Data.Mobtime, data.Data.Hide > 0, data.Data.Team);
                    }
                }
            }

            if (chr != null)
            {
                if (data.Data.Type == LifeType.NPC)
                {
                    chr.yellowMessage("Pnpc created.");
                }
                if (data.Data.Type == LifeType.Monster)
                {
                    chr.yellowMessage("Pmob created.");
                }
            }
            ctx.WorldChannel.NodeService.DataService.LoadAllPLife();
        }
    }
}
