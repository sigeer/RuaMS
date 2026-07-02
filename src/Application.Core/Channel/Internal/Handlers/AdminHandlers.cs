using Application.Core.Channel.AntiMacro;
using Application.Core.Channel.Commands;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Resources.Messages;
using Application.Shared.Message;
using Config;
using Dto;
using Google.Protobuf;
using JailProto;
using Microsoft.Extensions.DependencyInjection;
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

            protected override async Task HandleMessage(ReloadEventsResponse res)
            {
                await _server.BroadcastAsync(w =>
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

            protected override async Task HandleMessage(SummonPlayerByNameResponse res)
            {
                if (res.Code != 0)
                {
                    await _server.SendToPlayerAsync(res.Request.MasterId, async chr =>
                    {
                        await chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.VictimId, async chr =>
                    {
                        if (chr.getEventInstance() == null)
                        {
                            await _server.AdminService.WarpPlayerByName(chr, res.WarpToName);
                        }
                    });
                }
            }

            protected override SummonPlayerByNameResponse Parse(ByteString data) => SummonPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class WarpPlayer : InternalSessionChannelHandler<WrapPlayerByNameResponse>
        {
            public WarpPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.WarpPlayer;

            protected override async Task HandleMessage(WrapPlayerByNameResponse res)
            {
                await _server.SendToPlayerAsync(res.Request.MasterId, async chr =>
                {
                    if (res.Code != 0)
                    {
                        await chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                    }
                    else
                    {
                        await chr.Client.ChangeChannel(res.TargetChannel);
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

            protected override async Task HandleMessage(ToggleMonitorPlayerResponse res)
            {
                if (res.Code == 0)
                {
                    await _server.PushChannelCommandAsync(
                        new InvokeDropMessageAsyncCommand(res.Request.MasterId, -1, res.Request.TargetName + " is " + (res.IsMonitored ? "now being monitored." : "no longer being monitored.")));
                }
                else
                    await _server.PushChannelCommandAsync(
                        new InvokeDropMessageAsyncCommand(res.Request.MasterId, 5, $"未找到玩家：{res.Request.TargetName}"));
            }

            protected override ToggleMonitorPlayerResponse Parse(ByteString data) => ToggleMonitorPlayerResponse.Parser.ParseFrom(data);
        }

        public class Ban : InternalSessionChannelHandler<BanResponse>
        {
            public Ban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.BanPlayer;

            protected override async Task HandleMessage(BanResponse data)
            {
                if (data.Code != 0)
                {
                    await _server.SendToPlayerAsync(data.Request.OperatorId, async chr =>
                    {
                        await chr.SendPacket(PacketCreator.getGMEffect(6, 1));
                    });
                }
                else
                {
                    await _server.SendToPlayersAsync([data.Request.OperatorId, data.VictimId], async chr =>
                    {
                        if (chr.Id == data.Request.OperatorId)
                        {
                            await chr.SendPacket(PacketCreator.getGMEffect(4, 0));
                        }
                        else if (chr.Id == data.VictimId)
                        {
                            await chr.SendPacket(PacketCreator.sendPolice(string.Format("You have been blocked by the#b {0} Police for {1}.#k", "RuaMS", data.Request.ReasonDesc)));
                            await chr.Yellow(nameof(ClientMessage.Ban_NoticePlayer), data.OperatorName);
                            await chr.Yellow(chr.GetMessageByKey(ClientMessage.BanReason) + data.Request.ReasonDesc);

                            Timer? timer = null;
                            timer = new System.Threading.Timer(async _ =>
                            {
                                await _server.SendToPlayerAsync(data.VictimId, async nChr =>
                                {
                                    await nChr.Client.Disconnect(false, false);
                                });
                                timer?.Dispose();
                            }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
                        }
                        ;
                    });
                }
            }

            protected override BanResponse Parse(ByteString data) => BanResponse.Parser.ParseFrom(data);
        }

        public class Unban : InternalSessionChannelHandler<UnbanResponse>
        {
            public Unban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Unban;

            protected override async Task HandleMessage(UnbanResponse res)
            {
                if (res.Code == 0)
                {
                    await _server.PushChannelCommandAsync(new InvokeDropMessageAsyncCommand(res.Request.OperatorId, 5, "Unbanned " + res.Request.Victim));
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

            protected override async Task HandleMessage(CreateJailResponse res)
            {
                await _server.SendToPlayersAsync([res.Request.MasterId, res.TargetId], async chr =>
                {
                    if (chr.Id == res.Request.MasterId)
                    {
                        if (res.Code != 0)
                            await chr.Pink(nameof(ClientMessage.PlayerNotFound));
                        else
                        {
                            if (res.IsExtend)
                                await chr.Pink(nameof(ClientMessage.Jail_ExtendResult), res.Request.TargetName, res.Request.Minutes.ToString());
                            else
                                await chr.Pink(nameof(ClientMessage.Jail_Result), res.Request.TargetName, res.Request.Minutes.ToString());
                        }
                    }
                    else if (chr.Id == res.TargetId)
                    {
                        await chr.addJailExpirationTime(res.Request.Minutes * 60000);
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

            protected override async Task HandleMessage(CreateUnjailResponse res)
            {
                if (res.Code == 0)
                {
                    await _server.SendToPlayersAsync([res.Request.MasterId, res.TargetId], async chr =>
                     {
                         if (chr.Id == res.Request.MasterId)
                         {
                             await chr.Yellow(nameof(ClientMessage.Command_Done), chr.getLastCommandMessage());
                         }
                         else if (chr.Id == res.TargetId)
                         {
                             chr.removeJailExpirationTime();
                             await chr.Pink(nameof(ClientMessage.Unjail_Notify));
                         }
                     });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.MasterId, async chr =>
                    {
                        if (res.Code == 1)
                        {
                            await chr.Pink(nameof(ClientMessage.PlayerNotFound));
                        }
                        else if (res.Code == 2)
                        {
                            await chr.Pink(nameof(ClientMessage.UnjailCommand_AlreadyFree));
                        }
                    });
                }

            }

            protected override CreateUnjailResponse Parse(ByteString data) => CreateUnjailResponse.Parser.ParseFrom(data);
        }

        public class AntiMacroNotify : InternalSessionChannelHandler<AntiMacroNotifyMessage>
        {
            AntiMacroService _service;
            public AntiMacroNotify(WorldChannelServer server) : base(server)
            {
                _service = server.ServiceProvider.GetRequiredService<AntiMacroService>();
            }

            public override int MessageId => (int)ChannelRecvCode.AntiMacroNotify;

            protected override Task HandleMessage(AntiMacroNotifyMessage res)
            {
                return _service.PenalizeAsync(res);
            }

            protected override AntiMacroNotifyMessage Parse(ByteString data) => AntiMacroNotifyMessage.Parser.ParseFrom(data);
        }
    }
}
