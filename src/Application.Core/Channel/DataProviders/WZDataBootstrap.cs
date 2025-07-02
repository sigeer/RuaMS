using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Tls;
using System.Diagnostics;

namespace Application.Core.Channel.DataProviders
{
    public abstract class WZDataBootstrap
    {
        protected readonly ILogger<WZDataBootstrap> _logger;
        public string Name { get; set; } = "WZ";
        bool isLoading = false;
        bool loaded = false;

        protected WZDataBootstrap(ILogger<WZDataBootstrap> logger)
        {
            _logger = logger;
        }

        public void LoadData()
        {
            if (isLoading)
            {
                _logger.LogInformation("WZ - {WZType} 正在加载", Name);
                return;
            }

            if (loaded)
            {
                _logger.LogInformation("WZ - {WZType} 已加载", Name);
                return;
            }

            isLoading = true;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            _logger.LogInformation("WZ - {WZType} 正在加载...",  Name);
            LoadDataInternal();
            _logger.LogInformation("WZ - {WZType} 加载完成, 耗时{Cost}", Name, sw.Elapsed.TotalSeconds);

            isLoading = false;
            loaded = true;
        }

        protected abstract void LoadDataInternal();
    }
}
