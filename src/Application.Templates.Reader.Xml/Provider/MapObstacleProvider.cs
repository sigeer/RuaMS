using Application.Templates.Map;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class MapObstacleProvider : AbstractAllProvider<MapObstacleTemplate>
    {
        public override ProviderType Type => ProviderType.MapObstacle;

        public MapObstacleProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<MapObstacleTemplate> GetDataFromImg()
        {
            try
            {
                List<MapObstacleTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;
                    var mobHunt = root.Elements().FirstOrDefault(e => e.GetName() == "mobHunt");
                    if (mobHunt == null)
                        continue;
                    foreach (var entry in mobHunt.Elements())
                    {
                        var dmgEl = entry.Elements().FirstOrDefault(e => e.GetName() == "mobdamage");
                        var dmg = dmgEl?.GetIntValue() ?? 0;
                        if (dmg > 0)
                        {
                            var template = new MapObstacleTemplate(list.Count) { MobDamage = dmg };
                            InsertItem(template);
                            list.Add(template);
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
