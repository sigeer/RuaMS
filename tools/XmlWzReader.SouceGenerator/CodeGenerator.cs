using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace XmlWzReader.SouceGenerator
{
    [Generator]
    public class CodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // context.RegisterForSyntaxNotifications(() => new WZPathSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            var compilation = context.Compilation;

            var aAssembly = compilation.References
                .Select(r => compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol)
                .FirstOrDefault(a => a?.Name == "Application.Templates");

            if (aAssembly != null)
            {
                foreach (var type in aAssembly.GlobalNamespace.GetNamespaceMembers()
                             .SelectMany(ns => GetAllTypes(ns)))
                {

                    if (type.IsAbstract) continue;

                    foreach (var attr in type.GetAttributes())
                    {
                        if (attr.AttributeClass?.ToDisplayString() == "Application.Templates.GenerateTagAttribute")
                        {
                            // 对 A 库类生成代码
                            var obj = new GeneratorScope(context, type);
                            obj.Build();
                        }
                    }
                }
            }
        }

        IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
                yield return type;

            foreach (var subNs in ns.GetNamespaceMembers())
                foreach (var t in GetAllTypes(subNs))
                    yield return t;
        }
    }

}
