using Application.Core.Login.Models;
using Application.Core.Login.ServerTransports;
using Application.Shared.Message;
using Application.Shared.Servers;
using CashProto;
using Config;
using Dto;
using Google.Protobuf;
using Humanizer;
using MessageProto;
using System.Threading.Tasks;

namespace Application.Core.Login
{
    public class MasterServerTransport : MasterServerTransportBase, IServerTransport
    {
        public MasterServerTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        public async Task SendNotes(int channel, int id, Dto.NoteDto[] notes)
        {
            var data = new SendNoteResponse() { ReceiverChannel = channel, ReceiverId = id };
            data.List.AddRange(notes);
            await SendMessageN(ChannelRecvCode.InvokeNoteMessage, data, [data.ReceiverId]);
        }


        public async Task SendMultiChatAsync(int type, string nameFrom, IEnumerable<CharacterLiveObject> teamMember, string chatText)
        {
            var res = new MultiChatMessage { Type = type, FromName = nameFrom, Text = chatText };
            res.Receivers.AddRange(teamMember.Select(x => x.Character.Id));

            await BroadcastMessageN(ChannelRecvCode.MultiChat, res);
        }

        public async Task BroadcastPlayerFieldChange(ChannelRecvCode evt, CharacterLiveObject obj, int fromChannel)
        {
            SyncProto.PlayerFieldChange response = new SyncProto.PlayerFieldChange
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
                MedalItemId = obj.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal)?.Itemid ?? 0,
            };
            response.Buddies.AddRange(obj.BuddyList.Keys);
            await BroadcastMessageN(evt, response);
        }


        internal async Task BroadcastShutdown()
        {
            await BroadcastMessageN(ChannelRecvCode.UnregisterChannel);
        }

        internal async Task SendNewYearCards(SendNewYearCardResponse response)
        {
            await SendMessageN(ChannelRecvCode.OnNewYearCardSent, response, [response.Request.FromId]);
        }

        internal async Task SendNewYearCardNotify(NewYearCardNotifyDto response)
        {
            await SendMessageN(ChannelRecvCode.OnNewYearCardNotify, response, response.List.Select(x => x.MasterId).ToArray());
        }

        internal async Task SendNewYearCardDiscard(DiscardNewYearCardResponse response)
        {
            await BroadcastMessageN(ChannelRecvCode.OnNewYearCardDiscard, response);
        }


        internal async Task BroadcastPLifeCreated(LifeProto.CreatePLifeRequest request)
        {
            await BroadcastMessageN(ChannelRecvCode.OnPlifeCreated, request);
        }

        internal async Task BroadcastPLifeRemoved(LifeProto.RemovePLifeResponse request)
        {
            await BroadcastMessageN(ChannelRecvCode.OnPlifeRemoved, request);
        }
    }
}
