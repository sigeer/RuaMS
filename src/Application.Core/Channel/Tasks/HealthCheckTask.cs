namespace Application.Core.Channel.Tasks
{
    public class HealthCheckTask : AsyncAbstractRunnable
    {
        readonly WorldChannelServer _server;
        readonly NetworkMonitor networkMonitor;
        public HealthCheckTask(WorldChannelServer server) : base(
            $"{server.InstanceName}_{nameof(HealthCheckTask)}")
        {
            _server = server;
            networkMonitor = new NetworkMonitor();
        }

        public override async Task RunAsync()
        {
            var networkData = await networkMonitor.GetTrafficRateAsync();
            var model =  new ServerProto.MonitorData
            {
                ProcessMemoryUsed = SystemMonitor.GetCurrentProcessMemoryUsage(),

                SystemMemoryUsage = SystemMonitor.GetMemoryUsage(),
                SystemCPUUsage = await SystemMonitor.GetCpuUsage(),

                HeapSize = GC.GetTotalMemory(false),
                Gen0Count = GC.CollectionCount(0),
                Gen1Count = GC.CollectionCount(1),
                Gen2Count = GC.CollectionCount(2),

                InternalRecv = networkData.internalRecv,
                InternalSent = networkData.internalSent,
                ExternalRecv = networkData.externalRecv,
                ExternalSent = networkData.externalSent
            };
            _server.Transport.HealthCheck(model);
        }
    }
}
