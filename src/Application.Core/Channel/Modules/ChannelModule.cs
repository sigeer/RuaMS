using Application.Core.Channel.Commands;
using Application.Resources.Messages;
using Microsoft.Extensions.Logging;
using SyncProto;
using tools;

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
                _server.PushChannelCommand(new InvokeMultiDropMessageCommandPlus(
                    [-1], NoticeType.LightBlue, e => e.GetMessageByKey(
                            nameof(ClientMessage.Levelup_Congratulation),
                            CharacterViewDtoUtils.GetPlayerNameWithMedal(data.Name, e.Client.CurrentCulture.GetItemName(data.MedalItemId)),
                            data.Level.ToString(),
                            data.Name)));
            }
        }
    }
}
