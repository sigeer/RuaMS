using Application.Templates.XmlWzReader.Provider;
using BenchmarkDotNet.Attributes;
using System.Text;
using XmlWzReader.wz;

namespace Application.Benchmark
{
    /// <summary>
    /// | Method               | Mean      | Error     | StdDev    | Median    | Gen0     | Gen1     | Allocated |
    /// |--------------------- |----------:|----------:|----------:|----------:|---------:|---------:|----------:|
    /// | NewProvider_Load     |  6.325 ms | 0.1370 ms | 0.3952 ms |  6.185 ms | 421.8750 | 390.6250 |   2.41 MB |
    /// | XMLDomMapleData_Load | 80.561 ms | 1.5974 ms | 2.9609 ms | 79.753 ms | 800.0000 | 200.0000 |   5.04 MB |
    /// </summary>

    [MemoryDiagnoser]
    public class XmlWzBenchmark
    {

        int mapId = 100000000;

        [Benchmark]
        public void NewProvider_Load()
        {
            var provider = new MapProvider(new Application.Templates.ProviderOption());
            var fullData = provider.GetItem(mapId);
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
        public void XMLDomMapleData_Load()
        {
            // 有缓存，影响结果
            // var provider = DataProviderFactory.getDataProvider(XmlWzReader.wz.WZFiles.MAP);
            var provider = new XMLWZFileProvider(XmlWzReader.wz.WZFiles.MAP);
            var fullData = provider.getData(GetMapImg(mapId));
        }

    }

}
