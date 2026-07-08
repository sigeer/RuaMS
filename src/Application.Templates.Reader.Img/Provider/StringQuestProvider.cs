using Application.Templates.Reader;
using Application.Templates.String;
using Duey.Abstractions;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    public class StringQuestProvider : StringBaseProvider
    {
        public override ProviderType Type => ProviderType.StringQuest;
        public StringQuestProvider(CultureInfo cultureInfo, IWzPathResolver resolver) : base(cultureInfo, resolver)
        {
        }


        protected override AbstractTemplate? SetStringTemplate(IDataNode rootNode)
        {
            if (int.TryParse(rootNode.Name, out var questId))
            {
                var quest = new StringQuestTemplate(questId);
                foreach (var questProp in rootNode.Children)
                {
                    var propName = questProp.Name;
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
