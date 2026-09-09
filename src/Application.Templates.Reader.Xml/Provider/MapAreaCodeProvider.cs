using Application.Templates.Map;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
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
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;
                    foreach (var item in root.Elements())
                    {
                        if (int.TryParse(item.GetName(), out var id))
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
