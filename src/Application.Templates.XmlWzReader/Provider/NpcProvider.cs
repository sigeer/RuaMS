using Application.Templates.Npc;
using Application.Templates.Providers;

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
            if (int.TryParse(Path.GetFileName(path).AsSpan(0, 7), out var npcId))
            {
                var model = new NpcTemplate(npcId);
                InsertItem(model);
                return [model];
            }
            return [];
        }
    }
}
