using Application.Core.ServerTransports;
using Application.Shared.Events;
using Application.Shared.Login;
using Dto;
using Google.Protobuf.WellKnownTypes;
using System.Text;
using tools;

namespace Application.Core.Channel.Services
{
    public class AdminService
    {
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;

        public AdminService(IChannelServerTransport transport, WorldChannelServer server)
        {
            _transport = transport;
            _server = server;
        }

        public void AutoBan(IPlayer chr, int reason, string reasonDesc, int days, BanLevel level = BanLevel.All)
        {
            var res = _transport.Ban(new Dto.BanRequest
            {
                OperatorId = ServerConstants.SystemCId,
                Victim = chr.Name,
                Reason = reason,
                ReasonDesc = "[AutoBan] " + reasonDesc,
                BanLevel = (int)level,
                Days = days
            });
        }

        public void Ban(IPlayer chr, string victim, int reason, string reasonDesc, int days, BanLevel level = BanLevel.OnlyAccount)
        {
            var res = _transport.Ban(new Dto.BanRequest
            {
                OperatorId = chr.Id,
                Victim = victim,
                Reason = reason,
                ReasonDesc = reasonDesc,
                BanLevel = (int)level,
                Days = days
            });
            if (res.Code != 0)
            {
                chr.sendPacket(PacketCreator.getGMEffect(6, 1));
            }
            else
            {
                chr.sendPacket(PacketCreator.getGMEffect(4, 0));
            }
        }

        public void OnBannedNotify(Dto.BanBroadcast data)
        {
            var chr = _server.FindPlayerById(data.TargetId);
            if (chr != null)
            {
                chr.yellowMessage("You have been banned by #b" + data.OperatorName + " #k.");
                chr.yellowMessage("Reason: " + data.ReasonDesc);

                Timer? timer = null;
                timer = new System.Threading.Timer(_ =>
                {
                    chr.Client.CloseSession();

                    timer?.Dispose();
                }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            }

            _server.BroadcastGMPacket(PacketCreator.serverNotice(6, "[RIP]: " + data.TargetName + " has been banned."));
        }

        public void Unban(IPlayer chr, string victim)
        {
            var res = _transport.Unban(new Dto.UnbanRequest
            {
                OperatorId = chr.Id,
                Victim = victim,
            });
            if (res.Code == 0)
            {
                chr.message("Unbanned " + victim);
            }
        }

        internal void SetGmLevel(IPlayer chr, string victim, int newLevel)
        {
            var res = _transport.SetGmLevel(new SetGmLevelRequest { OperatorId = chr.Id, Level = newLevel, TargetName = victim });
            if (res.Code == 0)
            {
                chr.dropMessage(victim + " is now a level " + newLevel + " GM.");
            }
            else
            {
                chr.dropMessage("Player '" + victim + "' was not found on this channel.");
            }
        }

        public void OnSetGmLevelNotify(SetGmLevelBroadcast data)
        {
            var chr = _server.FindPlayerById(data.TargetId);
            if (chr != null)
            {
                chr.Client.AccountEntity!.GMLevel = (sbyte)data.Level;
                chr.dropMessage("You are now a level " + data.Level + " GM. See @commands for a list of available commands.");
            }
        }

        public OnlinedPlayerInfoDto[] GetOnlinedPlayers()
        {
            return _transport.GetOnlinedPlayers().List.ToArray();
        }

        /// <summary>
        /// 传送到玩家身边
        /// </summary>
        /// <param name="targetId"></param>
        [SupportRemoteCall(RemoteCallMethods.WarpPlayerByName)]
        public void WarpPlayerByName(IPlayer chr, string victim)
        {
            var sameChannelSearch = chr.Client.CurrentServer.Players.getCharacterByName(victim);
            if (sameChannelSearch != null)
            {
                var map = sameChannelSearch.getMap();
                chr.changeMap(map, sameChannelSearch.getPosition());
            }
            else
            {
                var res = _transport.WarpPlayerByName(new Dto.WrapPlayerByNameRequest { MasterId = chr.Id, Victim = victim });
                if (res.Code == 0)
                {
                    chr.Client.ChangeChannel(res.TargetChannel);
                }
                else
                {
                    chr.dropMessage($"玩家 {victim} 不存在或者不在线");
                }
            }
        }

        public void SummonPlayerByName(IPlayer chr, string victim)
        {
            var sameChannelSearch = chr.Client.CurrentServer.Players.getCharacterByName(victim);
            if (sameChannelSearch != null)
            {
                if (sameChannelSearch.getEventInstance() != null)
                {
                    // 处于事件中不能被传送走，避免影响其无法完成任务
                    chr.dropMessage($"玩家 {victim} 正处于事件中，无法离开");
                    return;
                }

                var map = chr.getMap();
                sameChannelSearch.changeMap(map, chr.getPosition());
            }
            else
            {
                var res = _transport.SummonPlayerByName(new Dto.SummonPlayerByNameRequest { MasterId = chr.Id, Victim = victim });
                if (res.Code != 0)
                {
                    chr.dropMessage($"玩家 {victim} 不存在或者不在线");
                }
            }
        }

        public void OnPlayerSummoned(Dto.SummonPlayerByNameBroadcast data)
        {
            var summoned = _server.FindPlayerById(data.MasterId);
            if (summoned != null)
            {
                if (summoned.getEventInstance() == null)
                {
                    WarpPlayerByName(summoned, data.WarpToName);
                }
            }
        }

        public void DisconnectPlayerByName(IPlayer chr, string victim)
        {
            var sameChannelSearch = chr.Client.CurrentServer.Players.getCharacterByName(victim);
            if (sameChannelSearch != null)
            {
                sameChannelSearch.Client.Disconnect(false);
            }
            else
            {
                var res = _transport.DisconnectPlayerByName(new Dto.DisconnectPlayerByNameRequest { MasterId = chr.Id, Victim = victim });
                if (res.Code != 0)
                {
                    chr.dropMessage($"玩家 {victim} 不存在或者不在线");
                }
            }
        }

        public void OnReceivedDisconnectCommand(Dto.DisconnectPlayerByNameBroadcast data)
        {
            var chr = _server.FindPlayerById(data.MasterId);
            if (chr != null)
            {
                chr.Client.Disconnect(false);
            }
        }

        public void DisconnectAll(IPlayer chr)
        {
            _transport.DisconnectAll(new Dto.DisconnectAllRequest { MasterId = chr.Id });
            chr.message("All players successfully disconnected.");
        }

        public void OnDisconnectAll(Empty data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                foreach (var chr in ch.Players.getAllCharacters())
                {
                    if (!chr.isGM())
                    {
                        chr.Client.Disconnect(false);
                    }
                }
            }
        }

        public List<Dto.ClientInfo> GetOnliendClientInfo()
        {
            Dto.GetAllClientInfo res = _transport.GetOnliendClientInfo();
            return res.List.ToList();
        }

        /// <summary>
        /// 停机
        /// </summary>
        /// <param name="delay">单位：秒。-1：立即</param>
        public void ShutdownMaster(IPlayer chr, int delay = -1)
        {
            chr.dropMessage("服务器正在停止中...");
            _transport.ShutdownMaster(new Config.ShutdownMasterRequest() { DelaySeconds = delay });
        }

        internal void SavelAll()
        {
            _transport.SaveAll(new Empty());
        }

        internal void OnSaveAll(Empty empty)
        {
            foreach (var chr in _server.PlayerStorage.getAllCharacters())
            {
                chr.saveCharToDB(setChannel: chr.Channel);
            }
            _server.BroadcastGMPacket(PacketCreator.serverNotice(5, "玩家数据已同步"));
        }

        internal string QueryExpeditionInfo(IPlayer onlinedCharacter)
        {
            var dataSource = _transport.GetExpeditionInfo().List;
            StringBuilder sb = new StringBuilder();
            foreach (var ch in dataSource)
            {
                sb.Append("==== 频道");
                sb.Append(ch.Channel);
                sb.Append(" ====");
                sb.Append("\r\n\r\n");
                var expeds = ch.Expeditions;
                if (expeds.Count == 0)
                {
                    sb.Append("无");
                    continue;
                }

                int id = 0;
                foreach (var exped in expeds)
                {
                    id++;
                    sb.Append("> Expedition " + id);

                    sb.Append(">> Type: " + EnumClassCache<ExpeditionType>.Values[exped.Type].name());
                    sb.Append(">> Status: " + (exped.Status == 1 ? "REGISTERING" : "UNDERWAY"));
                    sb.Append(">> Size: " + exped.Members.Count);
                    int memId = 1;
                    foreach (var e in exped.Members)
                    {
                        if (e.Id == exped.LeaderId)
                        {
                            sb.Append(">>> Leader: " + e.Name);
                        }
                        else
                        {
                            sb.Append(">>> Member " + memId + ": " + e.Name);
                            memId++;
                        }

                    }
                    sb.Append("\r\n\r\n");
                }
            }
            return sb.ToString();
        }
    }
}
