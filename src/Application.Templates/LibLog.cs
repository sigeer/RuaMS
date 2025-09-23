using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Templates
{
    public static class LibLog
    {
        public static ILogger Logger { get; set; } = NullLogger.Instance;
    }
}
