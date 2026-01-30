using Application.Core.Channel.Commands;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Application.Shared.Net;
using Microsoft.Extensions.Logging;
using net.server.guild;
using SyncProto;
using System.Threading.Channels;
using System.Xml.Linq;
using tools;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Core.Channel.Modules
{
    public sealed class ChannelModule : AbstractChannelModule
    {
        public ChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger) : base(server, logger)
        {
        }

        public override void OnPlayerServerChanged(PlayerFieldChange arg)
        {
            base.OnPlayerServerChanged(arg);

            _server.PushChannelCommand(new InvokeBuddyPacketCommand(arg.Id, arg.Buddies, PacketCreator.updateBuddyChannel(arg.Id, arg.Channel - 1)));
        }

        public override void OnPlayerChangeJob(SyncProto.PlayerFieldChange data)
        {
            if (YamlConfig.config.server.USE_ANNOUNCE_CHANGEJOB)
            {
                var jobModel = JobFactory.GetById(data.JobId);
                var packet = PacketCreator.serverNotice(6, 
                    $"[{ClientCulture.SystemCulture.Ordinal(jobModel.Rank)} Job] {data.Name} has just become a {ClientCulture.SystemCulture.GetJobName(jobModel)}.");

                _server.PushChannelCommand(new InvokeBuddyPacketCommand(data.Id, data.Buddies, packet));
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
