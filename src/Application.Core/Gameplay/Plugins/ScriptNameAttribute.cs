using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ScriptNameAttribute : Attribute
    {
        public ScriptNameAttribute(params string[] name)
        {
            Name = name;
        }

        public string[] Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ScriptTagAttribute : Attribute
    {
        public ScriptTagAttribute(string[] tags)
        {
            Tags = tags;
        }

        public string[] Tags { get; }
    }

    public class TypeUtils
    {
        public static Dictionary<string, MethodInfo> ExtractMethodsToDictionary(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var dict = new Dictionary<string, MethodInfo>();

            // 获取当前类声明的所有方法（包括公有、实例），不包含继承的方法
            var methods = type.GetMethods(
                BindingFlags.DeclaredOnly |
                BindingFlags.Public |
                BindingFlags.Instance
            );

            foreach (var method in methods)
            {
                // 获取方法上所有的 ScriptNameAttribute（不检查继承）
                var attributes = method.GetCustomAttribute<ScriptNameAttribute>(inherit: false);
                if (attributes != null)
                {
                    // 方法有特性：每个特性生成一个键值对
                    foreach (var attr in attributes.Name)
                    {
                        if (string.IsNullOrEmpty(attr))
                            throw new InvalidOperationException(
                                $"方法 '{method.Name}' 上的 ScriptNameAttribute 的 Name 属性不能为 null 或空。"
                            );

                        // 尝试添加，若键已存在则抛出异常
                        dict.Add(attr, method);
                    }
                }
                else
                {
                    dict.Add(method.Name, method);
                }
            }

            return dict;
        }
    }
}
