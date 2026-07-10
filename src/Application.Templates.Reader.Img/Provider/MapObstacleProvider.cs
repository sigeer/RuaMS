using Application.Templates.Map;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class MapObstacleProvider : AbstractAllProvider<MapObstacleTemplate>
    {
        public override ProviderType Type => ProviderType.MapObstacle;

        public MapObstacleProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
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
                    var root = new WZImage(fullPath);
                    var mobHunt = root.Children.FirstOrDefault(x => x.Name == "mobHunt");
                    if (mobHunt == null)
                        continue;
                    foreach (var entry in mobHunt.Children)
                    {
                        var dmg = entry.GetIntValue("mobdamage");
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
