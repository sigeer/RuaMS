using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.Skill;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
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
                using var fis = File.Open(_resolver.ResolveFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                var skillElement = xDoc.Elements().FirstOrDefault(x => x.Attribute("name")?.Value == "skill");
                if (skillElement != null)
                {
                    foreach (var skillEle in skillElement.Elements())
                    {
                        if (int.TryParse(skillEle.Attribute("name")?.Value, out var skillId))
                        {
                            var pEntry = new SkillTemplate(skillId);
                            SkillTemplateGenerated.ApplyProperties(pEntry, skillEle);
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
