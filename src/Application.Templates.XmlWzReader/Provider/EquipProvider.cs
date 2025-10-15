using Application.Templates.Character;
using Application.Templates.Providers;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class EquipProvider : ItemProviderBase
    {
        public override string ProviderName => ProviderNames.Character;
        public EquipProvider(TemplateOptions options) : base(options)
        {
        }

        protected override string? GetImgPathByTemplateId(int itemId)
        {
            string fileName = itemId.ToString().PadLeft(8, '0') + ".img.xml";
            return _files.FirstOrDefault(x => x.EndsWith(fileName));
        }
        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? imgPath)
        {
            using var fis = _fileProvider.ReadFile(imgPath);
            using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
            var xDoc = XDocument.Load(reader).Root!;

            if (!int.TryParse(xDoc.GetName().AsSpan(0, 8), out var equipItemId))
                return [];

            var pEntry = new EquipTemplate(equipItemId);
            EquipTemplateGenerated.ApplyProperties(pEntry, xDoc);
            InsertItem(pEntry);
            return [pEntry];
        }
    }
}
