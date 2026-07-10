using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Xml.Provider
{
    public class EtcMakeCharInfoProvider : AbstractAllProvider<MakerCharInfoTemplate>
    {
        public override ProviderType Type => ProviderType.EtcMakeCharInfo;

        public EtcMakeCharInfoProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<MakerCharInfoTemplate> GetDataFromImg()
        {
            try
            {
                List<MakerCharInfoTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    MakerCharInfoTemplate template = new MakerCharInfoTemplate();
                    MakerCharInfoTemplateGenerated.ApplyProperties(template, root);
                    InsertItem(template);
                    list.Add(template);
                }
                return list;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
