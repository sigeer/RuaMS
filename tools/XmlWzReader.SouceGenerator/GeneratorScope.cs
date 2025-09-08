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

            var sourceTree = TreeNode.BuildTree(_properyMapping);

            ProcessObject(sb, sourceTree, instanceName, "elementNode", true);
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

                    if (elementType is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
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


        void ProcessObject(StringBuilder sb, TreeNode node, string instanceName, string rootNodeName, bool isDir)
        {
            var objProps = node.Children.Where(x => x.Value == null).ToArray();
            var nodeVariableName = $"nodeInner{node.Depth}";
            if (isDir)
            {
                sb.AppendLineWithPreSpace($"        foreach (var {nodeVariableName} in {rootNodeName}.Elements())", node.Depth);
                sb.AppendLineWithPreSpace("        {", node.Depth);
            }
            var currentNode = isDir ? nodeVariableName : rootNodeName;
            foreach (var objNode in objProps)
            {
                if (isDir)
                {
                    sb.AppendLineWithPreSpace($"            if ({currentNode}.GetName() == \"{objNode.Name}\")", node.Depth);
                    sb.AppendLineWithPreSpace("            {", node.Depth);
                }

                var needDir = objNode.Children.Any(x => x.Value == null)
                    || (objNode.Children.Where(y => y.Value != null).Any(y => y.Value.Type == 0) && !objNode.Children.Where(y => y.Value != null).Any(y => y.Value.Type == 1));
                ProcessObject(sb, objNode, instanceName, currentNode, needDir);

                if (isDir)
                {
                    sb.AppendLineWithPreSpace("            }", node.Depth);
                }

            }

            var baseTypeData = node.Children.Where(x => x.Value != null && x.Value.Type == 0).ToList();
            var baseArrData = node.Children.Where(x => x.Value != null && x.Value.Type == 1).ToList();
            if (baseTypeData.Count > 0 && baseArrData.Count > 0)
            {
                ProcessMixed(sb, node.Depth + 1, baseArrData, baseTypeData, instanceName, currentNode);
            }
            else if (baseTypeData.Count > 0)
            {
                ProcessBaseDataType(sb, node.Depth + 1, baseTypeData, instanceName, currentNode);
            }
            else if (baseArrData.Count > 0)
            {
                ProcessIntArrayType(sb, node.Depth + 1, baseArrData, instanceName, currentNode);
            }

            var objArrPropData = node.Children.Where(x => x.Value != null && x.Value.Type == 2).ToList();
            if (objArrPropData.Count > 0)
            {
                ProcessObjectArrayType(sb, node.Depth + 1, objArrPropData, instanceName, currentNode);
            }

            var objPropData = node.Children.Where(x => x.Value != null && x.Value.Type == 3).ToList();
            if (objPropData.Count > 0)
            {
                ProcessInnerObject(sb, node.Depth + 1, objPropData, instanceName, currentNode);
            }

            if (isDir)
            {
                sb.AppendLineWithPreSpace("        }", node.Depth);
            }
        }

        void ProcessBaseDataType(StringBuilder sb, int depth, List<TreeNode> baseDatas, string instanceName, string currentNode)
        {

            var nodeVariableName = $"nodeProp{depth}";
            sb.AppendLineWithPreSpace($"        switch({currentNode}.GetName())", depth);
            sb.AppendLineWithPreSpace("        {", depth);
            foreach (var prop in baseDatas)
            {
                string getter = prop.Value.DataType switch
                {
                    "string" => "GetStringValue()",
                    "string?" => "GetStringValue()",
                    "int" => "GetIntValue()",
                    "bool" => "GetBoolValue()",
                    "Point" => "GetVectorValue()",
                    "double" => "GetDoubleValue()",
                    "double?" => "GetDoubleValue()",
                    "float" => "GetFloatValue()",
                    _ => "GetStringValue()"
                };
                sb.AppendLineWithPreSpace($"                case \"{prop.Name}\": {instanceName}.{prop.Value.DataName} = {currentNode}.{getter}; break;", depth);
            }

            sb.AppendLineWithPreSpace($"                default: break;", depth);
            sb.AppendLineWithPreSpace("        }", depth);
        }

        void ProcessIntArrayType(StringBuilder sb, int depth, List<TreeNode> baseDatas, string instanceName, string currentNode)
        {
            int localId = 0;
            var nodeVariableName = $"nodeItem{depth}";

            foreach (var prop in baseDatas)
            {
                var listName = $"listInt{depth}_{localId}";

                //if (prop.Value.Name != "-")
                //{
                //    sb.AppendLineWithPreSpace($"        if ({currentNode}.GetName() == \"{prop.Value.Name}\")", node.Depth);
                //    sb.AppendLineWithPreSpace("        {", node.Depth);
                //}

                sb.AppendLineWithPreSpace($"            List<int> {listName} = new List<int>();", depth);
                sb.AppendLineWithPreSpace($"            foreach (var {nodeVariableName} in {currentNode}.Elements())", depth);
                sb.AppendLineWithPreSpace("            {", depth);
                sb.AppendLineWithPreSpace($"                if (int.TryParse({nodeVariableName}.GetName(), out var _))", depth);
                sb.AppendLineWithPreSpace("                {", depth);
                sb.AppendLineWithPreSpace($"                     {listName}.Add({nodeVariableName}.GetIntValue());", depth);
                sb.AppendLineWithPreSpace("                }", depth);
                sb.AppendLineWithPreSpace("            }", depth);
                sb.AppendLineWithPreSpace($"            {instanceName}.{prop.Value.DataName} = {listName}.ToArray();", depth);

                //if (prop.Value.Name != "-")
                //{
                //    sb.AppendLineWithPreSpace("        }", node.Depth);
                //}

                localId++;
            }


        }

        void ProcessObjectArrayType(StringBuilder sb, int depth, List<TreeNode> baseDatas, string instanceName, string currentNode)
        {
            var nodeVariableName = $"nodeItem{depth}";

            int localId = 0;
            foreach (var prop in baseDatas)
            {
                var modelName = $"modelArr{depth}_{localId}";
                var listName = $"listObj{depth}_{localId}";

                //sb.AppendLineWithPreSpace($"        if ({currentNode}.GetName() == \"{prop.Value.Name}\")", depth);
                //sb.AppendLineWithPreSpace("        {", depth);

                sb.AppendLineWithPreSpace($"            List<{prop.Value.DataType.TrimEnd('?')}> {listName} = new List<{prop.Value.DataType.TrimEnd('?')}>();", depth);
                sb.AppendLineWithPreSpace($"            foreach (var {nodeVariableName} in {currentNode}.Elements())", depth);
                sb.AppendLineWithPreSpace("            {", depth);
                sb.AppendLineWithPreSpace($"                var {modelName} = new {prop.Value.DataType.TrimEnd('?')}();", depth);
                sb.AppendLineWithPreSpace($"                if (int.TryParse({nodeVariableName}.GetName(), out var idx))", depth);
                sb.AppendLineWithPreSpace("                {", depth);
                var newObjSB = new StringBuilder();
                ProcessObject(newObjSB, prop, modelName, nodeVariableName, true);
                sb.Append(newObjSB.ToString());
                sb.AppendLineWithPreSpace($"                     {listName}.Add({modelName});", depth);
                sb.AppendLineWithPreSpace("                }", depth);
                sb.AppendLineWithPreSpace("            }", depth);
                sb.AppendLineWithPreSpace($"            {instanceName}.{prop.Value.DataName} = {listName}.ToArray();", depth);

                //sb.AppendLineWithPreSpace("        }", depth);

                localId++;
            }


        }

        void ProcessInnerObject(StringBuilder sb, int depth, List<TreeNode> baseDatas, string instanceName, string currentNode)
        {

            int localId = 0;
            foreach (var prop in baseDatas)
            {
                var modelName = $"modelArr{depth}_{localId}";

                sb.AppendLineWithPreSpace($"        if ({currentNode}.GetName() == \"{prop.Value.Name}\")", depth);
                sb.AppendLineWithPreSpace("        {", depth);

                sb.AppendLineWithPreSpace($"            var {modelName} = new {prop.Value.DataType.TrimEnd('?')}();", depth);
                ProcessObject(sb, prop, modelName, currentNode, true);
                sb.AppendLineWithPreSpace($"            {instanceName}.{prop.Value.DataName} = {modelName};", depth);

                sb.AppendLineWithPreSpace("        }", depth);
                localId++;
            }
        }

        /// <summary>
        /// 同时包含普通数组和object
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="depth"></param>
        /// <param name="baseDatas"></param>
        /// <param name="instanceName"></param>
        /// <param name="currentNode"></param>
        void ProcessMixed(StringBuilder sb, int depth, List<TreeNode> arrayItems, List<TreeNode> propItems, string instanceName, string currentNode)
        {
            int localId = 0;

            foreach (var prop in arrayItems)
            {
                var listName = $"listMixedInt{depth}_{localId}";
                var nodeVariableName = $"nodeItem{depth}_{localId}";
                var nodeVariableNameValue = $"{nodeVariableName}_value";

                sb.AppendLineWithPreSpace($"            List<int> {listName} = new List<int>();", depth);
                sb.AppendLineWithPreSpace($"            foreach (var {nodeVariableName} in {currentNode}.Elements())", depth);
                sb.AppendLineWithPreSpace("            {", depth);
                sb.AppendLineWithPreSpace($"                var {nodeVariableNameValue} = {nodeVariableName}.GetName();", depth);
                sb.AppendLineWithPreSpace($"                if (int.TryParse({nodeVariableNameValue}, out var idx))", depth);
                sb.AppendLineWithPreSpace("                {", depth);
                sb.AppendLineWithPreSpace($"                     {listName}.Add({nodeVariableName}.GetIntValue());", depth);
                sb.AppendLineWithPreSpace("                }", depth);
                sb.AppendLineWithPreSpace("                else", depth);
                sb.AppendLineWithPreSpace("                {", depth);
                ProcessBaseDataType(sb, depth, propItems, instanceName, nodeVariableName);
                sb.AppendLineWithPreSpace("                }", depth);
                sb.AppendLineWithPreSpace("            }", depth);
                sb.AppendLineWithPreSpace($"            {instanceName}.{prop.Value.DataName} = {listName}.ToArray();", depth);


                localId++;
            }


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
