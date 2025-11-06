using Application.Templates.Providers;
using Application.Templates.Skill;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public class SkillProvider : AbstractGroupProvider<SkillTemplate>
    {
        public override string ProviderName => ProviderNames.Skill;
        public SkillProvider(ProviderOption options) : base(options)
        {
        }

        protected override string? GetImgPathByTemplateId(int key)
        {
            var jobId = key / 10000;
            var imgName = (jobId == 0 ? "000" : jobId.ToString()) + ".img.xml";
            return Path.Combine(ProviderName, imgName);
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string? filePath)
        {
            try
            {
                List<AbstractTemplate> imgData = [];
                using var fis = _fileProvider.ReadFile(filePath);
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
