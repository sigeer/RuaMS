using Application.Scripting;
using Application.Templates.Providers;
using Application.Templates.Reactor;
using Application.Templates.XmlWzReader.Provider;
using System.Text;

namespace Application.Host.Services
{
    public class Demo
    {
        public static void ExportReactors()
        {
            StringBuilder sb = new();
            var provider = ProviderSource.Instance.GetProvider<ReactorProvider>();
            var all = provider.LoadAll();


            var allEffectReactors = ScriptSource.Instance.GetSubScripts("reactor").Select(x =>
            {
                if (int.TryParse(x, out var v))
                {
                    return v;
                }
                return -1;
            }).Where(x => x > 0).ToArray();

            var a = all.Where(x => allEffectReactors.Contains(x.TemplateId)).OfType<ReactorTemplate>().OrderBy(x => x.Action)
                .GroupBy(x => x.Action).ToList();

            foreach (var item in a)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    var comm = "-- ";
                    foreach (var id in item)
                    {
                        comm += id.TemplateId + ", ";
                    }
                    sb.Append($"""
                    {comm}
                    function {item.Key}( ... )
                        
                    end
                    """);
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }

            var noAction = all.Where(x => allEffectReactors.Contains(x.TemplateId)).OfType<ReactorTemplate>().Where(x => x.Action == null).Select(x => x.TemplateId).ToList();
            var noActionIds = string.Join(", ", noAction);
            sb.Append("-- ").Append(noActionIds);

            File.WriteAllText("reactor.lua", sb.ToString());
        }
    }
}
