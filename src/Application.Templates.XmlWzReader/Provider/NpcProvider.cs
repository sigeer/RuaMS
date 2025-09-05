using Application.Templates.Npc;
using Application.Templates.Providers;

namespace Application.Templates.XmlWzReader.Provider
{
    public sealed class NpcProvider : AbstractProvider<NpcTemplate>
    {
        public override ProviderType ProviderName => ProviderType.Npc;

        public NpcProvider(TemplateOptions options)
            : base(options) { }

        protected override string GetImgPathByTemplateId(int key)
        {
            return Path.Combine(GetPath(), key.ToString().PadLeft(7, '0') + ".img.xml");
        }

        protected override void GetDataFromImg(string path)
        {
            if (int.TryParse(Path.GetFileName(path).Substring(0, 7), out var npcId))
            {
                var model = new NpcTemplate(npcId);
                InsertItem(model);
            }
        }
        protected override void LoadAllInternal()
        {
            var files = new DirectoryInfo(GetPath()).GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (var item in files)
            {
                GetDataFromImg(item.FullName);
            }
        }

    }
}
