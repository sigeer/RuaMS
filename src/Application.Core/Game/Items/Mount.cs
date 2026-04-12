using Application.Core.Channel;
using Application.Utility.Tickables;
using constants.game;
using System.Runtime.ConstrainedExecution;
using tools;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Core.Game.Items;

public class Mount : ILoopTickable, IDisposable
{
    public int itemid;
    public int skillid;
    public int Tiredness { get; private set; }
    public int Exp { get; private set; }
    public int Level { get; private set; }
    private Player owner;

    public TickableStatus Status { get; private set; }
    public long Next { get; private set; }
    public long Period => 60_000;

    public Mount(Player owner, int id)
    {
        this.owner = owner;
        itemid = id;

        skillid = owner.getJobType() * 10000000 + 1004;
        Tiredness = owner.Mounttiredness;
        Level = owner.MountLevel;
        Exp = owner.MountExp;

        owner.SetMount(this);
    }

    public int getItemId()
    {
        return itemid;
    }

    public int getSkillId()
    {
        return skillid;
    }

    /**
     * 1902000 - Hog
     * 1902001 - Silver Mane
     * 1902002 - Red Draco
     * 1902005 - Mimiana
     * 1902006 - Mimio
     * 1902007 - Shinjou
     * 1902008 - Frog
     * 1902009 - Ostrich
     * 1902010 - Frog
     * 1902011 - Turtle
     * 1902012 - Yeti
     *
     * @return the id
     */
    public int getId()
    {
        if (itemid < 1903000)
        {
            return itemid - 1901999;
        }
        return 5;
    }

    public int getTiredness()
    {
        return Tiredness;
    }

    public int getExp()
    {
        return Exp;
    }

    public int getLevel()
    {
        return Level;
    }

    public void setTiredness(int newtiredness)
    {
        Tiredness = Math.Max(newtiredness, 0);
    }

    public int incrementAndGetTiredness()
    {
        Tiredness++;
        return Tiredness;
    }

    public void AddExp(float delta)
    {
        if (delta > 0.0f)
        {
            Exp += (int)Math.Ceiling(delta * (2 * Level + 6));
            bool levelup = Exp >= ExpTable.getMountExpNeededForLevel(Level) && Level < 31;
            if (levelup)
            {
                Level++;

                owner.MapModel.broadcastMessage(PacketCreator.updateMount(owner.Id, this, true));
            }
        }
    }


    public void setItemId(int newitemid)
    {
        itemid = newitemid;
    }

    public void setSkillId(int newskillid)
    {
        skillid = newskillid;
    }

    public void setActive(bool set)
    {
        Status = set ? TickableStatus.Active : TickableStatus.InActive;
    }

    public void Dispose()
    {
        setActive(false);
    }

    int step = 0;
    public void OnTick(long now)
    {
        if (!this.IsAvailable())
        {
            return;
        }

        if (owner.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
        {
            return;
        }

        if (Status == TickableStatus.Active && Next >= now)
        {
            if (step % YamlConfig.config.server.MOUNT_EXHAUST_COUNT == 0)
            {
                int tiredness = incrementAndGetTiredness();

                this.owner.MapModel.broadcastMessage(PacketCreator.updateMount(this.getId(), this, false));
                if (tiredness > 99)
                {
                    setTiredness(99);
                    owner.dispelSkill(owner.getJobType() * 10000000 + 1004);
                    owner.dropMessage(6, "Your mount grew tired! Treat it some revitalizer before riding it again!");
                }
            }
            else
            {
                step++;
            }
            Next = now + Period;
        }        
    }
}
