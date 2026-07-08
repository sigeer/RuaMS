using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Skill;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class SkillProvider : AbstractGroupProvider<SkillTemplate>
    {
        public override ProviderType Type => ProviderType.Skill;
        public SkillProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected override IEnumerable<SkillTemplate> GetDataFromImg(string? filePath)
        {
            try
            {
                List<SkillTemplate> imgData = [];
                var fullPath = _resolver.ResolveFullPath(filePath);
                var rootNode = new WZImage(fullPath);

                var skillNode = rootNode.Children.FirstOrDefault(x => x.Name == "skill");
                if (skillNode != null)
                {
                    foreach (var skillEle in skillNode.Children)
                    {
                        if (int.TryParse(skillEle.Name, out var skillId))
                        {
                            var pEntry = new SkillTemplate(skillId);
                            SkillTemplateGenerated_Duey.ApplyProperties(pEntry, skillEle);
                            InsertItem(pEntry);
                            imgData.Add(pEntry);
                        }
                    }
                }
                return imgData;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}