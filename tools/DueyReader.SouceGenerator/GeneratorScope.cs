using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DueyReader.SouceGenerator
{
    public class GeneratorScope : IDisposable
    {
        readonly GeneratorExecutionContext _context;
        readonly INamedTypeSymbol classSymbol;

        Dictionary<string, PropertyData> _properyMapping = new Dictionary<string, PropertyData>();

        public GeneratorScope(GeneratorExecutionContext context, INamedTypeSymbol classSymbol)
        {
            _context = context;
            this.classSymbol = classSymbol;
        }

        IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
        {
            var seen = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);

            foreach (var prop in type.GetMembers().OfType<IPropertySymbol>())
            {
                var original = prop.OverriddenProperty ?? prop;

                if (seen.Add(original))
                {
                    yield return prop;
                }
            }

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
            if (classSymbol.IsAbstract)
                return;
            var tagAttr = classSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "GenerateTagAttribute");
            if (tagAttr == null)
                return;

            var instanceName = "template";
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Drawing;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Application.Templates;");
            sb.AppendLine($"using {classSymbol.ContainingNamespace.ToString()};");
            sb.AppendLine("using Duey.Abstractions;");
            sb.AppendLine("using Application.Templates.Reader.Img;");
            sb.AppendLine($"public static class {classSymbol.Name}Generated_Duey");
            sb.AppendLine("{");
            sb.AppendLine($"    public static void ApplyProperties({classSymbol.Name} {instanceName}, IDataNode elementNode)");
            sb.AppendLine("    {");

            SearchTree("", classSymbol);

            var sourceTree = TreeNode.BuildTreeCore(_properyMapping);
            var nodeVariableName = "rootNode";
            sb.AppendLine($"        foreach (var {nodeVariableName} in elementNode.Children)");
            sb.AppendLine("        {");

            var nodeNameValue = "rootNodeName";
            sb.AppendLine($"            var {nodeNameValue} = {nodeVariableName}.Name;");
            foreach (var item in sourceTree)
            {
                if (item.Value != null && item.Value.Type == 0)
                    ProcessBaseDataType(sb, item, instanceName, nodeVariableName, nodeNameValue);
                else if (item.Value != null && item.Value.Type == 3)
                    ProcessInnerObject(sb, item, instanceName, nodeVariableName, nodeNameValue);
                else
                    ProcessObject(sb, item, instanceName, nodeVariableName, nodeNameValue);
            }

            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            _context.AddSource($"{classSymbol.Name}Generated_Duey.g.cs", sb.ToString());
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

                if (propSymbol.SetMethod == null)
                    continue;

                if (propSymbol.Type is IArrayTypeSymbol arrayType)
                {
                    var elementType = arrayType.ElementType;

                    if (elementType is INamedTypeSymbol named && named.TypeKind == TypeKind.Class && named.SpecialType != SpecialType.System_String)
                    {
                        var path = propAttrValue;
                        if (string.IsNullOrEmpty(path))
                            path = GenratePath(lastPath, propSymbol.Name.FirstCharToLower()) + "/-";
                        else if (path.StartsWith("~"))
                            path = path.Replace("~", lastPath);

                        var pathArr = path.Split('/');
                        var matchName = pathArr[pathArr.Length - 2];
                        _properyMapping[path] = new PropertyData(2, matchName, elementType.ToString(), propSymbol.Name);
                        SearchTree(path, named);
                    }
                    else
                    {
                        var path = propAttrValue;
                        if (string.IsNullOrEmpty(path))
                            path = GenratePath(lastPath, propSymbol.Name.FirstCharToLower()) + "/-";
                        else if (path.StartsWith("~"))
                            path = path.Replace("~", lastPath);

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
                    else if (path.StartsWith("~"))
                        path = path.Replace("~", lastPath);

                    var matchName = path.Split('/').Last();

                    if (namedType.TypeKind == TypeKind.Class &&
                        namedType.SpecialType != SpecialType.System_String)
                    {
                        _properyMapping[path] = new PropertyData(3, matchName, propSymbol.Type.ToString(), propSymbol.Name);
                        SearchTree(path, namedType);
                    }
                    else
                    {
                        _properyMapping[path] = new PropertyData(0, matchName, propSymbol.Type.ToString(), propSymbol.Name);
                    }
                }
                else
                {
                    Console.WriteLine($"Unhandled type: {propSymbol.Name}");
                }
            }
        }

        void ProcessObject(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue, bool ignoreDir = false)
        {
            var isDir = !ignoreDir && (node.Value == null || node.Value.Type == 3) && node.Children.Count > 0;
            if (isDir)
            {
                sb.AppendLine($"            if ({nodeNameValue} == \"{node.Name}\")");
                sb.AppendLine("            {");
            }

            if (node.Value?.Name == "$existed")
            {
                sb.AppendLineWithPreSpace($"                    {instanceName}.{node.Value.DataName} = true;", node.Depth);
            }

            var arrItem = node.Children.FirstOrDefault(x => x.Value != null && (x.Value.Type == 1 || x.Value.Type == 2));
            var listName = $"listArr{node.Depth}";
            if (arrItem != null)
            {
                sb.AppendLineWithPreSpace($"                List<{arrItem.Value.DataType.TrimEnd('?')}> {listName} = new List<{arrItem.Value.DataType.TrimEnd('?')}>();", node.Depth);
            }

            var innerNodeName = currentNode;
            var innerNodeNameValue = nodeNameValue;

            var children = node.Children.Where(x => x.Value == null || x.Value.Type != 4).ToList();
            if (children.Count > 0)
            {
                innerNodeName = $"node{node.Depth}";
                innerNodeNameValue = $"nodeName{node.Depth}";

                sb.AppendLineWithPreSpace($"                foreach (var {innerNodeName} in {currentNode}.Children)", node.Depth);
                sb.AppendLineWithPreSpace("                {", node.Depth);
                sb.AppendLineWithPreSpace($"                    var {innerNodeNameValue} = {innerNodeName}.Name;", node.Depth);
            }

            foreach (var objNode in node.Children)
            {
                if (objNode.Value != null && objNode.Value.Type == 0)
                    ProcessBaseDataType(sb, objNode, instanceName, innerNodeName, innerNodeNameValue);
                else if (objNode.Value != null && objNode.Value.Type == 3)
                    ProcessInnerObject(sb, objNode, instanceName, innerNodeName, innerNodeNameValue);
                else if (objNode.Value != null && objNode.Name == "-")
                    continue;
                else
                    ProcessObject(sb, objNode, instanceName, innerNodeName, innerNodeNameValue);
            }

            if (arrItem != null)
            {
                ProcessArrayType(sb, arrItem, listName, innerNodeName, innerNodeNameValue);
                ProcessObjectArrayType(sb, arrItem, listName, innerNodeName, innerNodeNameValue);
            }

            if (children.Count > 0)
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

        string GetValueFunction(string dataType)
        {
            string getter = dataType switch
            {
                "string" => "GetStringValue()",
                "string?" => "GetStringValue()",
                "int" => "GetIntValue()",
                "int?" => "GetIntValue()",
                "bool" => "GetBoolValue()",
                "System.Drawing.Point" => "GetVectorValue()",
                "System.Drawing.Point?" => "GetVectorValue()",
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
                sb.AppendLineWithPreSpace($"        if ({nodeNameValue} == \"{node.Name}\")", node.Depth);
                sb.AppendLineWithPreSpace($"            {instanceName}.{node.Value.DataName} = {currentNode}.{GetValueFunction(node.Value.DataType)};", node.Depth);
            }
        }

        void ProcessArrayType(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            if (node.Value == null || node.Value.Type != 1)
                return;

            sb.AppendLineWithPreSpace($"                if (int.TryParse({currentNode}.Name, out var _))", node.Depth);
            sb.AppendLineWithPreSpace("                {", node.Depth);
            sb.AppendLineWithPreSpace($"                     {instanceName}.Add({currentNode}.{GetValueFunction(node.Value.DataType)});", node.Depth);
            sb.AppendLineWithPreSpace("                }", node.Depth);
        }

        void ProcessObjectArrayType(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            if (node.Value == null || node.Value.Type != 2)
                return;

            var modelName = $"modelArr{node.Depth}";
            var idxName = $"idx{node.Depth}";

            sb.AppendLineWithPreSpace($"                if (int.TryParse({nodeNameValue}, out var {idxName}))", node.Depth);
            sb.AppendLineWithPreSpace("                {", node.Depth);
            sb.AppendLineWithPreSpace($"                    var {modelName} = new {node.Value.DataType}();", node.Depth);

            var idx = node.Children.FirstOrDefault(x => x.Value != null && x.Value.Name == "$name");
            if (idx != null)
            {
                sb.AppendLineWithPreSpace($"                    {modelName}.{idx.Value.DataName} = {idxName};", node.Depth);
            }

            var len = node.Children.FirstOrDefault(x => x.Value != null && x.Value.Name == "$length");
            if (len != null)
            {
                sb.AppendLineWithPreSpace($"                    {modelName}.{len.Value.DataName} = {currentNode}.Children.Count();", node.Depth);
            }

            ProcessObject(sb, node, modelName, currentNode, nodeNameValue);
            sb.AppendLineWithPreSpace($"                     {instanceName}.Add({modelName});", node.Depth);
            sb.AppendLineWithPreSpace("                }", node.Depth);
        }

        void ProcessInnerObject(StringBuilder sb, TreeNode node, string instanceName, string currentNode, string nodeNameValue)
        {
            if (node.Value == null || node.Value.Type != 3)
                return;

            var modelName = $"modelInnerObj{node.Depth}";

            sb.AppendLineWithPreSpace($"        if ({nodeNameValue} == \"{node.Name}\")", node.Depth);
            sb.AppendLineWithPreSpace("        {", node.Depth);
            sb.AppendLineWithPreSpace($"            var {modelName} = new {node.Value.DataType.TrimEnd('?')}();", node.Depth);
            ProcessObject(sb, node, modelName, currentNode, nodeNameValue, true);
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
        /// 0. primitive 1. simple array 2. object array 3. object 4. special
        /// </summary>
        public PropertyData(int type, string name, string dataType, string dataName)
        {
            Type = type;
            Name = name;
            if (name.StartsWith("$"))
                Type = 4;

            DataType = dataType;
            DataName = dataName;
        }

        public int Type { get; set; }
        public string Name { get; set; }
        public string DataName { get; set; }
        public string DataType { get; set; }
    }
}
