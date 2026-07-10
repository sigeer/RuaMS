using Application.Templates.Exceptions;
using Application.Templates.Npc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class NpcProvider : AbstractGroupProvider<NpcTemplate>
    {
        public override ProviderType Type => ProviderType.Npc;

        public NpcProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<NpcTemplate> GetDataFromImg(string? path)
        {
            try
            {
                var fullPath = _resolver.ResolveFullPath(path);
                var rootNode = new WZImage(fullPath);

                if (!int.TryParse(rootNode.Name.AsSpan(0, 7), out var npcId))
                    throw new TemplateFormatException("Npc.wz", path);

                var model = new NpcTemplate(npcId);
                NpcTemplateGenerated_Duey.ApplyProperties(model, rootNode);
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
