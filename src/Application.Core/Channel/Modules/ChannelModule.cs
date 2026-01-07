using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Microsoft.Extensions.Logging;
using net.server.guild;
using SyncProto;
using System.Xml.Linq;
using tools;
using XmlWzReader;

namespace Application.Core.Channel.Modules
{
    public sealed class ChannelModule : AbstractChannelModule
    {
        public ChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger) : base(server, logger)
        {
        }

        public override void OnPlayerChangeJob(SyncProto.PlayerFieldChange data)
        {
            if (YamlConfig.config.server.USE_ANNOUNCE_CHANGEJOB)
            {
                var jobModel = JobFactory.GetById(data.JobId);
                var packet = PacketCreator.serverNotice(6, 
                    $"[{ClientCulture.SystemCulture.Ordinal(jobModel.Rank)} Job] {data.Name} has just become a {ClientCulture.SystemCulture.GetJobName(jobModel)}.");

                foreach (var buddy in data.Buddies)
                {
                    var buddyChr = _server.FindPlayerById(buddy);
                    if (buddyChr != null && buddyChr.BuddyList.Contains(data.Id))
                    {
                        buddyChr.sendPacket(packet);
                    }
                }
            }
        }

        public override void OnPlayerLevelUp(SyncProto.PlayerFieldChange data)
        {
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
        }
    }
}
