using Application.Templates.Character;
using Application.Templates.Providers;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class EquipProvider : ItemProviderBase
    {
        public override ProviderType ProviderName => ProviderType.Equip;
        string[] _itemFiles;
        public EquipProvider(TemplateOptions options) : base(options)
        {
            _itemFiles = Directory.GetFiles(GetPath(), "*", SearchOption.AllDirectories);
        }

        protected override string GetImgPathByTemplateId(int itemId)
        {
            string fileName = itemId.ToString().PadLeft(8, '0') + ".img.xml";
            return _itemFiles.FirstOrDefault(x => x.EndsWith(fileName)) ?? string.Empty;
        }
        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string imgPath)
        {
            if (!File.Exists(imgPath))
                return [];

            using var fis = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
            var xDoc = XDocument.Load(reader).Root!;

            if (!int.TryParse(xDoc.GetName().AsSpan(0, 8), out var equipItemId))
                return [];

            var pEntry = new EquipTemplate(equipItemId);
            EquipTemplateGenerated.ApplyProperties(pEntry, xDoc);
            InsertItem(pEntry);
            return [pEntry];
        }


        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = new List<AbstractTemplate>(_itemFiles.Length);
            foreach (var item in _itemFiles)
            {
                all.AddRange(GetDataFromImg(item));
            }
            return all;
        }
    }
}
