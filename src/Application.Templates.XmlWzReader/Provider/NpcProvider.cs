using Application.Templates.Exceptions;
using Application.Templates.Npc;
using Application.Templates.Providers;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class NpcProvider : AbstractGroupProvider<NpcTemplate>
    {
        public override string ProviderName => ProviderNames.Npc;

        public NpcProvider(ProviderOption options)
            : base(options) { }

        protected override string? GetImgPathByTemplateId(int key)
        {
            return Path.Combine(ProviderName, key.ToString().PadLeft(7, '0') + ".img.xml");
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? path)
        {
            try
            {
                using var fis = _fileProvider.ReadFile(path);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 7), out var npcId))
                    throw new TemplateFormatException(ProviderName, path);

                var model = new NpcTemplate(npcId);
                NpcTemplateGenerated.ApplyProperties(model, xDoc);
                InsertItem(model);
                return [model];
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }

        }
    }
}
