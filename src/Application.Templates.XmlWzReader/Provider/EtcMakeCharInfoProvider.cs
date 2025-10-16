using Application.Templates.Etc;
using Application.Templates.Providers;
using System.Xml.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Application.Templates.XmlWzReader.Provider
{
    public class EtcMakeCharInfoProvider : AbstractAllProvider<MakerCharInfoTemplate>
    {
        public EtcMakeCharInfoProvider(TemplateOptions options) : base(options, "MakeCharInfo.img.xml")
        {
        }

        public override string ProviderName => ProviderNames.Etc;

        protected override IEnumerable<AbstractTemplate> GetDataFromImg()
        {
            try
            {
                using var fis = _fileProvider.ReadFile(_file);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                List<AbstractTemplate> list = new();
                MakerCharInfoTemplate template = new MakerCharInfoTemplate();
                MakerCharInfoTemplateGenerated.ApplyProperties(template, xDoc);
                InsertItem(template);
                list.Add(template);
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
