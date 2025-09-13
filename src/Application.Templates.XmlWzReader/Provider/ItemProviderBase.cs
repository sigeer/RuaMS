using Application.Templates.Providers;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public abstract class ItemProviderBase : AbstractProvider<AbstractItemTemplate>
    {
        protected ItemProviderBase(TemplateOptions options) : base(options)
        {
        }
    }
}
