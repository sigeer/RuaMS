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
using System.Xml.Linq;

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
        _children = [];
        var rootNode = XDocument.Load(fis).Root!;

        Name = rootNode.Attribute("name")!.Value;
        var children = rootNode.Elements();
        foreach (var child in children)
        {
            _children.Add(new XMLDomMapleData(this, child));
        }
    }


    public XMLDomMapleData(XMLDomMapleData parent, XElement node)
    {
        Parent = parent;
        DataType = getType(node);
        Name = node.Attribute("name")!.Value;

        switch (DataType)
        {
            case DataType.DOUBLE:
                Value = Convert.ToDouble(node.Attribute("value")?.Value);
                break;
            case DataType.FLOAT:
                Value = Convert.ToSingle(node.Attribute("value")?.Value);
                break;
            case DataType.INT:
                Value = Convert.ToInt32(node.Attribute("value")?.Value);
                break;
            case DataType.SHORT:
                Value = Convert.ToInt16(node.Attribute("value")?.Value);
                break;
            case DataType.STRING:
            case DataType.UOL:
                Value = node.Attribute("value")?.Value;
                break;
            case DataType.VECTOR:
                string x = node.Attribute("x")?.Value ?? "0";
                string y = node.Attribute("y")?.Value ?? "0";
                Value = new Point(int.Parse(x), int.Parse(y));
                break;
            default:
                break;
        }

        _children = [];
        var children = node.Elements();
        foreach (var child in children)
        {
            _children.Add(new XMLDomMapleData(this, child));
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


    public static DataType getType(XElement node)
    {
        string nodeName = node.Name.LocalName;

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
        throw new NotSupportedException();

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