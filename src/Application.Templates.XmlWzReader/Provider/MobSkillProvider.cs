using Application.Templates.Providers;
using Application.Templates.Skill;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class MobSkillProvider : AbstractProvider<MobSkillTemplate>
    {
        public override ProviderType ProviderName => ProviderType.Skill;

        string _imgPath;
        public MobSkillProvider(TemplateOptions options) : base(options)
        {
            _imgPath = Path.Combine(GetPath(), "MobSkill.img.xml");
        }

        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            return GetDataFromImg(string.Empty);
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            List<AbstractTemplate> all = [];
            using var fis = new FileStream(_imgPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
