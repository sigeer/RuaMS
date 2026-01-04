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

            await _server.Transport.BroadcastPlayerFieldChange(ChannelRecvCode.OnPlayerServerChanged, obj, lastChannel);
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
            await _server.Transport.BroadcastPlayerFieldChange(ChannelRecvCode.OnPlayerJobChanged, origin, origin.Channel);
        }

        public override async Task OnPlayerLevelChanged(CharacterLiveObject origin)
        {
            await _server.Transport.BroadcastPlayerFieldChange(ChannelRecvCode.OnPlayerLevelChanged, origin, origin.Channel);
        }
    }
}
