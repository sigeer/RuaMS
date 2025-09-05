using System.Xml;

namespace Application.Templates.XmlWzReader
{
    internal class XmlReaderUtils
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
    }
}
