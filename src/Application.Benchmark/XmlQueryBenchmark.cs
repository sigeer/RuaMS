using BenchmarkDotNet.Attributes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Application.Benchmark
{
    /// <summary>
    /// | Method      | Mean      | Error     | StdDev   | Median    | Gen0    | Allocated |
    /// |------------ |----------:|----------:|---------:|----------:|--------:|----------:|
    /// | LINQ_Query  |  4.699 us | 0.3776 us | 1.113 us |  4.218 us |  0.1602 |     256 B |
    /// | XPath_Query | 30.654 us | 2.2330 us | 6.584 us | 28.456 us | 10.6812 |   16752 B |
    /// </summary>

    [MemoryDiagnoser]
    public class XmlQueryBenchmark
    {
        private XDocument doc = new XDocument(
                new XElement("imgdir",
                    Enumerable.Range(0, 100).Select(i =>
                        new XElement("imgdir",
                            new XAttribute("name", i % 10 == 0 ? "event" : i.ToString()),
                            new XElement("int", new XAttribute("name", "type"), new XAttribute("value", i))
                        )
                    )
                )
            );

        [Benchmark]
        public void LINQ_Query()
        {
            var nodes = doc.Root!.Elements()
                .Where(e => (string?)e.Attribute("name") == "event")
                .ToList();
        }

        [Benchmark]
        public void XPath_Query()
        {
            var nodes = doc.Root!.XPathSelectElements("./imgdir[@name='event']").ToList();
        }
    }

}
