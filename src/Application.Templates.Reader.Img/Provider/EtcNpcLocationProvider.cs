using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class EtcNpcLocationProvider : AbstractAllProvider<NpcLocationTemplate>
    {
        public EtcNpcLocationProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        public override ProviderType Type => ProviderType.EtcNpcLocation;

        protected override IEnumerable<NpcLocationTemplate> GetDataFromImg()
        {
            try
            {
                List<NpcLocationTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var rootNode = new WZImage(fullPath);

                    foreach (var item in rootNode.Children)
                    {
                        if (int.TryParse(item.Name, out var npcId))
                        {
                            var template = new NpcLocationTemplate(npcId);
                            var mapList = new List<int>();
                            foreach (var npcData in item.Children)
                            {
                                mapList.Add(npcData.GetIntValue());
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
