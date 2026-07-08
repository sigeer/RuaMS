using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public class EtcNpcLocationProvider : AbstractAllProvider<NpcLocationTemplate>
    {
        public override ProviderType Type => ProviderType.EtcNpcLocation;

        public EtcNpcLocationProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<NpcLocationTemplate> GetDataFromImg()
        {
            try
            {
                List<NpcLocationTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    foreach (var item in root.Elements())
                    {
                        if (int.TryParse(item.GetName(), out var npcId))
                        {
                            var template = new NpcLocationTemplate(npcId);
                            var mapList = new List<int>();
                            foreach (var npcData in item.Elements())
                            {
                                if (int.TryParse(npcData.GetName(), out var idx) && int.TryParse(npcData.GetStringValue(), out var mapId))
                                    mapList.Add(mapId);
                            }
                            template.Maps = mapList.ToArray();

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
