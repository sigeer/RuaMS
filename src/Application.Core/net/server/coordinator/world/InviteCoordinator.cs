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


using System.Collections.Concurrent;

namespace net.server.coordinator.world;


/**
 * @author Ronan
 */
public class InviteCoordinator
{

    // note: referenceFrom is a specific value that represents the "common association" created between the sender/recver parties
    public static bool createInvite(InviteType type, IPlayer from, object referenceFrom, int targetCid, params object[] paramsValue)
    {
        return type.addRequest(from, referenceFrom, targetCid, paramsValue);
    }

    public static bool hasInvite(InviteType type, int targetCid)
    {
        return type.hasRequest(targetCid);
    }

    public static InviteResult answerInvite(InviteType type, int targetCid, object referenceFrom, bool answer)
    {
        var table = type.getRequestsTable();

        IPlayer? from = null;
        InviteResultType result = InviteResultType.NOT_FOUND;
        KeyValuePair<IPlayer, object[]>? inviteInfo = null;

        var reference = table.GetValueOrDefault(targetCid);
        if (referenceFrom == reference)
        {
            inviteInfo = type.removeRequest(targetCid);
            from = inviteInfo == null ? null : inviteInfo.Value.Key;
            if (from != null && !from.isLoggedinWorld())
            {
                from = null;
            }

            result = answer ? InviteResultType.ACCEPTED : InviteResultType.DENIED;
        }

        return new InviteResult(result, from, inviteInfo != null ? inviteInfo.Value.Value : new object[0]);
    }

    public static void removeInvite(InviteType type, int targetCid)
    {
        type.removeRequest(targetCid);
    }

    public static void removePlayerIncomingInvites(int cid)
    {
        foreach (InviteType it in EnumClassUtils.GetValues<InviteType>())
        {
            it.removeRequest(cid);
        }
    }

    public static void runTimeoutSchedule()
    {
        foreach (InviteType it in EnumClassUtils.GetValues<InviteType>())
        {
            var timeoutTable = it.getRequestsTimeoutTable();

            if (timeoutTable.Count > 0)
            {
                HashSet<KeyValuePair<int, int>> entrySet = new(timeoutTable);
                foreach (var e in entrySet)
                {
                    int eVal = e.Value;

                    if (eVal > 5)
                    { // 3min to expire
                        it.removeRequest(e.Key);
                    }
                    else
                    {
                        timeoutTable.AddOrUpdate(e.Key, eVal + 1);
                    }
                }
            }
        }
    }


}

public enum InviteResultType
{
    ACCEPTED,
    DENIED,
    NOT_FOUND
}

public record InviteResult(InviteResultType result, IPlayer from, object[] paramsValue);


public class InviteType : EnumClass
{
    //BUDDY, (not needed)
    public static InviteType FAMILY = new InviteType(0);
    public static InviteType FAMILY_SUMMON = new InviteType(1);
    public static InviteType MESSENGER = new InviteType(2);
    public static InviteType TRADE = new InviteType(3);
    public static InviteType PARTY = new InviteType(4);
    public static InviteType GUILD = new InviteType(5);
    public static InviteType ALLIANCE = new InviteType(6);

    byte value;
    ConcurrentDictionary<int, object> invites;
    ConcurrentDictionary<int, IPlayer> inviteFrom;
    ConcurrentDictionary<int, int> inviteTimeouts;
    ConcurrentDictionary<int, object[]> inviteParams;
    public InviteType(byte value)
    {
        this.value = value;

        invites = new();
        inviteTimeouts = new();
        inviteFrom = new();
        inviteParams = new();
    }

    public ConcurrentDictionary<int, object> getRequestsTable()
    {
        return invites;
    }

    public ConcurrentDictionary<int, int> getRequestsTimeoutTable()
    {
        return inviteTimeouts;
    }

    public KeyValuePair<IPlayer, object[]>? removeRequest(int target)
    {
        if (invites.TryRemove(target, out var _) && inviteFrom.TryRemove(target, out var from) && inviteParams.TryRemove(target, out var d))
        {
            inviteTimeouts.Remove(target);
            return new(from, d);
        }
        return null;
    }

    public bool addRequest(IPlayer from, object referenceFrom, int targetCid, object[] paramsValue)
    {
        if (!invites.TryAdd(targetCid, referenceFrom))
        {
            // there was already an entry
            return false;
        }

        inviteFrom.AddOrUpdate(targetCid, from);
        inviteTimeouts.AddOrUpdate(targetCid, 0);
        inviteParams.AddOrUpdate(targetCid, paramsValue);
        return true;
    }

    public bool hasRequest(int targetCid)
    {
        return invites.ContainsKey(targetCid);
    }
}
