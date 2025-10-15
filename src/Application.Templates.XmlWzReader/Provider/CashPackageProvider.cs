using Application.Templates.Etc;
using Application.Templates.Providers;
using System.Xml;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class CashPackageProvider : AbstractAllProvider<CashPackageTemplate>
    {
        public override string ProviderName => ProviderNames.Etc;
        public CashPackageProvider(TemplateOptions options)
            : base(options, "CashPackage.img.xml")
        {
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg()
        {
            List<CashPackageTemplate> all = [];
            using var fis = _fileProvider.ReadFile(_file);
            using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
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
    }
}
