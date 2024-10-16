/*
    This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
               Matthias Butz <matze@odinms.de>
               Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/



using Application.Core.Game.Relation;
using net.server;

namespace server.partyquest;
/**
 * @author kevintjuh93
 */
public abstract class PartyQuest
{
    protected static ILogger log = LogFactory.GetLogger("PartyQuest");

    int channel, world;
    ITeam party;
    List<IPlayer> participants = new();

    public PartyQuest(ITeam party)
    {
        this.party = party;
        var leader = party.getLeader();
        channel = leader.Channel!.Value;
        world = leader.getWorld();
        int mapid = leader.getMapId();
        foreach (var pchr in party.getMembers())
        {
            if (pchr.Channel == channel && pchr.getMapId() == mapid)
            {
                var chr = Server.getInstance().getWorld(world).getChannel(channel).getPlayerStorage().getCharacterById(pchr.getId());
                if (chr != null)
                {
                    this.participants.Add(chr);
                }
            }
        }
    }

    public ITeam getParty()
    {
        return party;
    }

    public List<IPlayer> getParticipants()
    {
        return participants;
    }

    public void removeParticipant(IPlayer chr)
    {
        lock (participants)
        {
            participants.Remove(chr);
            chr.setPartyQuest(null);
            //System.gc();
        }
    }

    public static int getExp(string PQ, int level)
    {
        switch (PQ)
        {
            case "HenesysPQ":
                return 1250 * level / 5;
            case "KerningPQFinal":
                return 500 * level / 5;
            case "KerningPQ4th":
                return 400 * level / 5;
            case "KerningPQ3rd":
                return 300 * level / 5;
            case "KerningPQ2nd":
                return 200 * level / 5;
            case "KerningPQ1st":
                return 100 * level / 5;
            case "LudiMazePQ":
                return 2000 * level / 5;
            case "LudiPQ1st":
                return 100 * level / 5;
            case "LudiPQ2nd":
                return 250 * level / 5;
            case "LudiPQ3rd":
                return 350 * level / 5;
            case "LudiPQ4th":
                return 350 * level / 5;
            case "LudiPQ5th":
                return 400 * level / 5;
            case "LudiPQ6th":
                return 450 * level / 5;
            case "LudiPQ7th":
                return 500 * level / 5;
            case "LudiPQ8th":
                return 650 * level / 5;
            case "LudiPQLast":
                return 800 * level / 5;
            default:
                log.Warning("Unhandled PartyQuest: {PartyQuest}", PQ);
                return 0;
        }
    }
}
