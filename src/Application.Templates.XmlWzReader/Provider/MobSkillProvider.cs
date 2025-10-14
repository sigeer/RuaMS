using Application.Templates.Providers;
using Application.Templates.Skill;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class MobSkillProvider : AbstractProvider<MobSkillTemplate>
    {
        public override string ProviderName => ProviderNames.Skill;

        public override string[]? SingleImgFile => ["MobSkill.img.xml"];
        public MobSkillProvider(TemplateOptions options) : base(options)
        {
        }


        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? path)
        {
            List<AbstractTemplate> all = [];
            using var fis = _fileProvider.ReadFile(path);
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
    }
}
