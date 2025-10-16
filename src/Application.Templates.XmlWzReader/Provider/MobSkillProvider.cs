using Application.Templates.Providers;
using Application.Templates.Skill;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class MobSkillProvider : AbstractAllProvider<MobSkillTemplate>
    {
        public override string ProviderName => ProviderNames.Skill;

        public MobSkillProvider(TemplateOptions options) : base(options, "MobSkill.img.xml")
        {
        }


        protected override IEnumerable<AbstractTemplate> GetDataFromImg()
        {
            try
            {
                List<AbstractTemplate> all = [];
                using var fis = _fileProvider.ReadFile(_file);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);

                var xDoc = XDocument.Load(reader)!.Root!;

                foreach (var node in xDoc.Elements())
                {
                    if (int.TryParse(node.GetName(), out var skillId))
                    {
                        var template = new MobSkillTemplate(skillId);
                        MobSkillTemplateGenerated.ApplyProperties(template, node);
                        InsertItem(template);
                        all.Add(template);
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
