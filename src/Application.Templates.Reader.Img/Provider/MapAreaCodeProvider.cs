using Application.Templates.Map;
using Application.Templates.Reader.Resolvers;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class MapAreaCodeProvider : AbstractAllProvider<MapAreaCodeTemplate>
    {
        public MapAreaCodeProvider(IWzPathResolver fileMapping, bool useCache = true) : base(fileMapping, useCache)
        {
        }

        public override ProviderType Type => ProviderType.MapAreaCode;

        protected override IEnumerable<MapAreaCodeTemplate> GetDataFromImg()
        {
            try
            {
                List<MapAreaCodeTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var root = new WZImage(fullPath);
                    foreach (var item in root.Children)
                    {
                        if (int.TryParse(item.Name, out var id))
                        {
                            var template = new MapAreaCodeTemplate(id)
                            {
                                Code = item.GetIntValue(),
                            };
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
