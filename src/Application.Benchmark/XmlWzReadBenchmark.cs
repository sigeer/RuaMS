using Application.Templates.XmlWzReader.Provider;
using BenchmarkDotNet.Attributes;
using System.Text;
using XmlWzReader;
using XmlWzReader.wz;

namespace Application.Benchmark
{
    /// <summary>
    /// | Method               | Mean      | Error      | StdDev    | Gen0     | Gen1     | Allocated |
    /// |--------------------- |----------:|-----------:|----------:|---------:|---------:|----------:|
    /// | NewProvider_Read     |  6.180 ms |   4.790 ms | 0.2626 ms | 421.8750 | 390.6250 |   2.41 MB |
    /// | XMLDomMapleData_Read | 88.220 ms | 177.224 ms | 9.7143 ms | 750.0000 | 250.0000 |   5.04 MB |
    /// </summary>

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 3)]
    public class XmlWzReadBenchmark
    {
        int mapId = 100000000;

        [Benchmark]
        public void NewProvider_Read()
        {
            var provider = new MapProvider(new Application.Templates.ProviderOption());
            var fullData = provider.GetItem(mapId);

            var isTown = fullData?.Town ?? false;
        }

        private string GetMapImg(int mapid)
        {
            string mapName = mapId.ToString().PadLeft(9, '0');
            StringBuilder builder = new StringBuilder("Map/Map");
            int area = mapid / 100000000;
            builder.Append(area);
            builder.Append("/");
            builder.Append(mapName);
            builder.Append(".img");
            mapName = builder.ToString();
            return mapName;
        }

        [Benchmark]
        public void XMLDomMapleData_Read()
        {
            // 有缓存，影响结果
            // var provider = DataProviderFactory.getDataProvider(XmlWzReader.wz.WZFiles.MAP);
            var provider = new XMLWZFileProvider(XmlWzReader.wz.WZFiles.MAP);
            var fullData = provider.getData(GetMapImg(mapId));

            var isTown = DataTool.GetBoolean("info/town", fullData);
        }
    }

}
