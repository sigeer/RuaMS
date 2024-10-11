/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */



using Application.Core.Game.Skills;

namespace server.events;

/**
 * @author kevintjuh93
 */
public class RescueGaga : Events
{

    private int completed;

    public RescueGaga(int completed) : base()
    {
        this.completed = completed;
    }

    public int getCompleted()
    {
        return completed;
    }

    public void complete()
    {
        completed++;
    }

    public override int getInfo()
    {
        return getCompleted();
    }

    public void giveSkill(IPlayer chr)
    {
        int skillid = 0;
        switch (chr.getJobType())
        {
            case 0:
                skillid = 1013;
                break;
            case 1:
            case 2:
                skillid = 10001014;
                break;
        }

        long expiration = DateTimeOffset.Now.AddDays(20).ToUnixTimeMilliseconds(); //20 days
        if (completed < 20)
        {
            chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid), 1, 1, expiration);
            chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid + 1), 1, 1, expiration);
            chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid + 2), 1, 1, expiration);
        }
        else
        {
            chr.changeSkillLevel(SkillFactory.GetSkillTrust(skillid), 2, 2, chr.getSkillExpiration(skillid));
        }
    }

}
