using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader
{
    public static class XmlReaderUtils
    {

        public static void ReadChildNode(XmlReader reader, Action<XmlReader> action)
        {
            reader.MoveToContent();
            using (var subTree = reader.ReadSubtree())
            {
                subTree.Read(); // 进入根节点
                while (subTree.Read())
                {
                    if (subTree.NodeType == XmlNodeType.Element)
                    {
                        action(subTree);
                    }
                }
            }
        }

        public static void ReadChildNodeValue(XmlReader reader, Action<string?, string?> valueFunc)
        {
            if (!reader.IsEmptyElement)
            {
                using (var subTree = reader.ReadSubtree())
                {
                    subTree.Read(); // 进入根节点
                    while (subTree.Read())
                    {
                        if (subTree.NodeType == XmlNodeType.Element)
                        {
                            valueFunc(subTree.GetAttribute("name"), subTree.GetAttribute("value"));
                        }
                    }
                }
            }
        }

        public static string? GetStringValue(this XmlReader reader)
        {
            return reader.GetAttribute("value");
        }

        public static int GetIntData(this XmlReader reader, string name = "value")
        {
            var v = reader.GetAttribute(name) ?? "";
            if (int.TryParse(v, out int result))
                return result;

            return (int)double.Parse(v);
        }


        public static Point GetVectorData(this XmlReader reader)
        {
            return new Point(reader.GetIntData("x"), reader.GetIntData("y"));
        }
    }
}
