using Application.Core.Channel.Net.Packets;
using Application.Resources.Messages;
using Application.Shared.Team;
using TeamProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeTeamUpdateCommand : IWorldChannelCommand
    {
        UpdateTeamResponse res;

        public InvokeTeamUpdateCommand(UpdateTeamResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var operation = (PartyOperation)res.Request.Operation;
            var errorCode = (UpdateTeamCheckResult)res.Code;
            if (errorCode != UpdateTeamCheckResult.Success)
            {
                var operatorPlayer = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.FromId);
                if (operatorPlayer != null && operation != PartyOperation.SILENT_UPDATE && operation != PartyOperation.LOG_ONOFF)
                {
                    // 人数已满
                    if (errorCode == UpdateTeamCheckResult.Join_TeamMemberFull)
                        operatorPlayer.sendPacket(TeamPacketCreator.TeamFullCapacity());
                    // 队伍已解散
                    if (errorCode == UpdateTeamCheckResult.TeamNotExsited)
                        operatorPlayer.Pink(nameof(ClientMessage.Team_TeamNotFound));
                    // 已有队伍
                    if (errorCode == UpdateTeamCheckResult.Join_HasTeam)
                        operatorPlayer.sendPacket(TeamPacketCreator.AlreadInTeam());
                }
                return;
            }

            // ctx.WorldChannel.NodeService.TeamManager.SetTeam(res.Team);

            var partyMembers = res.Team.Members.Select(x => ctx.WorldChannel.getPlayerStorage().getCharacterById(x.Id))
                .Where(x => x != null && x.isLoggedinWorld()).ToList()!;

            foreach (var partychar in partyMembers)
            {
                partychar!.Party = operation == PartyOperation.DISBAND ? -1 : res.Team.Id;
                partychar.sendPacket(TeamPacketCreator.UpdateParty(partychar.getChannelServer(), res.Team, operation, res.Request.TargetId, res.TargetName));
            }

            var targetPlayer = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.TargetId);
            if (operation == PartyOperation.JOIN)
            {
                if (targetPlayer != null)
                {
                    foreach (var partychar in partyMembers)
                    {
                        if (partychar!.Map == targetPlayer.Map && targetPlayer.Channel == partychar.Channel)
                        {
                            partychar.sendPacket(TeamPacketCreator.updatePartyMemberHP(targetPlayer.Id, targetPlayer.HP, targetPlayer.ActualMaxHP));
                            targetPlayer.sendPacket(TeamPacketCreator.updatePartyMemberHP(partychar.Id, partychar.HP, partychar.ActualMaxHP));
                        }
                    }
                    targetPlayer.HandleTeamMemberCountChanged(null);
                }
            }
            else if (operation == PartyOperation.LEAVE)
            {
                if (targetPlayer != null)
                {
                    var partymembers = targetPlayer.getPartyMembersOnline();

                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.leftParty(targetPlayer);
                    }

                    targetPlayer.Party = -1;
                    targetPlayer.sendPacket(TeamPacketCreator.UpdateParty(targetPlayer.getChannelServer(), res.Team, operation, res.Request.TargetId, res.TargetName));
                    targetPlayer.HandleTeamMemberCountChanged(partymembers);

                    if (res.Request.Reason == 1)
                    {
                        targetPlayer.showHint("You have reached #blevel 10#k, therefore you must leave your #rstarter party#k.");
                    }
                }
            }
            else if (operation == PartyOperation.DISBAND)
            {
                if (targetPlayer != null)
                {
                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.disbandParty();
                    }
                }

                // ctx.WorldChannel.NodeService.TeamManager.ClearTeamCache(res.Team.Id);
            }
            else if (operation == PartyOperation.EXPEL)
            {
                if (targetPlayer != null)
                {
                    var preData = targetPlayer.getPartyMembersOnline();

                    var eim = targetPlayer.getEventInstance();
                    if (eim != null)
                    {
                        eim.leftParty(targetPlayer);
                    }

                    targetPlayer.Party = -1;
                    targetPlayer.sendPacket(TeamPacketCreator.UpdateParty(targetPlayer.getChannelServer(), res.Team, operation, res.Request.TargetId, res.TargetName));
                    targetPlayer.HandleTeamMemberCountChanged(preData);
                }
            }
            else if (operation == PartyOperation.CHANGE_LEADER)
            {
                var mc = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.FromId);
                if (mc != null && targetPlayer != null && mc.Channel == targetPlayer.Channel)
                {
                    var eim = mc.getEventInstance();

                    if (eim != null && eim.isEventLeader(mc))
                    {
                        eim.changedLeader(targetPlayer);
                    }
                    else
                    {
                        int oldLeaderMapid = mc.getMapId();

                        if (MiniDungeonInfo.isDungeonMap(oldLeaderMapid))
                        {
                            if (oldLeaderMapid != targetPlayer.getMapId() && ctx.WorldChannel.Id == mc.Channel)
                            {
                                var mmd = ctx.WorldChannel.getMiniDungeon(oldLeaderMapid);
                                if (mmd != null)
                                {
                                    mmd.close();
                                }
                            }
                        }
                    }
                }
            }
            return;
        }
    }
}
