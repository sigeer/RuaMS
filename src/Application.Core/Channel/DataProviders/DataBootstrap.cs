using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Core.Channel.DataProviders
{
    public abstract class DataBootstrap
    {
        protected readonly ILogger<DataBootstrap> _logger;
        public string Source { get; set; } = "WZ";
        public string Name { get; set; } = "WZ";
        bool isLoading = false;
        bool loaded = false;

        protected DataBootstrap(ILogger<DataBootstrap> logger)
        {
            _logger = logger;
        }

        public void LoadData()
        {
            if (isLoading)
            {
                _logger.LogInformation("{Source} - {Name} 正在加载", Source, Name);
                return;
            }

            if (loaded)
            {
                _logger.LogInformation("{Source} - {Name} 已加载", Source, Name);
                return;
            }

            isLoading = true;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            _logger.LogInformation("{Source} - {Name} 正在加载...", Source, Name);
            try
            {
                LoadDataInternal();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "{Source} - {Name} 加载失败。", Source, Name);
                throw;
            }
            finally
            {
                isLoading = false;
            }
            _logger.LogInformation("{Source} - {Name} 加载完成, 耗时{Cost}", Source, Name, sw.Elapsed.TotalSeconds);
            loaded = true;

        }

        protected abstract void LoadDataInternal();

        public virtual void Reload()
        {
            loaded = false;
            LoadData();
        }
    }
}
