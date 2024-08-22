using client;
using System.Collections.Concurrent;

namespace Application.Core.Loggers
{
    public class CategoryLogger
    {
        private readonly ILogger _logger;

        public CategoryLogger(string category)
        {
            _logger = Log.ForContext("Category", category);
        }

        public ILogger Logger => _logger;

        // 添加其他日志级别方法...
    }

    public class LogFactory
    {
        private static ConcurrentDictionary<string, CategoryLogger> dataSource = new ConcurrentDictionary<string, CategoryLogger>();
        public static ILogger GetLogger(string type)
        {
            return dataSource.GetOrAdd(type, e => new CategoryLogger(e)).Logger;
        }

    }
}
