using Application.Templates.Etc;
using Application.Templates.Providers;
using System.Xml;

namespace Application.Templates.XmlWzReader.Provider
{
    public class CashPackageProvider : AbstractProvider<CashPackageTemplate>
    {
        public override ProviderType ProviderName => ProviderType.CashPackage;
        string _imgPath;

        public CashPackageProvider(TemplateOptions options)
            : base(options)
        {
            _imgPath = Path.Combine(GetPath(), "CashPackage.img.xml");
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
                if (int.TryParse(itemNode.GetAttribute("name"), out var sourceId))
                {
                    var pEntry = new CashPackageTemplate(sourceId);
                    XmlReaderUtils.ReadChildNodeValue(itemNode, (name, value) =>
                    {
                        if (name == "SN")
                        {
                            List<long> list = [];
                            XmlReaderUtils.ReadChildNodeValue(itemNode, (name, value) =>
                            {
                                if (name == "SN")
                                {
                                    list.Add(Convert.ToInt32(value));
                                }
                            });
                            pEntry.SNList = list.ToArray();
                        }
                    });
                    InsertItem(pEntry);
                }
            });
        }
    }
}
