using System.Drawing;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader
{
    public static class XElementExtensions
    {
        public static string? GetName(this XElement element)
        {
            return element.Attribute("name")?.Value;
        }

        public static int GetIntName(this XElement element)
        {
            return Convert.ToInt32(element.GetName());
        }

        public static string? GetStringValue(this XElement element)
        {
            return element.Attribute("value")?.Value;
        }

        public static int GetIntValue(this XElement element)
        {
            return Convert.ToInt32(element.GetStringValue());
        }

        public static float GetFloatValue(this XElement element)
        {
            return Convert.ToSingle(element.GetStringValue());
        }

        public static bool GetBoolValue(this XElement element)
        {
            return Convert.ToInt32(element.GetStringValue()) > 0;
        }

        public static Point GetVectorValue(this XElement element)
        {
            return new Point(Convert.ToInt32(element.Attribute("x")?.Value), Convert.ToInt32(element.Attribute("y")?.Value));
        }
    }
}
