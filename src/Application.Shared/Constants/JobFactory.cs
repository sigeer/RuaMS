using Application.Utility;
using Application.Utility.Exceptions;

namespace Application.Shared.Constants
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

        public static int MaxJobId = _dataSource.Keys.Max();
    }
}
