using Application.Templates.Character;
using Application.Templates.Exceptions;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
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
                var fullPath = _resolver.ResolveFullPath(imgPath);
                var rootNode = new WZImage(fullPath);

                if (!int.TryParse(rootNode.Name.AsSpan(0, 8), out var equipItemId))
                    throw new TemplateFormatException("Character.wz", imgPath);

                var pEntry = new EquipTemplate(equipItemId);
                EquipTemplateGenerated_Duey.ApplyProperties(pEntry, rootNode);
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
