using Application.Templates.Etc;
using Application.Templates.Reader.Resolvers;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Application.Templates.Reader.Img.Provider
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
                    var rootNode = new WZImage(fullPath);

                    foreach (var item in rootNode.Children)
                    {
                        foreach (var itemNode in item.Children)
                        {
                            if (int.TryParse(itemNode.Name, out var itemId))
                            {
                                var template = new ItemMakeTemplate(itemId);
                                ItemMakeTemplateGenerated_Duey.ApplyProperties(template, itemNode);
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
