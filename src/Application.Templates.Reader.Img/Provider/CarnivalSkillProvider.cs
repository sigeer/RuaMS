using Application.Templates.PartyQuest;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class CarnivalSkillProvider : AbstractAllProvider<CarnivalSkillTemplate>
    {
        public override ProviderType Type => ProviderType.CarnivalSkill;

        public CarnivalSkillProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected override IEnumerable<CarnivalSkillTemplate> GetDataFromImg()
        {
            try
            {
                List<CarnivalSkillTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var root = new WZImage(fullPath);
                    foreach (var item in root.Children)
                    {
                        if (int.TryParse(item.Name, out var id))
                        {
                            var template = new CarnivalSkillTemplate(id)
                            {
                                SpendCP = item.GetIntValue("spendCP"),
                                MobSkillId = item.GetIntValue("mobSkillID"),
                                Level = item.GetIntValue("level"),
                                TargetsAll = item.GetIntValue("target") > 1,
                            };
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
