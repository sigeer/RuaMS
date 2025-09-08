using Microsoft.CodeAnalysis;

namespace XmlWzReader.SouceGenerator
{
    [Generator]
    public class CodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new WZPathSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // System.Diagnostics.Debugger.Launch();
            if (context.SyntaxReceiver is WZPathSyntaxReceiver receiver)
            {
                var compilation = context.Compilation;

                foreach (var classSyntax in receiver.CandidateClasses)
                {
                    var model = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                    var classSymbol = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
                    if (classSymbol == null) continue;
                    if (classSymbol.IsAbstract) continue;

                    var obj = new GeneratorScope(context, classSyntax);
                    obj.Build();
                }

            }
        }
    }

}
