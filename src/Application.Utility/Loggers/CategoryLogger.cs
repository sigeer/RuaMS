using Application.Utility;
using Serilog;
using System.Collections.Concurrent;

namespace Application.Utility.Loggers
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

        public static ILogger GetCharacterLog(int accountId, int characterId, string type = "Account")
        {
            return GetLogger($"{type}/{type}_{new RangeNumberGenerator(accountId, RangeSteps.Accounts)}/Account_{accountId}/Character_{characterId}");
        }

        public static ILogger CommandLogger => GetLogger("Command");
        public static ILogger GM => GetLogger("GM");

    }
}
