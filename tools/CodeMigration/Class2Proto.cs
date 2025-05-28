using System.Reflection;
using System.Text;

namespace CodeMigration
{
    internal class Class2Proto
    {
        public static void Run(string assemblyPath)
        {
            var dtoTypes = Assembly.LoadFile(assemblyPath)
            .GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Dto"))
            .ToList();

            foreach (var type in dtoTypes)
            {
                var proto = GenerateProto(type);
                var filePath = Path.Combine("Protos", $"{type.Name.Replace("Dto", "")}.proto");
                Directory.CreateDirectory("Protos");
                File.WriteAllText(filePath, proto);
                Console.WriteLine($" <Protobuf Include=\"Dto\\{type.Name.Replace("Dto", "")}.proto\" GrpcServices=\"None\" />");
            }
        }

        static string GenerateProto(Type type)
        {
            var sb = new StringBuilder();
            sb.AppendLine("syntax = \"proto3\";");
            sb.AppendLine();
            sb.AppendLine("""import "google/protobuf/timestamp.proto";""");
            sb.AppendLine("""import "google/protobuf/wrappers.proto";""");
            sb.AppendLine($"package Dto;");
            sb.AppendLine();
            sb.AppendLine($"message {type.Name} {{");

            var properties = type.GetProperties();
            int fieldNumber = 1;

            foreach (var prop in properties)
            {
                var (protoType, needsComment) = MapToProtoType(prop.PropertyType);
                var comment = needsComment ? " // CHECK TYPE" : "";
                sb.AppendLine($"  {protoType} {CamelCase(prop.Name)} = {fieldNumber++};{comment}");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        static (string, bool) MapToProtoType(Type type)
        {
            bool nullable = Nullable.GetUnderlyingType(type) != null;
            Type baseType = Nullable.GetUnderlyingType(type) ?? type;

            if (baseType == typeof(int) || baseType == typeof(sbyte) || baseType == typeof(byte) || baseType == typeof(short))
                return nullable ? ("google.protobuf.Int32Value", false) : ("int32", false);
            if (baseType == typeof(long))
                return nullable ? ("google.protobuf.Int64Value", false) : ("int64", false);
            if (baseType == typeof(bool))
                return nullable ? ("google.protobuf.BoolValue", false) : ("bool", false);
            if (baseType == typeof(string))
                return ("string", false);
            if (baseType == typeof(int[]))
                return ("repeated int32", false);
            if (baseType == typeof(string[]))
                return ("repeated string", false);
            if (baseType == typeof(DateTime) || baseType == typeof(DateTimeOffset))
                return ("google.protobuf.Timestamp", false); // or "google.protobuf.Timestamp"

            return ("string", true); // fallback
        }

        static string CamelCase(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
