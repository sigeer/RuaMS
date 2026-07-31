using Application.Templates.Reader;
using Application.Templates.String;

namespace Application.Host.Services
{
    /// <summary>
    /// 通过名称找到对应ID
    /// </summary>
    public class DataIdService
    {
        public List<IdName> QueryMap(string text, string locale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return [];
            }
            var provider = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale);
            return provider.Search(StringCategory.Map, text).OfType<StringMapTemplate>()
                .Select(x => new IdName(x.TemplateId, x.MapName, x.StreetName)).ToList();
        }
        public List<IdName> QueryMob(string text, string locale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return [];
            }
            var provider = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale);
            return provider.Search(StringCategory.Mob, text)
                .Select(x => new IdName(x.TemplateId, x.Name)).ToList();
        }

        public List<IdName> QueryItem(string text, string locale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return [];
            }
            var provider = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale);
            return provider.Search(StringCategory.Item, text).OfType<StringTemplate>()
                .Select(x => new IdName(x.TemplateId, x.Name, x.Description)).ToList();
        }

        public List<IdName> QueryNpc(string text, string locale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return [];
            }
            var provider = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale);
            return provider.Search(StringCategory.Npc, text)
                .Select(x => new IdName(x.TemplateId, x.Name)).ToList();
        }
        public List<IdName> QueryQuest(string text, string locale)
        {
            if (string.IsNullOrEmpty(text))
            {
                return [];
            }
            var provider = ProviderSource.Instance.GetProviderByKey<IStringProvider>(locale);
            return provider.Search(StringCategory.Quest, text)
                .Select(x => new IdName(x.TemplateId, x.Name)).ToList();
        }
    }

    public class IdName
    {
        public IdName(int id, string name): this(id, name, null)
        {
        }

        public IdName(int id, string name, string? desc)
        {
            Id = id;
            Name = name;
            Desc = desc;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Desc { get; set; }
    }
}
