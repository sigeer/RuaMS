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


using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using System.Collections.Concurrent;
using tools;

namespace client;


/// <summary>
/// ѧԺ
/// </summary>
public class Family
{
    private ILogger log;
    private static AtomicInteger familyIDCounter = new AtomicInteger();

    private int id, world;
    private ConcurrentDictionary<int, FamilyEntry> members = new();
    private FamilyEntry leader = null!;
    private string name = null!;
    private string? preceptsMessage = "";
    private int totalGenerations;

    public Family(int id, int world)
    {
        log = LogFactory.GetLogger("Family");
        int newId = id;
        if (id == -1)
        {
            // get next available family id
            while (idInUse(newId = familyIDCounter.incrementAndGet()))
            {
            }
        }
        this.id = newId;
        this.world = world;
    }

    private static bool idInUse(int id)
    {
        foreach (World world in Server.getInstance().getWorlds())
        {
            if (world.getFamily(id) != null)
            {
                return true;
            }
        }
        return false;
    }

    public int getID()
    {
        return id;
    }

    public int getWorld()
    {
        return world;
    }

    public void setLeader(FamilyEntry leader)
    {
        this.leader = leader;
        setName(leader.getName());
    }

    public FamilyEntry getLeader()
    {
        return leader;
    }

    private void setName(string name)
    {
        this.name = name;
    }

    public int getTotalMembers()
    {
        return members.Count;
    }

    public int getTotalGenerations()
    {
        return totalGenerations;
    }

    public void setTotalGenerations(int generations)
    {
        this.totalGenerations = generations;
    }

    public string getName()
    {
        return this.name;
    }

    public void setMessage(string? message, bool save)
    {
        this.preceptsMessage = message;
        if (save)
        {
            try
            {
                using var dbContext = new DBContext();
                dbContext.FamilyCharacters.Where(x => x.Cid == getLeader().getChrId()).ExecuteUpdate(x => x.SetProperty(y => y.Precepts, message));
            }
            catch (Exception e)
            {
                log.Error(e, "Could not save new precepts for family {FamilyId}", getID());
            }
        }
    }

    public string? getMessage()
    {
        return preceptsMessage;
    }

    public void addEntry(FamilyEntry entry)
    {
        members.AddOrUpdate(entry.getChrId(), entry);
    }

    public void removeEntryBranch(FamilyEntry root)
    {
        members.Remove(root.getChrId());
        foreach (var junior in root.getJuniors())
        {
            if (junior != null)
            {
                removeEntryBranch(junior);
            }
        }
    }

    public void addEntryTree(FamilyEntry root)
    {
        members.AddOrUpdate(root.getChrId(), root);
        foreach (var junior in root.getJuniors())
        {
            if (junior != null)
            {
                addEntryTree(junior);
            }
        }
    }

    public FamilyEntry? getEntryByID(int cid)
    {
        return members.GetValueOrDefault(cid);
    }

    public void broadcast(Packet packet)
    {
        broadcast(packet, -1);
    }

    public void broadcast(Packet packet, int ignoreID)
    {
        foreach (FamilyEntry entry in members.Values)
        {
            var chr = entry.getChr();
            if (chr != null)
            {
                if (chr.getId() == ignoreID)
                {
                    continue;
                }
                chr.sendPacket(packet);
            }
        }
    }

    public void broadcastFamilyInfoUpdate()
    {
        foreach (FamilyEntry entry in members.Values)
        {
            var chr = entry.getChr();
            if (chr != null)
            {
                chr.sendPacket(PacketCreator.getFamilyInfo(entry));
            }
        }
    }

    public void resetDailyReps()
    {
        foreach (FamilyEntry entry in members.Values)
        {
            entry.setTodaysRep(0);
            entry.setRepsToSenior(0);
            entry.resetEntitlementUsages();
        }
    }

    public static void loadAllFamilies(DBContext dbContext)
    {
        List<KeyValuePair<KeyValuePair<int, int>, FamilyEntry>> unmatchedJuniors = new(200); // <<world, seniorid> familyEntry>
        try
        {
            var characterWithFaimilyInfo = (from a in dbContext.FamilyCharacters
                                            join b in dbContext.Characters on a.Cid equals b.Id
                                            let c = dbContext.FamilyEntitlements.Where(x => a.Cid == x.Charid).Select(x => x.Entitlementid).ToList()
                                            select new
                                            {
                                                a.Cid,
                                                a.Familyid,
                                                a.Seniorid,
                                                a.Reputation,
                                                a.Todaysrep,
                                                a.Totalreputation,
                                                a.Reptosenior,
                                                a.Precepts,
                                                b.World,
                                                b.Name,
                                                b.Level,
                                                b.Job,
                                                entitlementIdList = c
                                            }).ToList();

            foreach (var item in characterWithFaimilyInfo)
            { // can be optimized
                int cid = item.Cid;
                string name = item.Name;
                int level = item.World;
                int jobID = item.Job;
                int world = item.World;

                int familyid = item.Familyid;
                int seniorid = item.Seniorid;
                int reputation = item.Reputation;
                int todaysRep = item.Todaysrep;
                int totalRep = item.Totalreputation;
                int repsToSenior = item.Reptosenior;
                string precepts = item.Precepts;
                //Timestamp lastResetTime = rsEntries.getTimestamp("lastresettime"); //taken care of by FamilyDailyResetTask
                var wserv = Server.getInstance().getWorld(world);
                if (wserv == null)
                {
                    continue;
                }
                var family = wserv.getFamily(familyid);
                if (family == null)
                {
                    family = new Family(familyid, world);
                    Server.getInstance().getWorld(world).addFamily(familyid, family);
                }
                FamilyEntry familyEntry = new FamilyEntry(family, cid, name, level, JobUtils.getById(jobID));
                family.addEntry(familyEntry);
                if (seniorid <= 0)
                {
                    family.setLeader(familyEntry);
                    family.setMessage(precepts, false);
                }
                var senior = family.getEntryByID(seniorid);
                if (senior != null)
                {
                    familyEntry.setSenior(family.getEntryByID(seniorid), false);
                }
                else
                {
                    if (seniorid > 0)
                    {
                        unmatchedJuniors.Add(new(new(world, seniorid), familyEntry));
                    }
                }
                familyEntry.setReputation(reputation);
                familyEntry.setTodaysRep(todaysRep);
                familyEntry.setTotalReputation(totalRep);
                familyEntry.setRepsToSenior(repsToSenior);
                //load used entitlements
            }

            // link missing ones (out of order)
            foreach (var unmatchedJunior in unmatchedJuniors)
            {
                int world = unmatchedJunior.Key.Key;
                int seniorid = unmatchedJunior.Key.Value;
                FamilyEntry junior = unmatchedJunior.Value;
                var senior = Server.getInstance().getWorld(world).getFamily(junior.getFamily().getID())?.getEntryByID(seniorid);
                if (senior != null)
                {
                    junior.setSenior(senior, false);
                }
                else
                {
                    Log.Logger.Error("Missing senior for chr {CharacterName} in world {WorldId}", junior.getName(), world);
                }
            }

            foreach (World world in Server.getInstance().getWorlds())
            {
                foreach (Family family in world.getFamilies())
                {
                    family.getLeader().doFullCount();
                }
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Could not get family_character entries.");
        }

    }

    public void saveAllMembersRep()
    {
        //was used for autosave task, but character autosave should be enough
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            bool success = true;
            foreach (FamilyEntry entry in members.Values)
            {
                success = entry.saveReputation(dbContext);
                if (!success)
                {
                    break;
                }
            }
            if (!success)
            {
                dbTrans.Rollback();
                log.Error("Family rep autosave failed for family {FamilyId}", getID());
            }
            dbTrans.Commit();
            //reset repChanged after successful save
            foreach (FamilyEntry entry in members.Values)
            {
                entry.savedSuccessfully();
            }
        }
        catch (Exception e)
        {
            log.Error(e, "Could not get connection to DB while saving all members rep");
        }
    }
}
