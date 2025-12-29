using Application.Core.Game.Life;
using Application.Resources.Messages;
using Microsoft.Extensions.Logging;
using net.server.guild;
using SyncProto;
using tools;
using XmlWzReader;

namespace Application.Core.Channel.Modules
{
    public sealed class ChannelModule : AbstractChannelModule
    {
        public ChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger) : base(server, logger)
        {
        }

        public override void OnPlayerServerChanged(PlayerFieldChange data)
        {
            base.OnPlayerServerChanged(data);

            if (data.GuildId > 0)
            {
                var guild = _server.GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.OnMemberChannelChanged(data.Id, data.Channel);
                }
            }
        }

        public override void OnPlayerChangeJob(SyncProto.PlayerFieldChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = _server.GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.OnMemberJobChanged(data.Id, data.JobId);
                }
            }
            _server.TeamManager.ProcessTeamUpdate(data);

        }

        public override void OnPlayerLevelUp(SyncProto.PlayerFieldChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = _server.GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.OnMemberLevelChanged(data.Id, data.Level);
                }
            }

            if (data.Level == JobFactory.GetById(data.JobId).MaxLevel)
            {
                foreach (var ch in _server.Servers.Values)
                {
                    ch.LightBlue(e =>
                        e.GetMessageByKey(
                            nameof(ClientMessage.Levelup_Congratulation),
                            CharacterViewDtoUtils.GetPlayerNameWithMedal(data.Name, e.GetItemName(data.MedalItemId)),
                            data.Level.ToString(),
                            data.Name)
                        );
                }
            }

            _server.TeamManager.ProcessTeamUpdate(data);
        }
    }
}
