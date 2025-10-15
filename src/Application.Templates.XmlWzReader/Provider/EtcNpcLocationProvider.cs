using Application.Templates.Etc;
using Application.Templates.Providers;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class EtcNpcLocationProvider : AbstractProvider<NpcLocationTemplate>
    {
        public EtcNpcLocationProvider(TemplateOptions options) : base(options)
        {

        }

        public override string ProviderName => ProviderNames.Etc;
        public override string[]? SingleImgFile => ["NpcLocation.img.xml"];

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? path)
        {
            using var fis = _fileProvider.ReadFile(path);
            using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
            var xDoc = XDocument.Load(reader).Root!;

            List<NpcLocationTemplate> list = new();
            foreach (var item in xDoc.Elements())
            {
                if (int.TryParse(item.GetName(), out var npcId))
                {
                    var template = new NpcLocationTemplate(npcId);
                    var mapList = new List<int>();
                    foreach (var npcData in item.Elements())
                    {
                        if (int.TryParse(npcData.GetName(), out var idx) && int.TryParse(npcData.GetStringValue(), out var mapId))
                            mapList.Add(mapId);
                    }
                    template.Maps = mapList.ToArray();

                    InsertItem(template);
                    list.Add(template);
                }
            }

            return list;
        }
    }
}
