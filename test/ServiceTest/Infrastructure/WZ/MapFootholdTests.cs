using Application.Core.Game.Maps;
using Application.Shared.MapObjects;
using Application.Templates.Map;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using System.Drawing;
using System.Globalization;

namespace ServiceTest.Infrastructure.WZ
{
    internal class MapFootholdTests : WzTestBase
    {
        protected override void OnProviderRegistering()
        {
            _providerSource.TryRegisterProvider<MapProvider>(o => new MapProvider(o));
            _providerSource.TryRegisterProvider<StringProvider>(o => new StringProvider(o, CultureInfo.GetCultureInfo("en-US")));
        }
        private Point[] GetRandomPoint(Rectangle rectangle, int count = 100)
        {
            Point[] points = new Point[count];
            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                int x = rand.Next(rectangle.X, rectangle.X + rectangle.Width + 1);
                int y = rand.Next(rectangle.Y, rectangle.Y + rectangle.Height + 1);
                points[i] = new Point(x, y);
            }

            return points;
        }

        private MapTemplate[] GetRandomMapTemplates(int count = 100)
        {
            var allMapIds = _providerSource.GetProvider<StringProvider>().GetSubProvider(Application.Templates.String.StringCategory.Map).LoadAll()
                .Select(x => x.TemplateId)
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToArray();
            return allMapIds.Select(x => _providerSource.GetProvider<MapProvider>().GetItem(x)).Where(x => x != null).ToArray();
        }

        // [Test]
        public void FindBelowEqualTest()
        {
            var templates = GetRandomMapTemplates();

            foreach (var template in templates)
            {
                var oldTree = FootholdTreeOld.FromTemplate(template);
                var newTree = FootholdTree.FromTemplate(template);
                var points = GetRandomPoint(template.GetMapRectangle());
                foreach (var point in points)
                {
                    var result = newTree.FindBelowFoothold(point);
                    var oldFoothold = oldTree.findBelow(point);
                    Assert.That(result, Is.EqualTo(oldFoothold), $"Map = {template.TemplateId}, Current: {point}, Old: {oldFoothold}, New: {result}");
                }
            }

        }

        // [Test]
        public void FindBelowSpecialTest()
        {
            var testCases = new Dictionary<int, Point[]>()
            {
                {211040101, [new Point(-179, -678)] },
                {926020001, [new Point(-52, -77)] },
                {220000400, [new Point(-2650, -138), new Point(-2330, -18)] }
            };
            var mapProvider = _providerSource.GetProvider<MapProvider>();
            foreach (var item in testCases)
            {
                var template = mapProvider.GetItem(item.Key)!;
                var oldTree = FootholdTreeOld.FromTemplate(template);
                var newTree = FootholdTree.FromTemplate(template);
                foreach (var point in item.Value)
                {
                    var result = newTree.FindBelowFoothold(point);
                    var oldFoothold = oldTree.findBelow(point);
                    Assert.That(result, Is.EqualTo(oldFoothold), $"Map = {template.TemplateId}, Current: {point}, Old: {oldFoothold}, New: {result}");
                }
            }

        }
    }
}
