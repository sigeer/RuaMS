using Application.Templates.XmlWzReader.Provider;
using System.Diagnostics;
using System.Text;
using XmlWzReader;

namespace ServiceTest.Infrastructure
{
    internal class XmlWzTests
    {
        int mapId = 100000000;
        private static string GetMapImg(int mapid)
        {
            string mapName = mapid.ToString().PadLeft(9, '0');
            StringBuilder builder = new StringBuilder("Map/Map");
            int area = mapid / 100000000;
            builder.Append(area);
            builder.Append("/");
            builder.Append(mapName);
            builder.Append(".img");
            mapName = builder.ToString();
            return mapName;
        }

        [Test]
        public void XMLDomMapleData_Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var provider = DataProviderFactory.getDataProvider(XmlWzReader.wz.WZFiles.MAP);
            var fullData = provider.getData(GetMapImg(mapId));
            sw.Stop();
            Console.WriteLine(sw);
            Assert.That(DataTool.GetBoolean("info/town", fullData));
        }

        [Test]
        public void NewProvider_Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var provider = new MapProvider(new Application.Templates.TemplateOptions());
            var data = provider.GetItem(mapId);
            sw.Stop();
            Console.WriteLine(sw);
            Assert.That(data?.Town ?? false);
        }
    }
}
