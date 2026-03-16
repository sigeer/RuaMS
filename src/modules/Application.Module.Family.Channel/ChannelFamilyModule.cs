using Application.Core.Channel;
using Application.Core.Channel.Events;
using Application.Core.Channel.Modules;
using Application.Core.Game.Players;
using Application.Module.Family.Channel.Commands;
using Application.Module.Family.Channel.Models;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Constants;
using Application.Shared.Constants.Job;
using constants.game;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tools;

namespace Application.Module.Family.Channel
{
    internal class ChannelFamilyModule : AbstractChannelModule
    {
        readonly FamilyManager _familyManager;

        readonly FamilyConfigs _config;
        public ChannelFamilyModule(FamilyManager familyManager, ILogger<AbstractChannelModule> logger, WorldChannelServer server, IOptions<FamilyConfigs> options) : base(server, logger)
        {
            _familyManager = familyManager;
            _config = options.Value;
        }

        public override void OnMonsterReward(MonsterRewardEvent evt)
        {
            var family = _familyManager.GetFamilyByPlayerId(evt.ToPlayer.Id);
            if (family == null)
                return;

            var entry = family.getEntryByID(evt.ToPlayer.Id);
            if (entry != null)
            {
                int repGain = evt.Monster.isBoss() ? _config.FAMILY_REP_PER_BOSS_KILL : _config.FAMILY_REP_PER_KILL;
                if (evt.Monster.getMaxHp() <= 1)
                {
                    repGain = 0; //don't count trash mobs
                }
                entry.giveReputationToSenior(repGain, true);
            }
        }

        public override void OnPlayerLevelUp(SyncProto.PlayerFieldChange arg)
        {
            var family = _familyManager.GetFamilyByPlayerId(arg.Id);
            if (family == null)
                return;

            var familyEntry = family.getEntryByID(arg.Id);
            if (familyEntry != null)
            {
                familyEntry.giveReputationToSenior(_config.FAMILY_REP_PER_LEVELUP, true);
                var senior = familyEntry.getSenior();
                if (senior != null)
                {
                    //only send the message to direct senior
                    var seniorChr = _server.FindPlayerById(senior.Channel, senior.Id);
                    if (seniorChr != null)
                    {
                        seniorChr.sendPacket(PacketCreator.levelUpMessage(1, arg.Level, arg.Name));
                    }
                }
            }
        }

        public override void OnPlayerChangeJob(SyncProto.PlayerFieldChange arg)
        {
            var family = _familyManager.GetFamilyByPlayerId(arg.Id);
            if (family != null)
            {
                family.broadcast(PacketCreator.jobMessage(1, arg.JobId, arg.Name), arg.Id);

                var jobModel = JobFactory.GetById(arg.JobId);
                family.broadcast(PacketCreator.serverNotice(6, "[" + ClientCulture.SystemCulture.Ordinal(jobModel.Rank) + " Job] " + arg.Name + " has just become a " + ClientCulture.SystemCulture.GetJobName(jobModel) + "."), arg.Id);
            }
        }


        public override void OnPlayerLogin(int chrId)
        {
            _server.PushChannelCommand(new InvokePlayerLoginNotifyCommand(chrId));
        }
    }
}
