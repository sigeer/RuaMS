using System.Reflection;

namespace Application.Utility.Extensions
{
    public static class ObjectExtensions
    {
        public static TModel DeepClone<TModel>(this TModel obj, params object[] contructParams) where TModel: class
        {
            var destination = Activator.CreateInstance(typeof(TModel), contructParams) as TModel;
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var value = field.GetValue(obj);
                field.SetValue(destination, value);
            }

            var props = obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var prop in props)
            {
                var value = prop.GetValue(obj);
                prop.SetValue(destination, value);
            }
            return destination!;
        }
    }
}
