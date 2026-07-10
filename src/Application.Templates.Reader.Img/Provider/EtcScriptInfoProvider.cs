using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public class EtcScriptInfoProvider : AbstractAllProvider<ScriptInfoTemplate>
    {
        public EtcScriptInfoProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        public override ProviderType Type =>  ProviderType.EtcScriptInfo;

        protected override IEnumerable<ScriptInfoTemplate> GetDataFromImg()
        {
            try
            {
                List<ScriptInfoTemplate> list = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var rootNode = new WZImage(fullPath);

                    var idx = 0;
                    foreach (var item in rootNode.Children)
                    {
                        var template = new ScriptInfoTemplate(idx);
                        template.Name = item.Name ?? "";
                        template.Value = item.GetStringValue() ?? "";
                        InsertItem(template);
                        list.Add(template);
                        idx++;
                    }
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
