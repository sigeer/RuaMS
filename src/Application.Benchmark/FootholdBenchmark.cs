using Application.Shared.MapObjects;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Application.Utility;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Application.Benchmark
{
    /// <summary>
    /// | Method       | Mean      | Error     | StdDev    | Median    | Gen0   | Allocated |
    /// |------------- |----------:|----------:|----------:|----------:|-------:|----------:|
    /// | FindBelowOld | 446.18 ns | 21.011 ns | 61.622 ns | 436.90 ns | 0.3824 |     600 B |
    /// | FindBelowNew |  14.84 ns |  0.609 ns |  1.775 ns |  14.08 ns |      - |         - |
    /// </summary>
    [MemoryDiagnoser]
    public class FootholdBenchmark
    {
        FootholdTreeOld oldTree;
        FootholdTree newTree;

        Point p;
        [GlobalSetup]
        public void Setup()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AppSettingKeys.Section_WZ}:BaseDir"] = Path.GetFullPath(Path.Combine(GetCurrentSourceFile(), "..", "Application.Resources", "wz"))
            });
            var configuration = configurationBuilder.Build();
            var providerFactory = new ProviderSource(configuration)
                .RegisterProvider<MapProvider>(o => new MapProvider(o));

            oldTree = FootholdTreeOld.FromTemplate(providerFactory.GetProvider<MapProvider>().GetItem(211040101)!);
            newTree = FootholdTree.FromTemplate(providerFactory.GetProvider<MapProvider>().GetItem(211040101)!);
            p = new Point(-179, -678);
        }

        public static string GetCurrentSourceFile([CallerFilePath] string path = "") => path;
        [Benchmark]
        public void FindBelowOld()
        {
            oldTree.findBelow(p);
        }

        [Benchmark]
        public void FindBelowNew()
        {
            newTree.FindBelowFoothold(p);
        }
    }
}
