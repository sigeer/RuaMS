using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Mapper
{
    [Obsolete]
    public class XmlMapper<T> where T : class
    {
        private readonly Dictionary<string, Action<T, XElement>> _map
            = new(StringComparer.OrdinalIgnoreCase);

        public XmlMapper<T> CreateMap(string elementName, string propertyName)
        {
            var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) throw new ArgumentException($"Property {propertyName} not found on {typeof(T)}");

            _map[elementName] = (obj, e) =>
            {
                object? value;
                if (prop.PropertyType == typeof(int))
                    value = e.GetIntValue();
                else if (prop.PropertyType == typeof(bool))
                    value = e.GetBoolValue();
                else if (prop.PropertyType == typeof(double))
                    value = e.GetDoubleValue();
                else if (prop.PropertyType == typeof(float))
                    value = e.GetFloatValue();
                else if (prop.PropertyType == typeof(Point))
                    value = e.GetVectorValue();
                else
                    value = e.GetStringValue();

                prop.SetValue(obj, value);
            };

            return this;
        }

        public void Map(T obj, XElement element)
        {
            foreach (var child in element.Elements())
            {
                var propName = child.GetName();
                if (!string.IsNullOrEmpty(propName) && _map.TryGetValue(propName, out var setter))
                    setter(obj, child);
            }
        }
    }
}
