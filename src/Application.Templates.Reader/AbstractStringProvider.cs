using Application.Templates.String;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Application.Templates.Reader
{
    public abstract class AbstractStringProvider : AbstractProvider<AbstractTemplate>
    {
        protected CultureInfo _culture;

        protected AbstractStringProvider(CultureInfo currentCulture, IWzPathResolver resolver) : base(resolver)
        {
            _culture = currentCulture;
        }



        protected override AbstractTemplate? GetItemInternal(int templateId)
        {
            return LoadAll().FirstOrDefault(x => x.TemplateId == templateId);
        }

        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = [];
            try
            {
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    all.AddRange(GetDataFromImg(file));
                }
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
            }
            return all;
        }

        protected abstract IEnumerable<AbstractTemplate> GetDataFromImg(string path);

        public virtual IEnumerable<AbstractTemplate> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringTemplate>()
                .Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Take(maxCount);
        }
    }
}
