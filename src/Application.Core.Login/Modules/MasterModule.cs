using Application.Core.Login.Models;
using Application.Shared.Team;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Modules
{
    public sealed class MasterModule : AbstractMasterModule
    {
        public MasterModule(MasterServer server, ILogger<MasterModule> logger) : base(server, logger)
        {
        }


        /// <summary>
        /// 进入频道
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isNewComer"></param>
        public override void OnPlayerLogin(CharacterLiveObject obj, bool isNewComer)
        {
            _server.Transport.BroadcastPlayerLoginOff(new Dto.PlayerOnlineChange
            {
                Id = obj.Character.Id,
                Name = obj.Character.Name,
                GuildId = obj.Character.GuildId,
                TeamId = obj.Character.Party,
                FamilyId = obj.Character.FamilyId,
                Channel = obj.Channel,
                IsNewComer = isNewComer
            });
            _server.BuddyManager.BroadcastNotify(obj);
            _server.TeamManager.UpdateParty(obj.Character.Party, PartyOperation.LOG_ONOFF, obj.Character.Id, obj.Character.Id);
            _server.NoteManager.SendNote(obj);
        }

        public override void OnPlayerLogoff(CharacterLiveObject obj)
        {
            _server.Transport.BroadcastPlayerLoginOff(new Dto.PlayerOnlineChange
            {
                Id = obj.Character.Id,
                Name = obj.Character.Name,
                GuildId = obj.Character.GuildId,
                TeamId = obj.Character.Party,
                FamilyId = obj.Character.FamilyId,
                Channel = obj.Channel,
            });

            _server.TeamManager.UpdateParty(obj.Character.Party, PartyOperation.LOG_ONOFF, obj.Character.Id, obj.Character.Id);
        }

        public override void OnPlayerMapChanged(CharacterLiveObject obj)
        {
            _server.TeamManager.UpdateParty(obj.Character.Party, PartyOperation.SILENT_UPDATE, obj.Character.Id, obj.Character.Id);
        }

        public override void OnPlayerJobChanged(CharacterLiveObject origin)
        {
            _server.Transport.BroadcastPlayerJobChanged(origin);
        }

        public override void OnPlayerLevelChanged(CharacterLiveObject origin)
        {
            _server.Transport.BroadcastPlayerLevelChanged(origin);
        }
    }
}
