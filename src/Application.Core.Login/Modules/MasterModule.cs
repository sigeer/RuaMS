using Application.Core.Login.Models;
using Application.Shared.Message;
using Application.Shared.Team;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Modules
{
    public sealed class MasterModule : AbstractMasterModule
    {
        public MasterModule(MasterServer server, ILogger<MasterModule> logger) : base(server, logger)
        {
        }

        public override async Task OnPlayerServerChanged(CharacterLiveObject obj, int lastChannel)
        {
            await base.OnPlayerServerChanged(obj, lastChannel);

            await _server.ChatRoomManager.LeaveChatRoom(new Dto.LeaveChatRoomRequst { MasterId = obj.Character.Id });
            await _server.InvitationManager.RemovePlayerInvitation(obj.Character.Id);

            var data = new SyncProto.PlayerFieldChange
            {
                Channel = obj.Channel,
                FamilyId = obj.Character.FamilyId,
                GuildId = obj.Character.GuildId,
                TeamId = obj.Character.Party,
                Id = obj.Character.Id,
                JobId = obj.Character.JobId,
                Level = obj.Character.Level,
                MapId = obj.Character.Map,
                Name = obj.Character.Name,
                MedalItemId = obj.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal)?.Itemid ?? 0,
                FromChannel = lastChannel,
            };
            data.Buddies.AddRange(obj.BuddyList.Keys);
            await _server.Transport.BroadcastMessageN(ChannelRecvCode.OnPlayerServerChanged, data);
        }
        /// <summary>
        /// 进入频道
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isNewComer"></param>
        public override async Task OnPlayerLogin(CharacterLiveObject obj)
        {
            await _server.NoteManager.SendNote(obj);

            await _server.DueyManager.SendDueyNotifyOnLogin(obj.Character.Id);
        }

        public override async Task OnPlayerLogoff(CharacterLiveObject obj)
        {
            obj.Character.LastLogoutTime = _server.GetCurrentTimeDateTimeOffset();
            obj.ChannelNode = null;
        }

        public override async Task OnPlayerMapChanged(CharacterLiveObject obj)
        {
            await _server.TeamManager.UpdateParty(obj.Character.Party, PartyOperation.SILENT_UPDATE, obj.Character.Id, obj.Character.Id);
        }

        public override async Task OnPlayerJobChanged(CharacterLiveObject origin)
        {
            await _server.Transport.BroadcastPlayerJobChanged(origin);
        }

        public override async Task OnPlayerLevelChanged(CharacterLiveObject origin)
        {
            await _server.Transport.BroadcastPlayerLevelChanged(origin);
        }
    }
}
