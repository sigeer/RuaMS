using Application.Templates.Etc;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public class EtcItemMakeProvider : AbstractAllProvider<ItemMakeTemplate>
    {
        public EtcItemMakeProvider(IWzPathResolver fileMapping, bool useCache = true) : base(fileMapping, useCache)
        {
        }

        public override ProviderType Type => ProviderType.EtcItemMake;

        protected override IEnumerable<ItemMakeTemplate> GetDataFromImg()
        {
            try
            {
                List<ItemMakeTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    foreach (var item in root.Elements())
                    {
                        foreach (var itemNode in item.Elements())
                        {
                            if (int.TryParse(itemNode.GetName(), out var itemId))
                            {
                                var template = new ItemMakeTemplate(itemId);
                                ItemMakeTemplateGenerated.ApplyProperties(template, itemNode);
                                InsertItem(template);
                                list.Add(template);
                            }
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
