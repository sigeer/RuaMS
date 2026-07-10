using Application.Templates.Exceptions;
using Application.Templates.Npc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class NpcProvider : AbstractGroupProvider<NpcTemplate>
    {
        public override ProviderType Type => ProviderType.Npc;

        public NpcProvider(IWzPathResolver resolver)
            : base(resolver) { }

        protected override IEnumerable<NpcTemplate> GetDataFromImg(string? path)
        {
            try
            {
                using var fis = File.Open(_resolver.ResolveFullPath(path), FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                if (!int.TryParse(xDoc.GetName().AsSpan(0, 7), out var npcId))
                    throw new TemplateFormatException("Npc.wz", path);

                var model = new NpcTemplate(npcId);
                NpcTemplateGenerated.ApplyProperties(model, xDoc);
                if (model.LinkId > 0)
                    GetItem(model.LinkId)?.CloneLink(model);
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
