using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public class EtcScriptInfoProvider : AbstractAllProvider<ScriptInfoTemplate>
    {
        public override ProviderType Type => ProviderType.EtcScriptInfo;

        public EtcScriptInfoProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<ScriptInfoTemplate> GetDataFromImg()
        {
            try
            {
                List<ScriptInfoTemplate> list = [];
                var idx = 0;
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    foreach (var item in root.Elements())
                    {
                        var template = new ScriptInfoTemplate(idx);
                        template.Name = item.GetName() ?? "";
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
