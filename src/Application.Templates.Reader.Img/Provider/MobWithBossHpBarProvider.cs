using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.UI;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class MobWithBossHpBarProvider : AbstractAllProvider<MobWithBossHpBarTemplate>
    {
        public MobWithBossHpBarProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        public override ProviderType Type => ProviderType.UIMobWithBossHpBar;

        protected override IEnumerable<MobWithBossHpBarTemplate> GetDataFromImg()
        {
            try
            {
                List<MobWithBossHpBarTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var rootNode = new WZImage(fullPath);

                    var mobGageNode = rootNode.Children.FirstOrDefault(x => x.Name == "MobGage");
                    var mobNode = mobGageNode?.Children.FirstOrDefault(x => x.Name == "Mob");
                    if (mobNode != null)
                    {
                        foreach (var item in mobNode.Children)
                        {
                            if (int.TryParse(item.Name, out var mobId))
                            {
                                var template = new MobWithBossHpBarTemplate(mobId);
                                InsertItem(template);
                                list.Add(template);
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
