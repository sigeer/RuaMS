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


using server.quest;
using tools;

namespace client;


/**
 * @author Matze
 */
public class QuestStatus
{
    private short questID;
    private Status status;
    //private bool updated;   //maybe this can be of use for someone?
    private Dictionary<int, string> _progress = new();
    private List<int> medalProgress = new();
    private int npc;
    private long completionTime, expirationTime;
    private int forfeited = 0, completed = 0;
    private string? customData;

    public QuestStatus(Quest quest, Status status)
    {
        this.questID = quest.getId();
        this.setStatus(status);
        this.completionTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        this.expirationTime = 0;
        //this.updated = true;
        if (status == Status.STARTED)
        {
            registerMobs();
        }
    }

    public QuestStatus(Quest quest, Status status, int npc) : this(quest, status)
    {
        this.setNpc(npc);
    }

    public Quest getQuest()
    {
        return Quest.getInstance(questID);
    }

    public short getQuestID()
    {
        return questID;
    }

    public Status getStatus()
    {
        return status;
    }

    public void setStatus(Status status)
    {
        this.status = status;
    }

    /*
    public bool wasUpdated() {
        return updated;
    }
    
    private void setUpdated() {
        this.updated = true;
    }
    
    public void resetUpdated() {
        this.updated = false;
    }
    */

    public int getNpc()
    {
        return npc;
    }

    public void setNpc(int npc)
    {
        this.npc = npc;
    }

    private void registerMobs()
    {
        foreach (int i in Quest.getInstance(questID).getRelevantMobs())
        {
            _progress.AddOrUpdate(i, "000");
        }
        //this.setUpdated();
    }

    public bool addMedalMap(int mapid)
    {
        if (medalProgress.Contains(mapid))
        {
            return false;
        }
        medalProgress.Add(mapid);
        //this.setUpdated();
        return true;
    }

    public int getMedalProgress()
    {
        return medalProgress.Count;
    }

    public List<int> getMedalMaps()
    {
        return medalProgress;
    }

    public bool progress(int id)
    {
        string? currentStr = _progress.GetValueOrDefault(id);
        if (currentStr == null)
        {
            return false;
        }

        int current = int.Parse(currentStr);
        if (current >= this.getQuest().getMobAmountNeeded(id))
        {
            return false;
        }

        string str = StringUtil.getLeftPaddedStr((++current).ToString(), '0', 3);
        _progress.AddOrUpdate(id, str);
        //this.setUpdated();
        return true;
    }

    public void setProgress(int id, string pr)
    {
        _progress.AddOrUpdate(id, pr);
        //this.setUpdated();
    }

    public bool madeProgress()
    {
        return _progress.Count > 0;
    }

    public string getProgress(int id)
    {
        return _progress.GetValueOrDefault(id) ?? "";
    }

    public void resetProgress(int id)
    {
        setProgress(id, "000");
    }

    public void resetAllProgress()
    {
        foreach (var entry in _progress)
        {
            setProgress(entry.Key, "000");
        }
    }

    public Dictionary<int, string> getProgress()
    {
        return new(_progress);
    }

    public short getInfoNumber()
    {
        Quest q = this.getQuest();
        Status s = this.getStatus();

        return q.getInfoNumber(s);
    }

    public string getInfoEx(int index)
    {
        Quest q = this.getQuest();
        Status s = this.getStatus();

        return q.getInfoEx(s, index);
    }

    public List<string> getInfoEx()
    {
        Quest q = this.getQuest();
        Status s = this.getStatus();

        return q.getInfoEx(s);
    }

    public long getCompletionTime()
    {
        return completionTime;
    }

    public void setCompletionTime(long completionTime)
    {
        this.completionTime = completionTime;
    }

    public long getExpirationTime()
    {
        return expirationTime;
    }

    public void setExpirationTime(long expirationTime)
    {
        this.expirationTime = expirationTime;
    }

    public int getForfeited()
    {
        return forfeited;
    }

    public int getCompleted()
    {
        return completed;
    }

    public void setForfeited(int forfeited)
    {
        if (forfeited >= this.forfeited)
        {
            this.forfeited = forfeited;
        }
        else
        {
            throw new ArgumentException("Can't set forfeits to something lower than before.");
        }
    }

    public void setCompleted(int completed)
    {
        if (completed >= this.completed)
        {
            this.completed = completed;
        }
        else
        {
            throw new ArgumentException("Can't set completes to something lower than before.");
        }
    }

    public void setCustomData(string? customData)
    {
        this.customData = customData;
    }

    public string? getCustomData()
    {
        return customData;
    }

    public string getProgressData()
    {
        return string.Join("", _progress.Values);
    }

    public enum Status
    {
        UNDEFINED = -1,
        NOT_STARTED = 0,
        STARTED = 1,
        COMPLETED = 2
    }
}