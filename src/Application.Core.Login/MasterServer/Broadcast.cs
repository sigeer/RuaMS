using Application.Shared.Message;
using Dto;
using System;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public Config.WorldConfig GetWorldConfig()
        {
            return new Config.WorldConfig
            {
                BossDropRate = BossDropRate,
                DropRate = DropRate,
                ExpRate = ExpRate,
                FishingRate = FishingRate,
                MesoRate = MesoRate,
                MobRate = MobRate,
                QuestRate = QuestRate,
                ServerMessage = ServerMessage,
                TravelRate = TravelRate
            };
        }

        public void DropWorldMessage(int type, string message, bool onlyGM)
        {
            if (onlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                DropWorldMessage(type, message, gmids);
            }
            else
            {
                DropWorldMessage(type, message);
            }
        }

        public void DropWorldMessage(int type, string message)
        {
            var msg = new MessageProto.DropMessageBroadcast { Type = type, Message = message };
            msg.Receivers.Add(-1);
            Transport.BroadcastMessage(BroadcastType.Broadcast_DropMessage, msg);
        }

        public void DropWorldMessage(int type, string message, int[] targets)
        {
            var msg = new MessageProto.DropMessageBroadcast { Type = type, Message = message };
            msg.Receivers.AddRange(targets);
            Transport.SendMessage(BroadcastType.Broadcast_DropMessage, msg, targets);
        }

        public void BroadcastPacket(MessageProto.PacketRequest p)
        {
            var msg = new MessageProto.PacketBroadcast { Data = p.Data };
            if (p.OnlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);
                Transport.SendMessage(BroadcastType.Broadcast_Packet, msg, gmids);
            }
            else
            {
                msg.Receivers.Add(-1);
                Transport.BroadcastMessage(BroadcastType.Broadcast_Packet, msg);
            }
        }

        public void BroadcastPacket(MessageProto.PacketRequest p, IEnumerable<int> chrIds)
        {
            var msg = new MessageProto.PacketBroadcast { Data = p.Data };
            msg.Receivers.AddRange(chrIds);
            Transport.SendMessage(BroadcastType.Broadcast_Packet, msg, msg.Receivers);
        }

        public void DropYellowTip(string message, bool onlyGM = false)
        {
            var msg = new MessageProto.YellowTipBroadcast { Message = message };
            if (onlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);
                Transport.SendMessage(BroadcastType.Broadcast_YellowTip, msg, gmids);
            }
            else
            {
                msg.Receivers.Add(-1);
                Transport.BroadcastMessage(BroadcastType.Broadcast_YellowTip, msg);
            }
        }

        public void DropEarnTitleMessage(string message, bool onlyGM = false)
        {
            var msg = new MessageProto.EarnTitleMessageBroadcast { Message = message };
            if (onlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);
                Transport.SendMessage(BroadcastType.Broadcast_EarnTitleMessage, msg, gmids);
            }
            else
            {
                msg.Receivers.Add(-1);
                Transport.BroadcastMessage(BroadcastType.Broadcast_EarnTitleMessage, msg);
            }
        }

        public void DisconnectChr(int chrId)
        {
            var data = new SystemProto.DisconnectPlayerByNameBroadcast() { MasterId = chrId };
            Transport.SendMessage(BroadcastType.SendPlayerDisconnect, data, [data.MasterId]);
        }
    }
}
