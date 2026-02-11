using Application.Shared.Message;
using Dto;
using System;
using System.Threading.Tasks;

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

        public async Task DropWorldMessage(int type, string message, bool onlyGM = false)
        {
            var msg = new MessageProto.DropMessageBroadcast { Type = type, Message = message };
            if (onlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);

                if (msg.Receivers.Count > 0)
                {
                    await Transport.BroadcastMessageN(ChannelRecvCode.DropTextMessage, msg);
                }
            }
            else
            {
                msg.Receivers.Add(-1);
                await Transport.BroadcastMessageN(ChannelRecvCode.DropTextMessage, msg);
            }
        }

        public async Task DropWorldMessage(int type, string message, int[] targets)
        {
            var msg = new MessageProto.DropMessageBroadcast { Type = type, Message = message };
            msg.Receivers.AddRange(targets);

            await Transport.BroadcastMessageN(ChannelRecvCode.DropTextMessage, msg);
        }

        public async Task BroadcastPacket(MessageProto.PacketRequest p)
        {
            var msg = new MessageProto.PacketBroadcast { Data = p.Data };
            if (p.OnlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);
                if (msg.Receivers.Count > 0)
                {
                    await Transport.SendMessageN(ChannelRecvCode.HandleFullPacket, msg, msg.Receivers);
                }
            }
            else
            {
                msg.Receivers.Add(-1);
                await Transport.BroadcastMessageN(ChannelRecvCode.HandleFullPacket, msg);
            }
        }

        public async Task BroadcastPacket(MessageProto.PacketRequest p, IEnumerable<int> chrIds)
        {
            var msg = new MessageProto.PacketBroadcast { Data = p.Data };
            msg.Receivers.AddRange(chrIds);
            await Transport.SendMessageN(ChannelRecvCode.HandleFullPacket, msg, msg.Receivers);
        }

        public async Task DropYellowTip(string message, bool onlyGM = false)
        {
            await DropWorldMessage(-1, message, onlyGM);
        }

        public async Task DropEarnTitleMessage(string message, bool onlyGM = false)
        {
            await DropWorldMessage(-2, message, onlyGM);
        }

        public void DisconnectChr(int chrId)
        {
            var data = new SystemProto.DisconnectPlayerByNameResponse() { TargetId = chrId, Request = new() };
            _ = Transport.SendMessageN(ChannelRecvCode.InvokeDisconnectPlayer, data, [chrId]);
        }
    }
}
