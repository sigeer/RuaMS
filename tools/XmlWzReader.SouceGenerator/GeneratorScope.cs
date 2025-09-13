using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlWzReader.SouceGenerator
{
    public class GeneratorScope : IDisposable
    {
        readonly GeneratorExecutionContext _context;
        readonly ClassDeclarationSyntax _classSyntax;

        Dictionary<string, PropertyData> _properyMapping = new Dictionary<string, PropertyData>();

        public GeneratorScope(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            _context = context;
            _classSyntax = classDeclarationSyntax;
        }

        IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
        {
            var seen = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);

            foreach (var prop in type.GetMembers().OfType<IPropertySymbol>())
            {
                var original = prop.OverriddenProperty ?? prop;

                if (seen.Add(original))
                {
                    yield return prop; // 返回子类的 override 属性，而不是基类的
                }
            }

            // 递归处理基类
            if (type.BaseType != null)
            {
                foreach (var prop in GetAllProperties(type.BaseType))
                {
                    if (seen.Add(prop))
                    {
                        yield return prop;
                    }
                }
            }
        }


        public void Build()
        {
            var compilation = _context.Compilation;
            var model = compilation.GetSemanticModel(_classSyntax.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(_classSyntax) as INamedTypeSymbol;
            if (classSymbol == null)
                return;
            if (classSymbol.IsAbstract)
                return;
            var tagAttr = classSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "GenerateTagAttribute");
            if (tagAttr == null)
                return;

            var instanceName = "template";
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Application.Templates;");
            sb.AppendLine($"using {classSymbol.ContainingNamespace.ToString()};");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Application.Templates.XmlWzReader;");
            sb.AppendLine("using System.Xml.Linq;");
            sb.AppendLine($"public static partial class {classSymbol.Name}Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    public static void ApplyProperties({classSymbol.Name} {instanceName}, XElement elementNode)");
            sb.AppendLine("    {");

            //没有Attribute时，根据结构层次查找，有Attribute时，以Attribute的结构层次查找
            SearchTree("", classSymbol);

            var sourceTree = TreeNode.BuildTreeCore(_properyMapping);

            var nodeVariableName = "rootNode";
            sb.AppendLine($"        foreach (var {nodeVariableName} in elementNode.Elements())");
            sb.AppendLine("        {");

            var nodeNameValue = "rootNodeName";
            sb.AppendLine($"            var {nodeNameValue} = {nodeVariableName}.GetName();");
            foreach (var item in sourceTree)
            {
                ProcessObject(sb, item, instanceName, nodeVariableName, nodeNameValue);
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            _context.AddSource($"{classSymbol.Name}Generated.g.cs", sb.ToString());
        }

        string GenratePath(string lastPath, string currentPath)
        {
            if (string.IsNullOrEmpty(lastPath))
                return currentPath;
            return lastPath + "/" + currentPath;
        }

        void SearchTree(string lastPath, INamedTypeSymbol classSymbol)
        {
            foreach (var propSymbol in GetAllProperties(classSymbol).OfType<IPropertySymbol>())
            {
                var wzAttr = propSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "WZPathAttribute");
                var propAttrValue = wzAttr == null ? null : wzAttr.ConstructorArguments[0].Value as string;

                var ignoreAttr = propSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "GenerateIgnorePropertyAttribute");
                if (ignoreAttr != null)
                    continue;

                if (propSymbol.Type is IArrayTypeSymbol arrayType) // 数组
                {
                    var elementType = arrayType.ElementType;

                    if (elementType is INamedTypeSymbol named && named.TypeKind == TypeKind.Class && named.SpecialType != SpecialType.System_String)
                    {
                        // 对象数组
                        var path = propAttrValue;
                        if (string.IsNullOrEmpty(path))
                            path = GenratePath(lastPath, propSymbol.Name.FirstCharToLower()) + "/-";

                        var pathArr = path.Split('/');
                        var matchName = pathArr[pathArr.Length - 2];
                        _properyMapping[path] = new PropertyData(2, matchName, elementType.ToString(), propSymbol.Name);
                        SearchTree(path, named);
                    }
                    else
                    {
                        // 普通数组 (int[], string[] 等)
                        var path = propAttrValue;
                        if (string.IsNullOrEmpty(path))
                            path = GenratePath(lastPath, propSymbol.Name.FirstCharToLower()) + "/-";

                        var pathArr = path.Split('/');
                        var matchName = pathArr[pathArr.Length - 2];
                        _properyMapping[path] = new PropertyData(1, matchName, elementType.ToString(), propSymbol.Name);
                    }
                }
                else if (propSymbol.Type is INamedTypeSymbol namedType)
                {
                    var path = propAttrValue;
                    if (string.IsNullOrEmpty(path))
                        path = GenratePath(lastPath, propSymbol.Name.FirstCharToLower());
                    var matchName = path.Split('/').Last();

                    if (namedType.TypeKind == TypeKind.Class &&
                        namedType.SpecialType != SpecialType.System_String)
                    {
                        // 普通对象
                        _properyMapping[path] = new PropertyData(3, matchName, propSymbol.Type.ToString(), propSymbol.Name);
                        SearchTree(path, namedType);
                    }
                    else
                    {
                        // 基础类型 (int, string, bool 等)
                        _properyMapping[path] = new PropertyData(0, matchName, propSymbol.Type.ToString(), propSymbol.Name);
                    }
                }
                else
                {
                    // 其它情况 (泛型参数、指针等)
                    Console.WriteLine($"未考虑到的类型：{propSymbol.Name}");
                }
            }
        }


        void ProcessObject(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            //一些通过WZPath跳过的中间节点
            var isDir = node.Value == null && node.Children.Count > 0;
            if (isDir)
            {
                sb.AppendLine($"            if ({nodeNameValue} == \"{node.Name}\")");
                sb.AppendLine("            {");
            }

            var arrItem = node.Children.FirstOrDefault(x => x.Value != null && (x.Value.Type == 1 || x.Value.Type == 2));
            var listName = $"listArr{node.Depth}";
            if (arrItem != null)
            {
                sb.AppendLineWithPreSpace($"                List<{arrItem.Value.DataType.TrimEnd('?')}> {listName} = new List<{arrItem.Value.DataType.TrimEnd('?')}>();", node.Depth);
            }

            var innerNodeName = $"node{node.Depth}";
            var innerNodeNameValue = $"nodeName{node.Depth}";

            sb.AppendLineWithPreSpace($"                foreach (var {innerNodeName} in {currentNode}.Elements())", node.Depth);
            sb.AppendLineWithPreSpace("                {", node.Depth);
            sb.AppendLineWithPreSpace($"                    var {innerNodeNameValue} = {innerNodeName}.GetName();", node.Depth);

            foreach (var objNode in node.Children)
            {
                if (objNode.Value == null)
                {
                    ProcessObject(sb, objNode, instanceName, innerNodeName, innerNodeNameValue);
                }
                else
                {
                    ProcessBaseDataType(sb, objNode, instanceName, innerNodeName, innerNodeNameValue);
                    ProcessInnerObject(sb, objNode, instanceName, innerNodeName, innerNodeNameValue);
                }
            }

            // 数组、对象数组同级只会存在一条
            if (arrItem != null)
            {
                ProcessArrayType(sb, arrItem, listName, innerNodeName, innerNodeNameValue);
                ProcessObjectArrayType(sb, arrItem, listName, innerNodeName, innerNodeNameValue);
            }

            sb.AppendLineWithPreSpace("                }", node.Depth);

            if (arrItem != null)
            {
                sb.AppendLineWithPreSpace($"                {instanceName}.{arrItem.Value.DataName} = {listName}.ToArray();", node.Depth);
            }

            if (isDir)
            {
                sb.AppendLine("            }");
            }
        }

        string GetBaseFunction(string dataType)
        {
            string getter = dataType switch
            {
                "string" => "GetStringValue()",
                "string?" => "GetStringValue()",
                "int" => "GetIntValue()",
                "int?" => "GetIntValue()",
                "bool" => "GetBoolValue()",
                "System.Drawing.Point" => "GetVectorValue()",
                "double" => "GetDoubleValue()",
                "double?" => "GetDoubleValue()",
                "float" => "GetFloatValue()",
                "float?" => "GetFloatValue()",
                _ => "GetStringValue()"
            };
            return getter;
        }

        void ProcessBaseDataType(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            if (node.Value != null && node.Value.Type == 0)
            {
                if (node.Name == "$name")
                {
                    sb.AppendLineWithPreSpace($"        {instanceName}.{node.Value.DataName} = idx;", node.Depth);
                }
                else
                {
                    sb.AppendLineWithPreSpace($"        if ({nodeNameValue} == \"{node.Name}\")", node.Depth);
                    sb.AppendLineWithPreSpace($"            {instanceName}.{node.Value.DataName} = {currentNode}.{GetBaseFunction(node.Value.DataType)};", node.Depth);
                }

            }
        }

        void ProcessArrayType(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            if (node.Value == null || node.Value.Type != 1)
                return;

            sb.AppendLineWithPreSpace($"                if (int.TryParse({currentNode}.GetName(), out var _))", node.Depth);
            sb.AppendLineWithPreSpace("                {", node.Depth);
            sb.AppendLineWithPreSpace($"                     {instanceName}.Add({currentNode}.{GetBaseFunction(node.Value.DataType)});", node.Depth);
            sb.AppendLineWithPreSpace("                }", node.Depth);
        }

        void ProcessObjectArrayType(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            if (node.Value == null || node.Value.Type != 2)
                return;

            var modelName = $"modelArr{node.Depth}";

            sb.AppendLineWithPreSpace($"                if (int.TryParse({nodeNameValue}, out var idx))", node.Depth);
            sb.AppendLineWithPreSpace("                {", node.Depth);
            sb.AppendLineWithPreSpace($"                    var {modelName} = new {node.Value.DataType}();", node.Depth);
            ProcessObject(sb, node, modelName, currentNode, nodeNameValue);
            sb.AppendLineWithPreSpace($"                     {instanceName}.Add({modelName});", node.Depth);
            sb.AppendLineWithPreSpace("                }", node.Depth);

        }

        void ProcessInnerObject(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {

            if (node.Value == null || node.Value.Type != 3)
                return;

            var modelName = $"modelInnerObj{node.Depth}";
            var innerNodeName = $"modelInnerNode{node.Depth}";
            var innerNodeNameValue = $"modelInnerNodeName{node.Depth}";

            sb.AppendLineWithPreSpace($"        if ({nodeNameValue} == \"{node.Name}\")", node.Depth);
            sb.AppendLineWithPreSpace("        {", node.Depth);
            sb.AppendLineWithPreSpace($"            var {modelName} = new {node.Value.DataType.TrimEnd('?')}();", node.Depth);
            ProcessObject(sb, node, modelName, currentNode, nodeNameValue);
            sb.AppendLineWithPreSpace($"            {instanceName}.{node.Value.DataName} = {modelName};", node.Depth);
            sb.AppendLineWithPreSpace("        }", node.Depth);
        }
        public void Dispose()
        {
            _properyMapping.Clear();
        }
    }

    public class PropertyData
    {

        /// <summary>
        /// 0. 基本类型 1. 普通数组 2. 对象数组 3. 对象
        /// </summary>
        public PropertyData(int type, string name, string dataType, string dataName)
        {
            Type = type;
            Name = name;
            DataType = dataType;
            DataName = dataName;
        }
        /// <summary>
        /// 0. 基本类型 1. 普通数组 2. 对象数组 3. 对象
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 匹配名（xml中的name）
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 属性名
        /// </summary>
        public string DataName { get; set; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public string DataType { get; set; }
    }

}
