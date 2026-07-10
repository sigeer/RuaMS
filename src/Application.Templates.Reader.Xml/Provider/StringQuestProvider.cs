using Application.Templates.Reader;
using Application.Templates.String;
using System.Globalization;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public class StringQuestProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringQuest;
        public StringQuestProvider(IWzPathResolver resolver, CultureInfo cultureInfo) : base(cultureInfo, resolver)
        {
        }

        protected override AbstractTemplate? SetStringTemplate(XElement rootNode)
        {
            if (int.TryParse(rootNode.Attribute("name")?.Value, out var questId))
            {
                var quest = new StringQuestTemplate(questId);
                foreach (var questProp in rootNode.Elements())
                {
                    var propName = questProp.GetName();
                    if (propName == "name")
                        quest.Name = questProp.GetStringValue() ?? string.Empty;
                    else if (propName == "parent")
                        quest.ParentName = questProp.GetStringValue() ?? string.Empty;
                }
                return quest;
            }
            return null;
        }

        public override IEnumerable<AbstractTemplate> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringQuestTemplate>().Where(x => 
                x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || x.ParentName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).Take(maxCount);
        }
    }
}
