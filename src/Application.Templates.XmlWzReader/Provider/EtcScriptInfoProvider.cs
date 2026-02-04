using Application.Templates.Etc;
using Application.Templates.Providers;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class EtcScriptInfoProvider : AbstractAllProvider<ScriptInfoTemplate>
    {
        public EtcScriptInfoProvider(ProviderOption options) : base(options, "ScriptInfo.img.xml")
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

                List<ScriptInfoTemplate> list = new();
                foreach (var item in xDoc.Elements())
                {
                    var template = new ScriptInfoTemplate();
                    template.Name = item.GetName() ?? "";
                    template.Value = item.GetStringValue() ?? "";

                    InsertItem(template);
                    list.Add(template);
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
