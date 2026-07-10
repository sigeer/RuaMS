using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class CashCommodityProvider : AbstractAllProvider<CashCommodityTemplate>
    {
        public override ProviderType Type => ProviderType.EtcCashCommodity;

        public CashCommodityProvider(IWzPathResolver resolver)
            : base(resolver)
        {
        }

        protected override IEnumerable<CashCommodityTemplate> GetDataFromImg()
        {
            try
            {
                List<CashCommodityTemplate> all = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    using var fis = File.Open(_resolver.ResolveFullPath(file), FileMode.Open, FileAccess.Read, FileShare.Read);

                    using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                    if (reader.IsEmptyElement)
                        return [];

                    XmlReaderUtils.ReadChildNode(reader, itemNode =>
                    {
                        if (int.TryParse(itemNode.GetAttribute("name"), out var index))
                        {
                            var pEntry = new CashCommodityTemplate(index);
                            XmlReaderUtils.ReadChildNodeValue(itemNode, (name, value) =>
                            {
                                if (name == "SN")
                                {
                                    pEntry.CashItemSN = Convert.ToInt32(value);
                                    pEntry.TemplateId = Convert.ToInt32(value);
                                }
                                else if (name == "ItemId")
                                    pEntry.ItemID = Convert.ToInt32(value);
                                else if (name == "Count")
                                    pEntry.Count = Convert.ToInt32(value);
                                else if (name == "Price")
                                    pEntry.Price = Convert.ToInt32(value);
                                else if (name == "Period")
                                    pEntry.Period = Convert.ToInt32(value);
                                else if (name == "Priority")
                                    pEntry.Priority = Convert.ToInt32(value);
                                else if (name == "Gender")
                                    pEntry.Gender = Convert.ToInt32(value);
                                else if (name == "OnSale")
                                    pEntry.OnSale = Convert.ToInt32(value) > 0;
                                else if (name == "Class")
                                    pEntry.Classification = Convert.ToInt32(value);
                            });
                            all.Add(pEntry);
                            InsertItem(pEntry);
                        }
                    });
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
