using Application.Templates.Providers;
using Application.Templates.UI;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class MobWithBossHpBarProvider : AbstractAllProvider<MobWithBossHpBarTemplate>
    {
        public MobWithBossHpBarProvider(ProviderOption options) : base(options, "UIWindow.img.xml")
        {
        }

        public override string ProviderName => ProviderNames.UI;

        protected override IEnumerable<AbstractTemplate> GetDataFromImg()
        {
            try
            {
                using var fis = _fileProvider.ReadFile(_file);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!.Elements()
                    .FirstOrDefault(x => x.GetName() == "MobGage")!
                    .Elements()
                    .FirstOrDefault(x => x.GetName() == "Mob")!;

                List<MobWithBossHpBarTemplate> list = new();
                foreach (var item in xDoc.Elements())
                {
                    if (int.TryParse(item.GetName(), out var mobId))
                    {
                        var template = new MobWithBossHpBarTemplate(mobId);

                        InsertItem(template);
                        list.Add(template);
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
