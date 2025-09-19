using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace ServiceTest
{
    public class PrivateContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // 获取所有字段（包含私有的）
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var props = fields
                .Select(f => base.CreateProperty(f, memberSerialization))
                .ToList();

            foreach (var p in props)
            {
                p.Readable = true;
                p.Writable = true;
            }

            return props;
        }
    }
}
