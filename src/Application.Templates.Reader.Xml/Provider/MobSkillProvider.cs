using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Skill;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public class MobSkillProvider : AbstractAllProvider<MobSkillTemplate>
    {
        public override ProviderType Type => ProviderType.MobSkill;

        public MobSkillProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected override IEnumerable<MobSkillTemplate> GetDataFromImg()
        {
            try
            {
                List<MobSkillTemplate> all = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    foreach (var node in root.Elements())
                    {
                        if (int.TryParse(node.GetName(), out var skillId))
                        {
                            var template = new MobSkillTemplate(skillId);
                            MobSkillTemplateGenerated.ApplyProperties(template, node);
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
