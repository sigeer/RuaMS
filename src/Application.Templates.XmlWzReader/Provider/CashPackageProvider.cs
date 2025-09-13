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

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            List<CashPackageTemplate> all = [];
            using var fis = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = XmlReader.Create(fis, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
            if (reader.IsEmptyElement)
                return all;

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
                    all.Add(pEntry);
                    InsertItem(pEntry);
                }
            });
            return all;
        }


        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            return GetDataFromImg(_imgPath);
        }
    }
}
