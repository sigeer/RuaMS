using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Skill;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
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
                    var rootNode = new WZImage(fullPath);

                    foreach (var node in rootNode.Children)
                    {
                        if (int.TryParse(node.Name, out var skillId))
                        {
                            var template = new MobSkillTemplate(skillId);
                            MobSkillTemplateGenerated_Duey.ApplyProperties(template, node);
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
