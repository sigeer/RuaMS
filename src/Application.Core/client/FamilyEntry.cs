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


using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using tools;

namespace client;

/**
 * @author Ubaware
 */

public class FamilyEntry
{
    private static ILogger log = LogFactory.GetLogger(LogType.Family);

    private int characterID;
    private volatile Family family;
    private volatile IPlayer? character;

    private volatile FamilyEntry? senior;
    private FamilyEntry?[] juniors = new FamilyEntry[2];
    private int[] entitlements = new int[11];
    private volatile int reputation, totalReputation;
    private volatile int todaysRep, repsToSenior; //both are daily values
    private volatile int totalJuniors, totalSeniors;

    private volatile int generation;

    private volatile bool repChanged; //used to ignore saving unchanged rep values

    // cached values for offline players
    private string charName;
    private int level;
    private Job job;

    public FamilyEntry(Family family, int characterID, string charName, int level, Job job)
    {
        this.family = family;
        this.characterID = characterID;
        this.charName = charName;
        this.level = level;
        this.job = job;
    }

    public IPlayer? getChr()
    {
        return character;
    }

    public void setCharacter(IPlayer? newCharacter)
    {
        // bug£¿
        if (newCharacter == null)
        {
            cacheOffline(newCharacter);
        }
        else
        {
            newCharacter.setFamilyEntry(this);
        }
        this.character = newCharacter;
    }

    private void cacheOffline(IPlayer? chr)
    {
        if (chr != null)
        {
            charName = chr.getName();
            level = chr.getLevel();
            job = chr.getJob();
        }
    }

    object joinLock = new object();
    public void join(FamilyEntry? senior)
    {
        lock (joinLock)
        {
            if (senior == null || getSenior() != null)
            {
                return;
            }
            Family oldFamily = getFamily();
            Family newFamily = senior.getFamily();
            setSenior(senior, false);
            addSeniorCount(newFamily.getTotalGenerations(), newFamily); //count will be overwritten by doFullCount()
            newFamily.getLeader().doFullCount(); //easier than keeping track of numbers
            oldFamily.setMessage(null, true);
            newFamily.addEntryTree(this);
            Server.getInstance().getWorld(oldFamily.getWorld()).removeFamily(oldFamily.getID());

            //db
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                bool success = updateDBChangeFamily(dbContext, getChrId(), newFamily.getID(), senior.getChrId());
                foreach (var junior in juniors)
                { // better to duplicate this than the SQL code
                    if (junior != null)
                    {
                        success = junior.updateNewFamilyDB(dbContext); // recursively updates juniors in db
                        if (!success)
                        {
                            break;
                        }
                    }
                }
                if (!success)
                {
                    dbTrans.Rollback();
                    log.Error("Could not absorb {OldName}'s family into {NewName}'s family. (SQL ERROR)", oldFamily.getName(), newFamily.getName());
                }
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                log.Error(e, "Could not get connection to DB when joining families");
            }
        }
    }

    object forkLock = new object();
    public void fork()
    {
        lock (forkLock)
        {
            Family oldFamily = getFamily();
            var oldSenior = getSenior();
            family = new Family(-1, oldFamily.getWorld());
            Server.getInstance().getWorld(family.getWorld()).addFamily(family.getID(), family);
            setSenior(null, false);
            family.setLeader(this);
            addSeniorCount(-getTotalSeniors(), family);
            setTotalSeniors(0);
            if (oldSenior != null)
            {
                oldSenior.addJuniorCount(-getTotalJuniors());
                oldSenior.removeJunior(this);
                oldFamily.getLeader().doFullCount();
            }
            oldFamily.removeEntryBranch(this);
            family.addEntryTree(this);
            this.repsToSenior = 0;
            this.repChanged = true;
            family.setMessage("", true);
            doFullCount(); //to make sure all counts are correct
                           // update db
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();

                bool success = updateDBChangeFamily(dbContext, getChrId(), getFamily().getID(), 0);

                foreach (var junior in juniors)
                { // better to duplicate this than the SQL code
                    if (junior != null)
                    {
                        success = junior.updateNewFamilyDB(dbContext); // recursively updates juniors in db
                        if (!success)
                        {
                            break;
                        }
                    }
                }
                if (!success)
                {
                    dbTrans.Rollback();
                    log.Error("Could not fork family with new leader {LeaderName}. (Old senior: {OldName}, leader: {NewName})", getName(), oldSenior?.getName(), oldFamily.getLeader().getName());
                }
                dbTrans.Commit();

            }
            catch (Exception e)
            {
                log.Error(e, "Could not get connection to DB when forking families");
            }
        }
    }

    object updateLock = new object();
    private bool updateNewFamilyDB(DBContext dbContext)
    {
        lock (updateLock)
        {


            if (!updateFamilyEntryDB(dbContext, getChrId(), getFamily().getID()))
            {
                return false;
            }
            if (!updateCharacterFamilyDB(dbContext, getChrId(), getFamily().getID(), true))
            {
                return false;
            }

            foreach (var junior in juniors)
            {
                if (junior != null)
                {
                    if (!junior.updateNewFamilyDB(dbContext))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    private static bool updateFamilyEntryDB(DBContext dbContext, int cid, int familyid)
    {
        try
        {
            dbContext.FamilyCharacters.Where(x => x.Cid == cid).ExecuteUpdate(x => x.SetProperty(y => y.Familyid, familyid));
            return true;
        }
        catch (Exception e)
        {
            log.Error(e, "Could not update family id in 'family_character' for chrId {CharacterId}. (fork)", cid);
            return false;
        }
    }

    object addSeniorLock = new object();
    private void addSeniorCount(int seniorCount, Family? newFamily)
    {
        lock (addSeniorLock)
        {
            // traverses tree and subtracts seniors and updates family
            if (newFamily != null)
            {
                this.family = newFamily;
            }
            setTotalSeniors(getTotalSeniors() + seniorCount);
            this.generation += seniorCount;
            foreach (var junior in juniors)
            {
                if (junior != null)
                {
                    junior.addSeniorCount(seniorCount, newFamily);
                }
            }
        }
    }

    object addJuniorLock = new object();
    private void addJuniorCount(int juniorCount)
    {
        lock (addJuniorLock)
        {
            // climbs tree and adds junior count
            setTotalJuniors(getTotalJuniors() + juniorCount);
            var senior = getSenior();
            if (senior != null)
            {
                senior.addJuniorCount(juniorCount);
            }
        }
    }

    public Family getFamily()
    {
        return family;
    }

    public int getChrId()
    {
        return characterID;
    }

    public string getName()
    {
        var chr = character;
        if (chr != null)
        {
            return chr.getName();
        }
        else
        {
            return charName;
        }
    }

    public int getLevel()
    {
        var chr = character;
        if (chr != null)
        {
            return chr.getLevel();
        }
        else
        {
            return level;
        }
    }

    public Job getJob()
    {
        var chr = character;
        if (chr != null)
        {
            return chr.getJob();
        }
        else
        {
            return job;
        }
    }

    public int getReputation()
    {
        return reputation;
    }

    public int getTodaysRep()
    {
        return todaysRep;
    }

    public void setReputation(int reputation)
    {
        if (reputation != this.reputation)
        {
            this.repChanged = true;
        }
        this.reputation = reputation;
    }

    public void setTodaysRep(int today)
    {
        if (today != todaysRep)
        {
            this.repChanged = true;
        }
        this.todaysRep = today;
    }

    public int getRepsToSenior()
    {
        return repsToSenior;
    }

    public void setRepsToSenior(int reputation)
    {
        if (reputation != this.repsToSenior)
        {
            this.repChanged = true;
        }
        this.repsToSenior = reputation;
    }

    public void gainReputation(int gain, bool countTowardsTotal)
    {
        gainReputation(gain, countTowardsTotal, this);
    }

    private void gainReputation(int gain, bool countTowardsTotal, FamilyEntry from)
    {
        if (gain != 0)
        {
            repChanged = true;
        }
        this.reputation += gain;
        this.todaysRep += gain;
        if (gain > 0 && countTowardsTotal)
        {
            this.totalReputation += gain;
        }
        var chr = getChr();
        if (chr != null)
        {
            chr.sendPacket(PacketCreator.sendGainRep(gain, from != null ? from.getName() : ""));
        }
    }

    public void giveReputationToSenior(int gain, bool includeSuperSenior)
    {
        int actualGain = gain;
        var senior = getSenior();
        if (senior != null && senior.getLevel() < getLevel() && gain > 0)
        {
            actualGain /= 2; //don't halve negative values
        }
        if (senior != null)
        {
            senior.gainReputation(actualGain, true, this);
            if (actualGain > 0)
            {
                this.repsToSenior += actualGain;
                this.repChanged = true;
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

    public int getTotalReputation()
    {
        return totalReputation;
    }

    public void setTotalReputation(int totalReputation)
    {
        if (totalReputation != this.totalReputation)
        {
            this.repChanged = true;
        }
        this.totalReputation = totalReputation;
    }

    public FamilyEntry? getSenior()
    {
        return senior;
    }

    object setSeniorLock = new object();
    public bool setSenior(FamilyEntry? senior, bool save)
    {
        lock (setSeniorLock)
        {
            if (this.senior == senior)
            {
                return false;
            }
            var oldSenior = this.senior;
            this.senior = senior;
            if (senior != null)
            {
                if (senior.addJunior(this))
                {
                    if (save)
                    {
                        updateDBChangeFamily(getChrId(), senior.getFamily().getID(), senior.getChrId());
                    }
                    if (this.repsToSenior != 0)
                    {
                        this.repChanged = true;
                    }
                    this.repsToSenior = 0;
                    this.addSeniorCount(1, null);
                    this.setTotalSeniors(senior.getTotalSeniors() + 1);
                    return true;
                }
            }
            else
            {
                if (oldSenior != null)
                {
                    oldSenior.removeJunior(this);
                }
            }
            return false;
        }
    }

    private static bool updateDBChangeFamily(int cid, int familyid, int seniorid)
    {
        try
        {
            using var dbContext = new DBContext();
            return updateDBChangeFamily(dbContext, cid, familyid, seniorid);
        }
        catch (Exception e)
        {
            log.Error(e, "Could not get connection to DB while changing family");
            return false;
        }
    }

    private static bool updateDBChangeFamily(DBContext dbContext, int cid, int familyid, int seniorid)
    {
        try
        {
            dbContext.FamilyCharacters.Where(x => x.Cid == cid).ExecuteUpdate(x =>
               x.SetProperty(y => y.Familyid, familyid).SetProperty(y => y.Seniorid, seniorid).SetProperty(y => y.Reptosenior, 0));
        }
        catch (Exception e)
        {
            log.Error(e, "Could not update seniorId in 'family_character' for chrId {CharacterId}", cid);
            return false;
        }
        return updateCharacterFamilyDB(dbContext, cid, familyid, false);
    }

    private static bool updateCharacterFamilyDB(DBContext dbContext, int charid, int familyid, bool fork)
    {
        try
        {
            dbContext.Characters.Where(x => x.Id == charid).ExecuteUpdate(x => x.SetProperty(y => y.FamilyId, familyid));
        }
        catch (Exception e)
        {
            log.Error(e, "Could not update familyId in 'characters' for chrId {CharacterId} when changing family. {IsFork}", charid, fork ? "(fork)" : "");
            return false;
        }
        return true;
    }

    public List<FamilyEntry?> getJuniors()
    {
        return (Arrays.asList(juniors));
    }

    public FamilyEntry? getOtherJunior(FamilyEntry junior)
    {
        if (juniors[0] == junior)
        {
            return juniors[1];
        }
        else if (juniors[1] == junior)
        {
            return juniors[0];
        }
        return null;
    }

    public int getJuniorCount()
    { //close enough to be relatively consistent to multiple threads (and the result is not vital)
        int juniorCount = 0;
        if (juniors[0] != null)
        {
            juniorCount++;
        }
        if (juniors[1] != null)
        {
            juniorCount++;
        }
        return juniorCount;
    }

    public bool addJunior(FamilyEntry newJunior)
    {
        lock (addJuniorLock)
        {
            for (int i = 0; i < juniors.Length; i++)
            {
                if (juniors[i] == null)
                { // successfully add new junior to family
                    juniors[i] = newJunior;
                    addJuniorCount(1);
                    getFamily().addEntry(newJunior);
                    return true;
                }
            }
            return false;
        }
    }

    object checkJuniorLock = new object();
    public bool isJunior(FamilyEntry entry)
    {
        lock (checkJuniorLock)
        {


            //require locking since result accuracy is vital
            if (juniors[0] == entry)
            {
                return true;
            }
            else
            {
                return juniors[1] == entry;
            }
        }
    }

    public bool removeJunior(FamilyEntry junior)
    {
        lock (addJuniorLock)
        {


            for (int i = 0; i < juniors.Length; i++)
            {
                if (juniors[i] == junior)
                {
                    juniors[i] = null;
                    return true;
                }
            }
            return false;
        }
    }

    public int getTotalSeniors()
    {
        return totalSeniors;
    }

    public void setTotalSeniors(int totalSeniors)
    {
        this.totalSeniors = totalSeniors;
    }

    public int getTotalJuniors()
    {
        return totalJuniors;
    }

    public void setTotalJuniors(int totalJuniors)
    {
        this.totalJuniors = totalJuniors;
    }

    public void announceToSenior(Packet packet, bool includeSuperSenior)
    {
        var senior = getSenior();
        if (senior != null)
        {
            var seniorChr = senior.getChr();
            if (seniorChr != null)
            {
                seniorChr.sendPacket(packet);
            }
            senior = senior.getSenior();
            if (includeSuperSenior && senior != null)
            {
                seniorChr = senior.getChr();
                if (seniorChr != null)
                {
                    seniorChr.sendPacket(packet);
                }
            }
        }
    }

    public void updateSeniorFamilyInfo(bool includeSuperSenior)
    {
        var senior = getSenior();
        if (senior != null)
        {
            var seniorChr = senior.getChr();
            if (seniorChr != null)
            {
                seniorChr.sendPacket(PacketCreator.getFamilyInfo(senior));
            }
            senior = senior.getSenior();
            if (includeSuperSenior && senior != null)
            {
                seniorChr = senior.getChr();
                if (seniorChr != null)
                {
                    seniorChr.sendPacket(PacketCreator.getFamilyInfo(senior));
                }
            }
        }
    }

    object doFullLock = new object();
    /**
     * Traverses entire family tree to update senior/junior counts. Call on leader.
     */
    public void doFullCount()
    {
        lock (doFullLock)
        {
            var counts = this.traverseAndUpdateCounts(0);
            getFamily().setTotalGenerations(counts.Key + 1);
        }
    }

    private KeyValuePair<int, int> traverseAndUpdateCounts(int seniors)
    { // recursion probably limits family size, but it should handle a depth of a few thousand
        setTotalSeniors(seniors);
        this.generation = seniors;
        int juniorCount = 0;
        int highestGeneration = this.generation;
        foreach (var entry in juniors)
        {
            if (entry != null)
            {
                var counts = entry.traverseAndUpdateCounts(seniors + 1);
                juniorCount += counts.Value; //total juniors
                if (counts.Key > highestGeneration)
                {
                    highestGeneration = counts.Key;
                }
            }
        }
        setTotalJuniors(juniorCount);
        return new(highestGeneration, juniorCount); //creating new objects to return is a bit inefficient, but cleaner than packing into a long
    }

    public bool useEntitlement(FamilyEntitlement entitlement)
    {
        int id = entitlement.ordinal();
        if (entitlements[id] >= 1)
        {
            return false;
        }
        try
        {
            using var dbContext = new DBContext();
            dbContext.FamilyEntitlements.Add(new DB_FamilyEntitlement
            {
                Entitlementid = id,
                Charid = getChrId(),
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            });
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            log.Error(e, "Could not insert new row in 'family_entitlement' for chr {CharacterName}", getName());
        }
        entitlements[id]++;
        return true;
    }

    public bool refundEntitlement(FamilyEntitlement entitlement)
    {
        int id = entitlement.ordinal();
        try
        {
            using var dbContext = new DBContext();
            dbContext.FamilyEntitlements.Where(x => x.Entitlementid == id && x.Charid == getChrId()).ExecuteDelete();
        }
        catch (Exception e)
        {
            log.Error(e, "Could not refund family entitlement \"{EntitleName}\" for chr {CharacterName}", entitlement.getName(), getName());
        }
        entitlements[id] = 0;
        return true;
    }

    public bool isEntitlementUsed(FamilyEntitlement entitlement)
    {
        return entitlements[entitlement.ordinal()] >= 1;
    }

    public int getEntitlementUsageCount(FamilyEntitlement entitlement)
    {
        return entitlements[entitlement.ordinal()];
    }

    public void setEntitlementUsed(int id)
    {
        entitlements[id]++;
    }

    public void resetEntitlementUsages()
    {
        foreach (FamilyEntitlement entitlement in FamilyEntitlement.values<FamilyEntitlement>())
        {
            entitlements[entitlement.ordinal()] = 0;
        }
    }

    public bool saveReputation()
    {
        if (!repChanged)
        {
            return true;
        }
        try
        {
            using var dbContext = new DBContext();
            return saveReputation(dbContext);
        }
        catch (Exception e)
        {
            log.Error(e, "Could not get connection to DB while saving reputation");
            return false;
        }
    }

    public bool saveReputation(DBContext dbContext)
    {
        if (!repChanged)
        {
            return true;
        }
        try
        {
            dbContext.FamilyCharacters.Where(x => x.Cid == getChrId()).ExecuteUpdate(x => x
                .SetProperty(y => y.Reputation, getReputation())
                .SetProperty(y => y.Todaysrep, getTodaysRep())
                .SetProperty(y => y.Totalreputation, getTotalReputation())
                .SetProperty(y => y.Reptosenior, getRepsToSenior()));
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to autosave rep to 'family_character' for chrId {CharacterId}", getChrId());
            return false;
        }
        return true;
    }

    public void savedSuccessfully()
    {
        this.repChanged = false;
    }
}
