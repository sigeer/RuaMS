using Application.Templates.String;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Application.Templates.Reader
{
    public abstract class AbstractStringProvider : AbstractProvider<StringTemplateBase>
    {
        protected CultureInfo _culture;

        protected AbstractStringProvider(CultureInfo currentCulture, IWzPathResolver resolver) : base(resolver)
        {
            _culture = currentCulture;
        }

        protected static List<StringTemplateBase> ExternalTemplates = [
            new StringTemplate(0) { Name = "金币"},
            new StringTemplate(-1) { Name = "点券"},
            new StringTemplate(-2) { Name = "抵用券"},
            new StringTemplate(-3) { Name = "信用点"},
            new StringTemplate(-4) { Name = "经验"}
        ];


        protected override StringTemplateBase? GetItemInternal(int templateId)
        {
            return LoadAll().FirstOrDefault(x => x.TemplateId == templateId);
        }

        protected override IEnumerable<StringTemplateBase> LoadAllInternal()
        {
            List<StringTemplateBase> all = [];
            try
            {
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    all.AddRange(GetDataFromImg(file));
                }

                if (Type == ProviderType.StringItem)
                {
                    foreach (var item in ExternalTemplates)
                    {
                        InsertItem(item);
                        all.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
            }
            return all;
        }

        protected abstract IEnumerable<StringTemplateBase> GetDataFromImg(string path);


        public virtual IEnumerable<StringTemplateBase> Search(string searchText, int maxCount = 50)
        {
            return LoadAll()
                .Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || x.TemplateId.ToString().Contains(searchText))
                .OrderByDescending(x => x.Name == searchText)          // 第一优先级：Name精确
                .ThenByDescending(x => x.TemplateId.ToString() == searchText) // 第二优先级：Id精确
                .ThenBy(x => x.Name) // 兜底排序
                .Take(maxCount);
        }
    }
}
