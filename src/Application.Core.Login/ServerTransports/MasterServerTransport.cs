using Application.Core.Login.Models;
using Application.Core.Login.ServerTransports;
using Application.Shared.Message;
using Application.Shared.Servers;

namespace Application.Core.Login
{
    public class MasterServerTransport : MasterServerTransportBase, IServerTransport
    {
        public MasterServerTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        public async Task SendNotes(int channel, int id, ProtoModel.NoteProto[] notes)
        {
            var data = new ProtoService.SendNoteResponse() { ReceiverChannel = channel, ReceiverId = id };
            data.List.AddRange(notes);
            await SendMessageN(ChannelRecvCode.InvokeNoteMessage, data, [data.ReceiverId]);
        }


        public async Task SendMultiChatAsync(int type, string nameFrom, IEnumerable<CharacterLiveObject> teamMember, string chatText)
        {
            var res = new ProtoModel.MultiChatMessage { Type = type, FromName = nameFrom, Text = chatText };
            res.Receivers.AddRange(teamMember.Select(x => x.Character.Id));

            await BroadcastMessageN(ChannelRecvCode.MultiChat, res);
        }

        public async Task BroadcastPlayerFieldChange(ChannelRecvCode evt, CharacterLiveObject obj, int fromChannel)
        {
            ProtoModel.PlayerFieldChange response = new ProtoModel.PlayerFieldChange
            {
                Channel = obj.Channel,
                FromChannel = fromChannel,
                FamilyId = obj.Character.FamilyId,
                GuildId = obj.Character.GuildId,
                TeamId = obj.Character.Party,
                Id = obj.Character.Id,
                JobId = obj.Character.JobId,
                Level = obj.Character.Level,
                MapId = obj.Character.Map,
                Name = obj.Character.Name,
                MedalItemId = obj.Character.Data.Bag.EquippedInv.FirstOrDefault(x => x.Position == EquipSlot.Medal)?.Itemid ?? 0,
            };
            response.Buddies.AddRange(obj.Character.Data.BuddyList.Select(x => x.Id));
            await BroadcastMessageN(evt, response);
        }


        internal async Task BroadcastShutdown()
        {
            await BroadcastMessageN(ChannelRecvCode.UnregisterChannel);
        }

        internal async Task SendNewYearCards(ProtoService.SendNewYearCardResponse response)
        {
            await SendMessageN(ChannelRecvCode.OnNewYearCardSent, response, [response.Request.FromId]);
        }

        internal async Task SendNewYearCardNotify(ProtoModel.NewYearCardNotifyProto response)
        {
            await SendMessageN(ChannelRecvCode.OnNewYearCardNotify, response, response.List.Select(x => x.MasterId).ToArray());
        }

        internal async Task SendNewYearCardDiscard(ProtoService.DiscardNewYearCardResponse response)
        {
            await BroadcastMessageN(ChannelRecvCode.OnNewYearCardDiscard, response);
        }


        internal async Task BroadcastPLifeCreated(ProtoService.CreatePLifeRequest request)
        {
            await BroadcastMessageN(ChannelRecvCode.OnPlifeCreated, request);
        }

        internal async Task BroadcastPLifeRemoved(ProtoService.RemovePLifeResponse request)
        {
            await BroadcastMessageN(ChannelRecvCode.OnPlifeRemoved, request);
        }
    }
}
