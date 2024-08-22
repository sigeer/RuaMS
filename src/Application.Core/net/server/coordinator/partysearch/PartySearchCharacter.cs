/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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


using client;

namespace net.server.coordinator.partysearch;



/**
 * @author Ronan
 */
public class PartySearchCharacter
{

    private WeakReference<Character> player;
    private int level;
    private bool queued;

    public PartySearchCharacter(Character chr)
    {
        player = new(chr);
        level = chr.getLevel();
        queued = true;
    }

    public override string ToString()
    {
        return !player.TryGetTarget(out var chr) || chr == null ? "[empty]" : chr.ToString();
    }

    public Character? callPlayer(int leaderid, int callerMapid)
    {
        if (!player.TryGetTarget(out var chr) || chr == null || !PartySearchCoordinator.isInVicinity(callerMapid, chr.getMapId()))
        {
            return null;
        }

        if (chr.hasDisabledPartySearchInvite(leaderid))
        {
            return null;
        }

        queued = false;
        if (chr.isLoggedinWorld() && chr.getParty() == null)
        {
            return chr;
        }
        else
        {
            return null;
        }
    }

    public Character? getPlayer()
    {
        return player.TryGetTarget(out var chr) ? chr : null;
    }

    public int getLevel()
    {
        return level;
    }

    public bool isQueued()
    {
        return queued;
    }

}
