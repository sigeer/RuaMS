using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Core.Scripting.Events;
using Application.Scripting;
using Application.Shared.Events;
using scripting.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script.Events
{
    internal class PQ_KerningEventManager : PartyQuestEventManager
    {
        public WorldChannel ChannelServer { get; }
        public PQ_KerningEventManager(WorldChannel cserv) 
        {
            ChannelServer = cserv;
        }

        public int MinCount { get; }
        public int MaxCount { get; }

        public int MinLevel { get; }
        public int MaxLevel { get; }

        public int EntryMap { get; }
        public int EntryPortal { get; }
        public int ExitMap { get; }
        public int ExitPortal { get; }
        public int RecruitMap { get; }
        public int ClearMap { get; }

        public int MinMap { get; }
        public int MaxMap { get; }

        public virtual void Init()
        {

        }

        public virtual AbstractEventInstanceManager SetUp(int level, int lobbyId)
        {
            var eim = newInstance("Kerning" + lobbyId);
            eim.setProperty("level", level);

            respawnStages(eim);
            eim.startEventTimer(eventTime * 60000);
            setEventRewards(eim);
            setEventExclusives(eim);
            return eim;
        }

        public virtual List<Player> GetEligibleParty(Player leader)
        {
            var members = leader.getPartyMembersOnSameMap();

            if (members.Count >= MinCount 
                && members.Count <= MaxCount 
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }

        public virtual void OnMemberDied(AbstractEventInstanceManager eim, Player chr)
        {
            
        }
    }
}
