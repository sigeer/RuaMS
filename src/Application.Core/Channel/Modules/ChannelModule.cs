using Application.Core.Game.Life;
using Application.Resources.Messages;
using Microsoft.Extensions.Logging;
using net.server.guild;
using SyncProto;
using tools;

namespace Application.Core.Channel.Modules
{
    public sealed class ChannelModule : AbstractChannelModule
    {
        public ChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger) : base(server, logger)
        {
        }

        public override void OnPlayerChangeJob(SyncProto.PlayerFieldChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = _server.GuildManager.SoftGetGuild(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberJob(data.Id, data.JobId);
                    guild.broadcast(PacketCreator.jobMessage(0, data.JobId, data.Name), data.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(data.GuildId, data.Id, data.Level, data.JobId));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = _server.GuildManager.SoftGetAlliance(guild.AllianceId);
                        if (alliance != null)
                        {
                            alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, data.Id, data.Level, data.JobId), data.Id, -1);
                        }
                    }
                }
            }
            _server.TeamManager.ProcessTeamUpdate(data);

        }

        public override void OnPlayerLevelUp(SyncProto.PlayerFieldChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = _server.GuildManager.SoftGetGuild(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberLevel(data.Id, data.Level);
                    guild.broadcast(PacketCreator.levelUpMessage(0, data.Level, data.Name), data.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(data.GuildId, data.Id, data.Level, data.JobId));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = _server.GuildManager.SoftGetAlliance(guild.AllianceId);
                        if (alliance != null)
                        {
                            alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, data.Id, data.Level, data.JobId), data.Id, -1);
                        }
                    }

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
