using Application.Core.Models;
using Application.Core.scripting.npc;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Events;
using Application.Shared.Languages;
using Application.Shared.Login;
using Application.Shared.Message;
using AutoMapper;
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
        readonly IMapper _mapper;

        public AdminService(IChannelServerTransport transport, WorldChannelServer server, IMapper mapper)
        {
            _transport = transport;
            _server = server;
            _mapper = mapper;
        }

        public void AutoBan(IPlayer chr, int reason, string reasonDesc, int days, BanLevel level = BanLevel.All)
        {
            var res = _transport.Ban(new SystemProto.BanRequest
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
            _ = _transport.Ban(new SystemProto.BanRequest
            {
                OperatorId = chr.Id,
                Victim = victim,
                Reason = reason,
                ReasonDesc = reasonDesc,
                BanLevel = (int)level,
                Days = days
            });
        }

        public void Unban(IPlayer chr, string victim)
        {
            _ = _transport.Unban(new SystemProto.UnbanRequest
            {
                OperatorId = chr.Id,
                Victim = victim,
            });
        }

        internal void SetGmLevel(IPlayer chr, string victim, int newLevel)
        {
            _ = _transport.SetGmLevel(new SystemProto.SetGmLevelRequest { OperatorId = chr.Id, Level = newLevel, TargetName = victim });
        }


        public void SetFly(IPlayer chr, bool v)
        {
            var data = _transport.SendSetFly(new ConfigProto.SetFlyRequest { CId = chr.Id, SetStatus = v });
            if (data.Code == 0)
            {
                string sendStr = "";
                if (data.Request.SetStatus)
                {
                    sendStr += "Enabled Fly feature (F1). With fly active, you cannot attack.";
                    if (!chr.Client.AccountEntity!.CanFly)
                    {
                        sendStr += " Re-login to take effect.";
                    }
                }
                else
                {
                    sendStr += "Disabled Fly feature. You can now attack.";
                    if (chr.Client.AccountEntity!.CanFly)
                    {
                        sendStr += " Re-login to take effect.";
                    }
                }

                chr.dropMessage(sendStr);
            }
        }

        public SystemProto.OnlinedPlayerInfoDto[] GetOnlinedPlayers()
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
                _ = _transport.WarpPlayerByName(new SystemProto.WrapPlayerByNameRequest { MasterId = chr.Id, Victim = victim });
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
                _ = _transport.SummonPlayerByName(new SystemProto.SummonPlayerByNameRequest { MasterId = chr.Id, Victim = victim });
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
                _ = _transport.DisconnectPlayerByName(new SystemProto.DisconnectPlayerByNameRequest { MasterId = chr.Id, Victim = victim });
            }
        }


        public void DisconnectAll(IPlayer chr)
        {
            _ = _server.Transport.DisconnectAllNotifyAsync();
            // _transport.DisconnectAll(new SystemProto.DisconnectAllRequest { MasterId = chr.Id });
            chr.Pink(ClientMessage.Command_Done, "dcall");
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
            _transport.ShutdownMaster(new SystemProto.ShutdownMasterRequest() { DelaySeconds = delay });
        }

        internal void SavelAll()
        {
            _server.Transport.SaveAllNotifyAsync();
        }


        internal string QueryExpeditionInfo(IPlayer onlinedCharacter)
        {
            return "";
            //var dataSource = _transport.GetExpeditionInfo().List;
            //StringBuilder sb = new StringBuilder();
            //foreach (var ch in dataSource)
            //{
            //    sb.Append("==== 频道");
            //    sb.Append(ch.Channel);
            //    sb.Append(" ====");
            //    sb.Append("\r\n\r\n");
            //    var expeds = ch.Expeditions;
            //    if (expeds.Count == 0)
            //    {
            //        sb.Append("无");
            //        continue;
            //    }

            //    int id = 0;
            //    foreach (var exped in expeds)
            //    {
            //        id++;
            //        sb.Append("> Expedition " + id);

            //        sb.Append(">> Type: " + EnumClassCache<ExpeditionType>.Values[exped.Type].name());
            //        sb.Append(">> Status: " + (exped.Status == 1 ? "REGISTERING" : "UNDERWAY"));
            //        sb.Append(">> Size: " + exped.Members.Count);
            //        int memId = 1;
            //        foreach (var e in exped.Members)
            //        {
            //            if (e.Id == exped.LeaderId)
            //            {
            //                sb.Append(">>> Leader: " + e.Name);
            //            }
            //            else
            //            {
            //                sb.Append(">>> Member " + memId + ": " + e.Name);
            //                memId++;
            //            }

            //        }
            //        sb.Append("\r\n\r\n");
            //    }
            //}
            //return sb.ToString();
        }

        internal ServerState GetServerStats()
        {
            return _mapper.Map<ServerState>(_transport.GetServerState());
        }

        //public void printSessionTrace(IChannelClient c)
        //{
        //    string str = "Opened server sessions:\r\n\r\n";

        //    if (onlineClients.Count > 0)
        //    {
        //        var elist = onlineClients.OrderBy(x => x.Key).ToList();

        //        str += ("Current online clients:\r\n");
        //        foreach (var e in elist)
        //        {
        //            str += ("  " + e.Key + "\r\n");
        //        }
        //    }

        //    if (onlineRemoteHwids.Count > 0)
        //    {
        //        List<Hwid> hwids = onlineRemoteHwids.OrderBy(x => x.hwid).ToList();

        //        str += ("Current online HWIDs:\r\n");
        //        foreach (Hwid s in hwids)
        //        {
        //            str += ("  " + s + "\r\n");
        //        }
        //    }

        //    if (loginRemoteHosts.Count > 0)
        //    {
        //        var elist = loginRemoteHosts.OrderBy(x => x.Key).ToList();

        //        str += ("Current login sessions:\r\n");
        //        foreach (var e in elist)
        //        {
        //            str += ("  " + e.Key + ", IP: " + e.Value.RemoteAddress + "\r\n");
        //        }
        //    }

        //    TempConversation.Create(c, NpcId.TEMPLE_KEEPER)?.RegisterTalk(str);
        //}

        public List<string> GetChannelServerTasks()
        {
            return _server.TimerManager.TaskScheduler.Keys.OrderBy(x => x).ToList();
        }
    }
}
