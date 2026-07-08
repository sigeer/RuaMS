using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class EtcMakeCharInfoProvider : AbstractAllProvider<MakerCharInfoTemplate>
    {
        public EtcMakeCharInfoProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        public override ProviderType Type => ProviderType.EtcMakeCharInfo;

        protected override IEnumerable<MakerCharInfoTemplate> GetDataFromImg()
        {
            try
            {
                List<MakerCharInfoTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var rootNode = new WZImage(fullPath);

                    MakerCharInfoTemplate template = new();
                    MakerCharInfoTemplateGenerated_Duey.ApplyProperties(template, rootNode);
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
