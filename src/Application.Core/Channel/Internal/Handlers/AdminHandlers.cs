using Application.Core.Channel.Commands;
using Application.Core.Game.Players;
using Application.Resources.Messages;
using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Dto;
using Google.Protobuf;
using JailProto;
using System.Numerics;
using SystemProto;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class AdminHandlers
    {
        public class ReloadWorldEvents : InternalSessionChannelHandler<ReloadEventsResponse>
        {
            public ReloadWorldEvents(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.HandleWorldEventReload;

            protected override void HandleMessage(ReloadEventsResponse res)
            {
                _server.Broadcast(w =>
                {
                    w.reloadEventScriptManager();
                    w.getPlayerStorage().GetCharacterClientById(res.Request.MasterId)?.TypedMessage(5, "Reloaded Events");
                });
            }

            protected override ReloadEventsResponse Parse(ByteString data) => ReloadEventsResponse.Parser.ParseFrom(data);
        }

        public class SummonPlayer : InternalSessionChannelHandler<SummonPlayerByNameResponse>
        {
            public SummonPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.SummonPlayer;

            protected override void HandleMessage(SummonPlayerByNameResponse res)
            {
                _server.Broadcast(w =>
                {
                    if (res.Code != 0)
                    {
                        var chr = w.getPlayerStorage().GetCharacterClientById(res.Request.MasterId);
                        if (chr != null)
                        {
                            chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                        }
                        return;
                    }

                    w.getPlayerStorage().GetCharacterActor(res.VictimId)?.Send(m =>
                    {
                        var summoned = m.getCharacterById(res.VictimId);
                        if (summoned != null)
                        {
                            if (summoned.getEventInstance() == null)
                            {
                                w.NodeService.AdminService.WarpPlayerByName(summoned, res.WarpToName);
                            }
                        }
                    });
                });
            }

            protected override SummonPlayerByNameResponse Parse(ByteString data) => SummonPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class WarpPlayer : InternalSessionChannelHandler<WrapPlayerByNameResponse>
        {
            public WarpPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.WarpPlayer;

            protected override void HandleMessage(WrapPlayerByNameResponse res)
            {
                _server.Broadcast(w =>
                {
                    var chr = w.getPlayerStorage().GetCharacterClientById(res.Request.MasterId);
                    if (chr != null)
                    {
                        if (res.Code != 0)
                        {
                            chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                        }
                        else
                        {
                            chr.Client.ChangeChannel(res.TargetChannel);
                        }
                    }
                });
            }

            protected override WrapPlayerByNameResponse Parse(ByteString data) => WrapPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class MonitorChanged : InternalSessionChannelHandler<ToggleMonitorPlayerResponse>
        {
            public MonitorChanged(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.InvokeMonitor;

            protected override void HandleMessage(ToggleMonitorPlayerResponse res)
            {
                if (res.Code == 0)
                {
                    _server.PushChannelCommand(
                        new InvokeDropMessageCommand(res.Request.MasterId, -1, res.Request.TargetName + " is " + (res.IsMonitored ? "now being monitored." : "no longer being monitored.")));
                }
                else
                    _server.PushChannelCommand(
                        new InvokeDropMessageCommand(res.Request.MasterId, 5, $"未找到玩家：{res.Request.TargetName}"));
            }

            protected override ToggleMonitorPlayerResponse Parse(ByteString data) => ToggleMonitorPlayerResponse.Parser.ParseFrom(data);
        }

        public class Ban : InternalSessionChannelHandler<BanResponse>
        {
            public Ban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.BanPlayer;

            protected override void HandleMessage(BanResponse data)
            {
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.Request.OperatorId)?
                    .Send(m =>
                    {
                        var masterChr = m.getCharacterById(data.Request.OperatorId);
                        if (masterChr != null)
                        {
                            if (data.Code != 0)
                            {
                                masterChr.sendPacket(PacketCreator.getGMEffect(6, 1));
                                return;
                            }
                            else
                            {
                                masterChr.sendPacket(PacketCreator.getGMEffect(4, 0));
                            }
                        }
                    });

                    w.getPlayerStorage().GetCharacterActor(data.VictimId)?
                        .Send(m =>
                        {
                            var chr = m.getCharacterById(data.VictimId);

                            if (chr != null)
                            {
                                chr.Yellow(nameof(ClientMessage.Ban_NoticePlayer), data.OperatorName);
                                chr.yellowMessage(chr.GetMessageByKey(ClientMessage.BanReason) + data.Request.ReasonDesc);

                                Timer? timer = null;
                                timer = new System.Threading.Timer(_ =>
                                {
                                    m.Send(mm =>
                                    {
                                        mm.getCharacterById(data.VictimId)?.Client.Disconnect(false, false);
                                    });
                                    timer?.Dispose();
                                }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
                            }
                        });
                });
            }

            protected override BanResponse Parse(ByteString data) => BanResponse.Parser.ParseFrom(data);
        }

        public class Unban : InternalSessionChannelHandler<UnbanResponse>
        {
            public Unban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Unban;

            protected override void HandleMessage(UnbanResponse res)
            {
                if (res.Code == 0)
                {
                    _server.PushChannelCommand(new InvokeDropMessageCommand(res.Request.OperatorId, 5, "Unbanned " + res.Request.Victim));
                }
            }

            protected override UnbanResponse Parse(ByteString data) => UnbanResponse.Parser.ParseFrom(data);
        }

        public class Jail : InternalSessionChannelHandler<CreateJailResponse>
        {
            public Jail(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Jail;

            protected override void HandleMessage(CreateJailResponse res)
            {
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)?
                        .Send(m =>
                        {
                            var masterChr = m.getCharacterById(res.Request.MasterId);
                            if (masterChr != null)
                            {
                                if (res.Code != 0)
                                    masterChr.Pink(nameof(ClientMessage.PlayerNotFound));
                                else
                                {
                                    if (res.IsExtend)
                                        masterChr.Pink(nameof(ClientMessage.Jail_ExtendResult), res.Request.TargetName, res.Request.Minutes.ToString());
                                    else
                                        masterChr.Pink(nameof(ClientMessage.Jail_Result), res.Request.TargetName, res.Request.Minutes.ToString());
                                }
                            }
                        });

                    if (res.Code == 0)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.TargetId)?
                            .Send(m =>
                            {
                                m.getCharacterById(res.TargetId)?.addJailExpirationTime(res.Request.Minutes * 60000);
                            });
                    }
                });
            }

            protected override CreateJailResponse Parse(ByteString data) => CreateJailResponse.Parser.ParseFrom(data);
        }

        public class Unjail : InternalSessionChannelHandler<CreateUnjailResponse>
        {
            public Unjail(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Unjail;

            protected override void HandleMessage(CreateUnjailResponse res)
            {
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(res.Request.MasterId)?
                        .Send(m =>
                        {
                            var masterChr = m.getCharacterById(res.Request.MasterId);
                            if (masterChr != null)
                            {
                                if (res.Code == 1)
                                {
                                    masterChr.Pink(nameof(ClientMessage.PlayerNotFound));
                                }
                                else if (res.Code == 2)
                                {
                                    masterChr.Pink(nameof(ClientMessage.UnjailCommand_AlreadyFree));
                                }
                                else if (res.Code == 0)
                                {
                                    masterChr.Yellow(nameof(ClientMessage.Command_Done), masterChr.getLastCommandMessage());
                                }
                            }
                        });

                    if (res.Code == 0)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.TargetId)?
                            .Send(m =>
                            {
                                var chr = m.getCharacterById(res.TargetId);
                                if (chr != null)
                                {
                                    chr.removeJailExpirationTime();
                                    chr.Pink(nameof(ClientMessage.Unjail_Notify));
                                }
                            });
                    }
                });
            }

            protected override CreateUnjailResponse Parse(ByteString data) => CreateUnjailResponse.Parser.ParseFrom(data);
        }
    }
}
