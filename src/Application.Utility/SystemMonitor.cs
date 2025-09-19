using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Application.Utility
{
    public class SystemMonitor
    {
        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        public static async Task<double> GetCpuUsage()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var start = Process.GetCurrentProcess().TotalProcessorTime;
                    var startTime = DateTime.UtcNow;
                    await Task.Delay(500);
                    var end = Process.GetCurrentProcess().TotalProcessorTime;
                    var endTime = DateTime.UtcNow;
                    double cpuUsed = (end - start).TotalMilliseconds /
                                     (Environment.ProcessorCount * (endTime - startTime).TotalMilliseconds);
                    return cpuUsed * 100;
                }
                else
                {
                    var cpu1 = ReadCpuStat();
                    await Task.Delay(500);
                    var cpu2 = ReadCpuStat();
                    ulong idleDiff = cpu2.idle - cpu1.idle;
                    ulong totalDiff = cpu2.total - cpu1.total;
                    return (1.0 - ((double)idleDiff / totalDiff)) * 100.0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private static (ulong idle, ulong total) ReadCpuStat()
        {
            try
            {
                var lines = File.ReadAllLines("/proc/stat");
                var parts = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                ulong user = ulong.Parse(parts[1]);
                ulong nice = ulong.Parse(parts[2]);
                ulong system = ulong.Parse(parts[3]);
                ulong idle = ulong.Parse(parts[4]);
                ulong iowait = ulong.Parse(parts[5]);
                ulong irq = ulong.Parse(parts[6]);
                ulong softirq = ulong.Parse(parts[7]);
                ulong steal = ulong.Parse(parts[8]);
                ulong total = user + nice + system + idle + iowait + irq + softirq + steal;
                return (idle + iowait, total);
            }
            catch
            {
                return (0, 0);
            }
        }

        /// <summary>
        /// 获取内存使用率
        /// </summary>
        public static double GetMemoryUsage()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                    foreach (var obj in searcher.Get())
                    {
                        double total = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                        double free = Convert.ToDouble(obj["FreePhysicalMemory"]);
                        return (1 - free / total) * 100;
                    }
                    return 0;
                }
                else
                {
                    var lines = File.ReadAllLines("/proc/meminfo");
                    double total = 0, available = 0;
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("MemTotal:")) total = ParseMem(line);
                        else if (line.StartsWith("MemAvailable:")) available = ParseMem(line);
                    }
                    return total > 0 ? (1.0 - available / total) * 100 : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private static double ParseMem(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return double.Parse(parts[1]); // kB
        }

        /// <summary>
        /// 获取硬盘使用率
        /// </summary>
        public static double GetDiskUsage(string path)
        {
            try
            {
                var root = Path.GetPathRoot(path) ?? "/";
                var drive = new DriveInfo(root);
                if (drive.IsReady)
                    return (1.0 - (double)drive.AvailableFreeSpace / drive.TotalSize) * 100.0;
            }
            catch
            {
                // 读取硬盘失败
            }
            return 0;
        }

        /// <summary>
        /// 获取当前进程的CPU使用率
        /// </summary>
        /// <returns></returns>
        public static async Task<double> GetCurrentProcessCpuUsage()
        {
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpu = process.TotalProcessorTime;

            await Task.Delay(500); // 等半秒

            var endTime = DateTime.UtcNow;
            var endCpu = process.TotalProcessorTime;

            double cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
            double totalMs = (endTime - startTime).TotalMilliseconds;

            double cpuPercent = cpuUsedMs / (Environment.ProcessorCount * totalMs) * 100.0;
            return cpuPercent;
        }

        /// <summary>
        /// 获取当前进程的内存使用（单位：字节）
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentProcessMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            return process.PrivateMemorySize64; // 单位：字节
        }

        /// <summary>
        /// 实时监控系统负载，每 interval 毫秒刷新一次
        /// </summary>
        public static async Task MonitorAsync(int interval = 1000)
        {
            while (true)
            {
                double cpu = await GetCpuUsage();
                double mem = GetMemoryUsage();
                double disk = GetDiskUsage("/");

                Console.WriteLine($"{DateTime.Now:HH:mm:ss} | CPU: {cpu:F2}% | 内存: {mem:F2}% | 硬盘: {disk:F2}%");

                await Task.Delay(interval);
            }
        }
    }
}
