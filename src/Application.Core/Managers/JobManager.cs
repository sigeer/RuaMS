namespace Application.Core.Managers
{
    public class JobManager
    {
        public static Job GetJobStyleInternal(int jobid, byte opt)
        {
            int jobtype = jobid / 100;

            if (jobtype == Job.WARRIOR.getId() / 100 || jobtype == Job.DAWNWARRIOR1.getId() / 100 || jobtype == Job.ARAN1.getId() / 100)
            {
                return (Job.WARRIOR);
            }
            else if (jobtype == Job.MAGICIAN.getId() / 100 || jobtype == Job.BLAZEWIZARD1.getId() / 100 || jobtype == Job.EVAN1.getId() / 100)
            {
                return (Job.MAGICIAN);
            }
            else if (jobtype == Job.BOWMAN.getId() / 100 || jobtype == Job.WINDARCHER1.getId() / 100)
            {
                if (jobid / 10 == Job.CROSSBOWMAN.getId() / 10)
                {
                    return (Job.CROSSBOWMAN);
                }
                else
                {
                    return (Job.BOWMAN);
                }
            }
            else if (jobtype == Job.THIEF.getId() / 100 || jobtype == Job.NIGHTWALKER1.getId() / 100)
            {
                return (Job.THIEF);
            }
            else if (jobtype == Job.PIRATE.getId() / 100 || jobtype == Job.THUNDERBREAKER1.getId() / 100)
            {
                if (opt == 0x80)
                {
                    return (Job.BRAWLER);
                }
                else
                {
                    return (Job.GUNSLINGER);
                }
            }

            return (Job.BEGINNER);
        }

        public static int GetJobLevelSp(int level, int jobId, int jobBranch)
        {
            if (GetJobStyleInternal(jobId, 0x40) == Job.MAGICIAN)
            {
                level += 2;  // starts earlier, level 8
            }

            return 3 * level + GameConstants.getChangeJobSpUpgrade(jobBranch);
        }

        public static int GetJobMaxSp(Job job)
        {
            int jobBranch = job.Rank;
            int jobRange = GameConstants.getJobUpgradeLevelRange(jobBranch);
            return GetJobLevelSp(jobRange, job.Id, jobBranch);
        }
    }
}
