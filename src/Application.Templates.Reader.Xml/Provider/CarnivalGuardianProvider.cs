using Application.Templates.PartyQuest;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class CarnivalGuardianProvider : AbstractAllProvider<CarnivalGuardianTemplate>
    {
        public override ProviderType Type => ProviderType.CarnivalGuardian;

        public CarnivalGuardianProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<CarnivalGuardianTemplate> GetDataFromImg()
        {
            try
            {
                List<CarnivalGuardianTemplate> all = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;
                    foreach (var item in root.Elements())
                    {
                        if (int.TryParse(item.GetName(), out var id))
                        {
                            var template = new CarnivalGuardianTemplate(id);
                            var spendCpEl = item.Elements().FirstOrDefault(e => e.GetName() == "spendCP");
                            if (spendCpEl != null)
                                template.SpendCP = spendCpEl.GetIntValue();
                            var mobSkillEl = item.Elements().FirstOrDefault(e => e.GetName() == "mobSkillID");
                            if (mobSkillEl != null)
                                template.MobSkillId = mobSkillEl.GetIntValue();
                            var levelEl = item.Elements().FirstOrDefault(e => e.GetName() == "level");
                            if (levelEl != null)
                                template.Level = levelEl.GetIntValue();
                            InsertItem(template);
                            all.Add(template);
                        }
                    }
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
