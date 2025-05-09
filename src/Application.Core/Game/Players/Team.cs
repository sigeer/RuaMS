using Application.Core.Game.Relation;
using Application.Core.Managers;
using Application.Shared.Relations;
using net.server.coordinator.matchchecker;
using System.Numerics;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public void setParty(ITeam? p)
        {
            Monitor.Enter(prtLock);
            try
            {
                if (p == null)
                {
                    doorSlot = -1;

                    TeamModel = null;
                }
                else
                {
                    TeamModel = p;
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public void updatePartyMemberHP()
        {
            Monitor.Enter(prtLock);
            try
            {
                receivePartyMemberHP();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public bool isLeader()
        {
            return isPartyLeader();
        }
        public bool isPartyLeader()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel?.getLeaderId() == getId();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }


        public ITeam? getParty()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel;
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public int getPartyId()
        {
            Monitor.Enter(prtLock);
            try
            {
                return TeamModel?.getId() ?? -1;
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public List<IPlayer> getPartyMembersOnline()
        {
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    return TeamModel.GetChannelMembers();
                }
                return new List<IPlayer>();
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public List<IPlayer> getPartyMembersOnSameMap()
        {
            List<IPlayer> list = new();
            int thisMapHash = this.MapModel.GetHashCode();

            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    foreach (var chr in TeamModel.GetChannelMembers())
                    {
                        var chrMap = chr.getMap();
                        // 用hashcode判断地图是否相同？ -- 同一频道、同一mapid
                        if (chrMap != null && chrMap.GetHashCode() == thisMapHash && chr.isLoggedinWorld())
                        {
                            list.Add(chr);
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }

            return list;
        }

        public bool isPartyMember(IPlayer chr)
        {
            return isPartyMember(chr.getId());
        }

        public bool isPartyMember(int cid)
        {
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    return TeamModel.getMemberById(cid) != null;
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }

            return false;
        }

        public void receivePartyMemberHP()
        {
            Monitor.Enter(prtLock);
            try
            {
                if (TeamModel != null)
                {
                    foreach (var player in this.getPartyMembersOnSameMap())
                    {
                        sendPacket(PacketCreator.updatePartyMemberHP(player.getId(), player.HP, player.ActualMaxHP));
                    }
                }
            }
            finally
            {
                Monitor.Exit(prtLock);
            }
        }

        public bool LeaveParty(bool disbandTeam = true)
        {
            lock (prtLock)
            {
                var party = getParty();
                if (party == null)
                    return false;

                if (Id == party.getLeaderId() && !disbandTeam)
                {
                    party.AssignNewLeader();
                }


                var channel = this.getChannelServer();

                if (Id == party.getLeaderId())
                {
                    var mcpq = getMonsterCarnival();
                    if (mcpq != null)
                    {
                        mcpq.leftParty(this);
                    }

                    channel.UpdateTeamGlobalData(party.getId(), PartyOperation.DISBAND, Id, Name);

                    var eim = getEventInstance();
                    if (eim != null)
                    {
                        eim.disbandParty();
                    }
                }
                else
                {
                    var mcpq = getMonsterCarnival();
                    if (mcpq != null)
                    {
                        mcpq.leftParty(this);
                    }

                    channel.UpdateTeamGlobalData(party.getId(), PartyOperation.LEAVE, Id, Name);

                    var eim = getEventInstance();
                    if (eim != null)
                    {
                        eim.leftParty(this);
                    }
                }

                setParty(null);

                var world = getWorldServer();
                MatchCheckerCoordinator mmce = world.getMatchCheckerCoordinator();
                if (mmce.getMatchConfirmationLeaderid(getId()) == getId() && mmce.getMatchConfirmationType(getId()) == MatchCheckerType.GUILD_CREATION)
                {
                    mmce.dismissMatchConfirmation(getId());
                }
                return true;
            }

        }

        public bool JoinParty(int partyid, bool silentCheck)
        {
            var playerParty = getParty();
            if (playerParty != null)
            {
                if (!silentCheck)
                {
                    sendPacket(PacketCreator.serverNotice(5, "You can't join the party as you are already in one."));
                }
                return false;
            }


            var channelServer = getChannelServer();
            var party = channelServer.GetLocalTeam(partyid);
            if (party == null)
            {
                sendPacket(PacketCreator.serverNotice(5, "You couldn't join the party since it had already been disbanded."));
                return false;
            }

            if (party.getMembers().Count < 6)
            {
                if (!silentCheck)
                {
                    sendPacket(PacketCreator.partyStatusMessage(17));
                }
                return false;
            }

            channelServer.UpdateTeamGlobalData(party.getId(), PartyOperation.JOIN, Id, Name);
            receivePartyMemberHP();
            updatePartyMemberHP();

            resetPartySearchInvite(party.getLeaderId());
            updatePartySearchAvailability(false);
            partyOperationUpdate(party, null);
            return true;;
        }

        public void ExpelFromParty(int expelCid)
        {
            getChannelServer().ExpelFromParty(Id, expelCid);
        }

        public bool CreateParty(bool silentCheck)
        {
            var party = getParty();
            if (party != null)
            {
                if (!silentCheck)
                {
                    sendPacket(PacketCreator.partyStatusMessage(16));
                }
                return false;
            }

            if (Level < 10 && !YamlConfig.config.server.USE_PARTY_FOR_STARTERS)
            {
                sendPacket(PacketCreator.partyStatusMessage(10));
                return false;
            }
            else if (getAriantColiseum() != null)
            {
                dropMessage(5, "You cannot request a party creation while participating the Ariant Battle Arena.");
                return false;
            }

            party = getChannelServer().CreateTeam(Id);
            setParty(party);
            silentPartyUpdate();

            updatePartySearchAvailability(false);
            partyOperationUpdate(party, null);

            sendPacket(PacketCreator.partyCreated(party, Id));

            return true;
        }

    }
}
