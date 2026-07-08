using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.UI;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public class MobWithBossHpBarProvider : AbstractAllProvider<MobWithBossHpBarTemplate>
    {
        public override ProviderType Type => ProviderType.UIMobWithBossHpBar;

        public MobWithBossHpBarProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected override IEnumerable<MobWithBossHpBarTemplate> GetDataFromImg()
        {
            try
            {
                List<MobWithBossHpBarTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    var mobGage = root.Elements()
                        .FirstOrDefault(x => x.GetName() == "MobGage");

                    if (mobGage == null)
                        continue;

                    var mobNode = mobGage.Elements()
                        .FirstOrDefault(x => x.GetName() == "Mob");

                    if (mobNode == null)
                        continue;

                    foreach (var item in mobNode.Elements())
                    {
                        if (int.TryParse(item.GetName(), out var mobId))
                        {
                            var template = new MobWithBossHpBarTemplate(mobId);

                            InsertItem(template);
                            list.Add(template);
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
