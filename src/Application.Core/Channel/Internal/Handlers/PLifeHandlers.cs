using Application.Shared.Message;
using Google.Protobuf;
using LifeProto;
using server.life;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class PLifeHandlers
    {

        public class Create : InternalSessionChannelHandler<CreatePLifeRequest>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlifeCreated;

            protected override void HandleMessage(CreatePLifeRequest data)
            {
                _server.Broadcast(w =>
                {
                    if (w.getMapFactory().isMapLoaded(data.Data.MapId))
                    {
                        var map = w.getMapFactory().getMap(data.Data.MapId);
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

                    w.getPlayerStorage().GetCharacterActor(data.MasterId)?.Send(m =>
                    {
                        var chr = m.getCharacterById(data.MasterId);
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
                    });

                    _server.DataService.LoadAllPLife();
                });
            }

            protected override CreatePLifeRequest Parse(ByteString data) => CreatePLifeRequest.Parser.ParseFrom(data);
        }

        public class Remove : InternalSessionChannelHandler<RemovePLifeResponse>
        {
            public Remove(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlifeRemoved;

            protected override void HandleMessage(RemovePLifeResponse res)
            {
                _server.Broadcast(w =>
                {
                    foreach (var data in res.RemovedItems)
                    {
                        if (w.getMapFactory().isMapLoaded(data.MapId))
                        {
                            var map = w.getMapFactory().getMap(data.MapId);
                            if (data.Type == LifeType.NPC)
                            {
                                map.destroyNPC(data.LifeId);
                            }
                            else if (data.Type == LifeType.Monster)
                            {
                                map.removeMonsterSpawn(data.LifeId, data.X, data.Y);
                            }
                        }
                    }

                    w.getPlayerStorage().GetCharacterActor(res.MasterId)?.Send(m =>
                    {
                        var chr = m.getCharacterById(res.MasterId);
                        if (chr != null)
                        {
                            if (res.LifeType == LifeType.NPC)
                            {
                                chr.yellowMessage("Cleared " + res.RemovedItems.Count + " pNPC placements.");
                            }
                            if (res.LifeType == LifeType.Monster)
                            {
                                chr.yellowMessage("Cleared " + res.RemovedItems.Count + " pmob placements.");
                            }
                        }
                    });

                    _server.DataService.LoadAllPLife();
                });
            }

            protected override RemovePLifeResponse Parse(ByteString data) => RemovePLifeResponse.Parser.ParseFrom(data);
        }
    }
}
