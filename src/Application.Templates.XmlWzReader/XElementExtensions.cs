using System.Collections.Concurrent;
using System.Drawing;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader
{
    public static class XElementExtensions
    {
        public static string? GetName(this XElement element)
        {
            return element.GetStringValue("name");
        }

        public static int GetIntName(this XElement element)
        {
            return element.GetIntValue("name");
        }

        public static string? GetStringValue(this XElement element, string name = "value")
        {
            return element.Attribute(name)?.Value;
        }

        public static int GetIntValue(this XElement element, string name = "value")
        {
            var v = element.GetStringValue(name) ?? "";
            if (int.TryParse(v, out int result))
                return result;

            return (int)double.Parse(v);
        }

        public static float GetFloatValue(this XElement element)
        {
            var v = element.GetStringValue();
            return v == null ? 0 : float.Parse(v);
        }

        public static double GetDoubleValue(this XElement element)
        {
            var v = element.GetStringValue();
            return v == null ? 0 : double.Parse(v.Replace(',', '.'));
        }

        public static bool GetBoolValue(this XElement element)
        {
            return element.GetIntValue() > 0;
        }

        public static Point GetVectorValue(this XElement element)
        {
            return new Point(element.GetIntValue("x"), element.GetIntValue("y"));
        }
    }
}
