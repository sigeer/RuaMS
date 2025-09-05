using Application.Templates.Etc;
using Application.Templates.Providers;
using System.Xml;

namespace Application.Templates.XmlWzReader.Provider
{
    public class CashCommodityProvider : AbstractProvider<CashCommodityTemplate>
    {
        public override ProviderType ProviderName => ProviderType.CashCommodity;
        string _imgPath;

        public CashCommodityProvider(TemplateOptions options)
            : base(options)
        {
            _imgPath = Path.Combine(GetPath(), "Commodity.img.xml");
        }

        protected override void GetDataFromImg(string path)
        {
            LoadAll();
        }

        protected override void LoadAllInternal()
        {
            using var fis = new FileStream(_imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = XmlReader.Create(fis, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
            if (reader.IsEmptyElement)
                return;

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
                    InsertItem(pEntry);
                }
            });
        }
    }
}
