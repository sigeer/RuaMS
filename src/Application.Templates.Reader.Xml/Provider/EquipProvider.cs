using Application.Templates.Character;
using Application.Templates.Exceptions;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class EquipProvider : AbstractGroupProvider<AbstractItemTemplate>
    {
        public override ProviderType Type => ProviderType.Equip;

        public EquipProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<AbstractItemTemplate> GetDataFromImg(string? imgPath)
        {
            try
            {
                using var fis = File.Open(_resolver.ResolveFullPath(imgPath), FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 8), out var equipItemId))
                    throw new TemplateFormatException("Character.wz", imgPath);

                var pEntry = new EquipTemplate(equipItemId);
                EquipTemplateGenerated.ApplyProperties(pEntry, xDoc);
                InsertItem(pEntry);
                return [pEntry];
            }
            catch (Exception notFoundEx)
            {
                LibLog.Logger.LogError(notFoundEx.ToString());
                return [];
            }
        }
    }
}
