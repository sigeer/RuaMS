using Application.Shared.Message;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        public ProtoModel.WorldConfig GetWorldConfig()
        {
            return new ProtoModel.WorldConfig
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
            var msg = new ProtoModel.DropMessageBroadcastProto { Type = type, Message = message };
            if (onlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);
                await Transport.SendMessageN(ChannelRecvCode.DropTextMessage, msg, msg.Receivers);
            }
            else
            {
                msg.Receivers.Add(-1);
                await Transport.BroadcastMessageN(ChannelRecvCode.DropTextMessage, msg);
            }
        }

        public async Task DropWorldMessage(int type, string message, int[] targets)
        {
            var msg = new ProtoModel.DropMessageBroadcastProto { Type = type, Message = message };
            msg.Receivers.AddRange(targets);

            await Transport.SendMessageN(ChannelRecvCode.DropTextMessage, msg, msg.Receivers);
        }

        public async Task BroadcastPacket(ProtoService.PacketRequest p)
        {
            var msg = new ProtoModel.PacketBroadcastProto { Data = p.Data };
            if (p.OnlyGM)
            {
                var gmids = CharacterManager.GetOnlinedGMs();
                msg.Receivers.AddRange(gmids);
                await Transport.SendMessageN(ChannelRecvCode.HandleFullPacket, msg, msg.Receivers);
            }
            else
            {
                msg.Receivers.Add(-1);
                await Transport.BroadcastMessageN(ChannelRecvCode.HandleFullPacket, msg);
            }
        }

        public async Task BroadcastPacket(ProtoService.PacketRequest p, IEnumerable<int> chrIds)
        {
            var msg = new ProtoModel.PacketBroadcastProto { Data = p.Data };
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
            var data = new ProtoService.DisconnectPlayerByNameResponse() { TargetId = chrId, Request = new() };
            _ = Transport.SendMessageN(ChannelRecvCode.InvokeDisconnectPlayer, data, [chrId]);
        }
    }
}
