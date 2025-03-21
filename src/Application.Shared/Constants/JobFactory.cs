using Application.Utility;
using Application.Utility.Exceptions;

namespace Application.Shared.Constants
{
    public class JobFactory
    {
        static Dictionary<int, Job> _dataSource = new Dictionary<int, Job>();

        static JobFactory()
        {
            _dataSource = EnumClassUtils.GetValues<Job>().ToDictionary(x => x.getId(), x => x);
        }

        public static Job GetById(int id)
        {
            if (_dataSource.TryGetValue(id, out var d))
                return d;

            throw new BusinessException("不存在的JobId: " + id);
        }
    }
}
