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


using Application.Module.Family.Common;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Module.Family.Channel.Models;

/**
 * @author Ubaware
 */

public class FamilyEntry
{
    readonly ILogger<FamilyEntry> _logger;

    public Family Family { get; }


    private volatile FamilyEntry? senior;
    /// <summary>
    ///  同时最多只有2个子级？
    /// </summary>
    // private FamilyEntry?[] juniors = new FamilyEntry[2];


    public int Id { get; }
    public int Level { get; }
    public int JobId { get; }
    public string Name { get; }
    public int Channel { get; set; }
    public int SeniorId { get; }
    public int Reputation { get; set; }
    public int TotalReputation { get; set; }
    public int ReputationToSenior { get; set; }
    public int TodayReputation { get; set; }
    public long LoginTime { get; set; }
    public List<FamilyEntitlementUseRecord> Records { get; set; }

    public FamilyEntry(Family family, int characterID, string charName, int level, int job)
    {
        Family = family;
        Id = characterID;
        Name = charName;
        Level = level;
        JobId = job;
    }


    public void gainReputation(int gain, bool countTowardsTotal)
    {
        gainReputation(gain, countTowardsTotal, this);
    }

    private void gainReputation(int gain, bool countTowardsTotal, FamilyEntry from)
    {
        Reputation += gain;
        TodayReputation += gain;
        if (gain > 0 && countTowardsTotal)
        {
            TotalReputation += gain;
        }
        var chr = Family.Server.FindPlayerById(Channel, Id);
        if (chr != null)
        {
            chr.sendPacket(PacketCreator.sendGainRep(gain, from?.Name ?? ""));
        }
    }

    public void giveReputationToSenior(int gain, bool includeSuperSenior)
    {
        int actualGain = gain;
        var senior = getSenior();
        if (senior != null && senior.Level < Level && gain > 0)
        {
            actualGain /= 2; //don't halve negative values
        }
        if (senior != null)
        {
            senior.gainReputation(actualGain, true, this);
            if (actualGain > 0)
            {
                ReputationToSenior += actualGain;
            }
            if (includeSuperSenior)
            {
                senior = senior.getSenior();
                if (senior != null)
                {
                    senior.gainReputation(actualGain, true, this);
                }
            }
        }
    }



    public FamilyEntry? getSenior()
    {
        return Family.Members.GetValueOrDefault(SeniorId);
    }


    public int getJuniorCount()
    {
        return Family.Members.Values.Count(x => x.SeniorId == Id);
    }

    public bool isJunior(FamilyEntry entry)
    {
        return entry.SeniorId == Id;
    }

    public void useEntitlement(FamilyEntitlement entitlement)
    {
        Records.Add(new FamilyEntitlementUseRecord(entitlement));
    }

    public bool refundEntitlement(FamilyEntitlement entitlement)
    {
        var recentIndex = Records.FindLastIndex(x => x.Id == entitlement.Value);
        if (recentIndex >= 0)
            Records.RemoveAt(recentIndex);
        return recentIndex >= 0;
    }

    public bool isEntitlementUsed(FamilyEntitlement entitlement)
    {
        return Records.Count(x => x.Id == entitlement.Value) >= entitlement.getUsageLimit();
    }

    public int getEntitlementUsageCount(FamilyEntitlement entitlement)
    {
        return Records.Count(x => x.Id == entitlement.Value);
    }

}
