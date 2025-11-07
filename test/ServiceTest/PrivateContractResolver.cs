using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace ServiceTest
{
    public class PrivateContractResolver : DefaultContractResolver
    {
        private readonly string[] _propsToIgnore;
        public PrivateContractResolver(params string[] propNames)
        {
            _propsToIgnore = propNames ?? Array.Empty<string>();
        }
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

            return props.Where(p => !_propsToIgnore.Any(x => $"<{x}>k__BackingField" == p.PropertyName)).ToList();
        }
    }
}
