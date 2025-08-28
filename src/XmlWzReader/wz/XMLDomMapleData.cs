/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System.Collections;
using System.Drawing;
using System.Xml;

namespace XmlWzReader.wz;


public class XMLDomMapleData : Data
{
    public XMLDomMapleData? Parent { get; set; }
    public DataType DataType { get; private set; }
    public string Name { get; private set; }
    public object? Value { get; private set; }
    List<Data> _children;
    public XMLDomMapleData(FileStream fis)
    {
        Parent = null;

        using (var reader = XmlReader.Create(fis, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true }))
        {
            reader.MoveToContent(); // 定位到第一个元素
            Name = reader.GetAttribute("name") ?? "";
            DataType = DataType.PROPERTY;

            _children = [];
            // 递归解析子元素
            if (!reader.IsEmptyElement)
            {
                using (var subTree = reader.ReadSubtree())
                {
                    subTree.Read(); // 进入根节点
                    while (subTree.Read())
                    {
                        if (subTree.NodeType == XmlNodeType.Element)
                        {
                            var child = new XMLDomMapleData(this, subTree);
                            _children.Add(child);
                        }
                    }
                }
            }
        }
    }


    public XMLDomMapleData(XMLDomMapleData parent, XmlReader reader)
    {
        Parent = parent;
        _children = new();

        Name = reader.GetAttribute("name") ?? "";
        DataType = getType(reader.Name);

        switch (DataType)
        {
            case DataType.DOUBLE:
                Value = Convert.ToDouble(reader.GetAttribute("value"));
                break;
            case DataType.FLOAT:
                Value = Convert.ToSingle(reader.GetAttribute("value"));
                break;
            case DataType.INT:
                Value = Convert.ToInt32(reader.GetAttribute("value"));
                break;
            case DataType.SHORT:
                Value = Convert.ToInt16(reader.GetAttribute("value"));
                break;
            case DataType.STRING:
            case DataType.UOL:
                Value = reader.GetAttribute("value");
                break;
            case DataType.VECTOR:
                string x = reader.GetAttribute("x") ?? "0";
                string y = reader.GetAttribute("y") ?? "0";
                Value = new Point(int.Parse(x), int.Parse(y));
                break;
            case DataType.CANVAS:
                string width = reader.GetAttribute("width") ?? "0";
                string height = reader.GetAttribute("height") ?? "0";
                Value = new Point(int.Parse(width), int.Parse(height));
                break;
        }

        // 递归处理子节点
        if (!reader.IsEmptyElement)
        {
            using (var subTree = reader.ReadSubtree())
            {
                subTree.Read(); // 进入当前节点
                while (subTree.Read())
                {
                    if (subTree.NodeType == XmlNodeType.Element)
                    {
                        var child = new XMLDomMapleData(this, subTree);
                        _children.Add(child);
                    }
                }
            }
        }
    }


    public Data? getChildByPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        // 处理 ../ 的情况
        if (path.StartsWith("../"))
        {
            if (Parent == null) return null;
            return Parent.getChildByPath(path.Substring(3));
        }

        // 分割路径
        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        XMLDomMapleData? current = this;

        foreach (var seg in segments)
        {
            // 在当前节点的 children 里找 name = seg 的节点
            current = current._children
                .FirstOrDefault(c => c.Name == seg) as XMLDomMapleData;

            if (current == null) return null; // 找不到就返回 null
        }

        return current;
    }


    public List<Data> getChildren()
    {
        return _children;
    }


    static DataType getType(string nodeName)
    {
        switch (nodeName)
        {
            case "imgdir":
                return DataType.PROPERTY;
            case "canvas":
                return DataType.CANVAS;
            case "convex":
                return DataType.CONVEX;
            case "sound":
                return DataType.SOUND;
            case "uol":
                return DataType.UOL;
            case "double":
                return DataType.DOUBLE;
            case "float":
                return DataType.FLOAT;
            case "int":
                return DataType.INT;
            case "short":
                return DataType.SHORT;
            case "string":
                return DataType.STRING;
            case "vector":
                return DataType.VECTOR;
            case "null":
                return DataType.IMG_0x00;
        }
        return DataType.NONE;

    }

    public string? getName()
    {
        return Name;
    }
    public IEnumerator<Data> GetEnumerator()
    {
        return new XMLDomDataEnumerator(_children);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public object? getData()
    {
        return Value;
    }
}


public class XMLDomDataEnumerator : IEnumerator<Data>
{
    int position = -1;
    List<Data> _items;
    public XMLDomDataEnumerator(List<Data> items)
    {
        _items = items;
    }

    public Data Current => _items[position];

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        // throw new NotImplementedException();
    }
    public bool MoveNext()
    {
        position++;
        return (position < _items.Count);
    }

    public void Reset()
    {
        position = -1;
    }
}