using Application.Templates.Providers;
using Application.Templates.Skill;
using System.Xml;

namespace Application.Templates.XmlWzReader.Provider
{
    public class MobSkillProvider : AbstractProvider<MobSkillTemplate>
    {
        public override ProviderType ProviderName => ProviderType.MobSkill;

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
            using var reader = XmlReader.Create(fis, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
            if (reader.IsEmptyElement)
                return all;

            XmlReaderUtils.ReadChildNode(reader, skillNode =>
            {
                if (int.TryParse(skillNode.GetAttribute("name"), out var skillId))
                {
                    var pEntry = new MobSkillTemplate(skillId);
                    XmlReaderUtils.ReadChildNode(skillNode, levelNode =>
                    {
                        var nodeType = skillNode.GetAttribute("name") ?? string.Empty;
                        if (nodeType == "level")
                        {
                            XmlReaderUtils.ReadChildNode(levelNode, itemNode =>
                            {
                                if (int.TryParse(itemNode.GetAttribute("name"), out var skillLevel))
                                {
                                    var item = new MobSkillLevelData(skillId, skillLevel);
                                    List<int> summonIds = [];
                                    XmlReaderUtils.ReadChildNodeValue(itemNode, (name, value) =>
                                    {
                                        if (name == "x")
                                            item.X = Convert.ToByte(value);
                                        else if (name == "y")
                                            item.Y = Convert.ToByte(value);
                                        else if (name == "mpCon")
                                            item.MpCon = Convert.ToByte(value);
                                        else if (name == "interval")
                                            item.Interval = Convert.ToByte(value);
                                        else if (name == "time")
                                            item.Time = Convert.ToByte(value);
                                        else if (name == "hp")
                                            item.HP = Convert.ToByte(value);
                                        else if (name == "prop")
                                            item.Prop = Convert.ToByte(value);
                                        else if (name == "limit")
                                            item.Limit = Convert.ToByte(value);
                                        else if (name == "randomTarget")
                                            item.RandomTarget = Convert.ToInt32(value) > 0;
                                        else if (name == "summonEffect")
                                            item.SummonEffect = Convert.ToByte(value);
                                        else if (name == "count")
                                            item.Count = Convert.ToByte(value);
                                        else if (int.TryParse(name, out _))
                                            summonIds.Add(Convert.ToInt32(value));
                                    });
                                    item.SummonIDs = summonIds.ToArray();
                                    pEntry.InsertLevelData(item);
                                }
                            });
                        }
                    });
                    InsertItem(pEntry);
                    all.Add(pEntry);
                }
            });
            return all;
        }
    }
}
