namespace Application.Core.Managers
{
    public class RandomManager
    {
        public static Job GetJobByLevel(int level)
        {
            if (level < 10)
                return Job.BEGINNER;

            if (level >= 10 && level < 30)
                return Randomizer.Select(JobFactory.GetAllJob().Where(x => x.Rank == 1));
            if (level >= 30 && level < 70)
                return Randomizer.Select(JobFactory.GetAllJob().Where(x => x.Rank == 2));
            if (level >= 70 && level < 120)
                return Randomizer.Select(JobFactory.GetAllJob().Where(x => x.Rank == 3));
            if (level >= 120 && level <= 200)
                return Randomizer.Select(JobFactory.GetAllJob().Where(x => x.Rank == 4));

            throw new BusinessException("等级超过最大值");
        }
    }
}
