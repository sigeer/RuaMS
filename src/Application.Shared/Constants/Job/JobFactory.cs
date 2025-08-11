using Application.Shared.Constants.Skill;
using Application.Utility;
using Application.Utility.Exceptions;

namespace Application.Shared.Constants.Job
{
    public class JobFactory
    {
        private static Dictionary<int, Job> _dataSource = EnumClassUtils.GetValues<Job>().ToDictionary(x => x.Id, x => x);

        public static Job GetById(int id)
        {
            if (_dataSource.TryGetValue(id, out var d))
                return d;

            throw new BusinessException("不存在的JobId: " + id);
        }

        public static List<Job> GetAllJob()
        {
            return _dataSource.Values.ToList();
        }

        public static JobType GetJobTypeById(int id)
        {
            return (JobType)(id / 1000);
        }

        public static int MaxJobId = _dataSource.Keys.Max();
    }

    public static class JobUtils
    {
        public static int getMax()
        {
            return 22;
        }

        public static int getJobMapChair(this Job job)
        {
            switch (job.Type)
            {
                case JobType.Adventurer:
                    return Beginner.MAP_CHAIR;
                case JobType.Cygnus:
                    return Noblesse.MAP_CHAIR;
                default:
                    return Legend.MAP_CHAIR;
            }
        }
    }
}
