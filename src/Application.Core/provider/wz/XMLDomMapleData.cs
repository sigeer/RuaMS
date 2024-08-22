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
using System.Collections.Concurrent;
using System.Xml;

namespace provider.wz;


public class XMLDomMapleData : Data
{
    private XmlNode node;

    public XMLDomMapleData(FileStream fis)
    {
        XmlDocument document = new XmlDocument();
        document.Load(fis);
        this.node = document.DocumentElement!;
    }

    private XMLDomMapleData(XmlNode node)
    {
        this.node = node;
    }

    ConcurrentDictionary<string, Data?> dataCache = new ConcurrentDictionary<string, Data?>();
    object getChildLock = new object();
    public Data? getChildByPath(string path)
    {  // the whole XML reading system seems susceptible to give nulls on strenuous read scenarios
        lock (getChildLock)
        {
            string[] segments = path.Split("/");
            if (segments[0].Equals(".."))
            {
                return ((Data)getParent()).getChildByPath(path.Substring(path.IndexOf("/") + 1));
            }

            if (segments.Count() == 1)
            {
                if (dataCache.ContainsKey(segments[0]))
                    return dataCache[segments[0]];

                XmlNodeList childNodes = node.ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    var childNode = childNodes.Item(i);
                    if (childNode != null && childNode.NodeType == XmlNodeType.Element)
                    {
                        var nodeName = childNode.Attributes!.GetNamedItem("name")!.Value!;
                        if (!dataCache.ContainsKey(nodeName))
                        {
                            XMLDomMapleData ret = new XMLDomMapleData(childNode);
                            dataCache.TryAdd(nodeName, ret);
                        }
                    }
                }

                if (dataCache.ContainsKey(segments[0]))
                    return dataCache[segments[0]];
                else
                {
                    dataCache.TryAdd(segments[0], null);
                    return null;
                }
            }
            else
            {
                return getChildByPath(segments[0])?.getChildByPath(string.Join('/', segments.Skip(1)));
            }
        }
    }

    public List<Data> getChildren()
    {
        lock (getChildLock)
        {
            List<Data> ret = new();

            XmlNodeList childNodes = node.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode? childNode = childNodes.Item(i);
                if (childNode != null && childNode.NodeType == XmlNodeType.Element)
                {
                    XMLDomMapleData child = new XMLDomMapleData(childNode);
                    ret.Add(child);
                }
            }

            return ret;
        }
    }

    object getDataLock = new object();
    public object? getData()
    {
        lock (getDataLock)
        {
            var attributes = node.Attributes!;
            var type = getType();
            switch (type)
            {
                case DataType.DOUBLE:
                case DataType.FLOAT:
                case DataType.INT:
                case DataType.SHORT:
                    {
                        string value = attributes.GetNamedItem("value")?.Value ?? "0";
                        switch (type)
                        {
                            case DataType.DOUBLE:
                                return double.Parse(value);
                            case DataType.FLOAT:
                                return float.Parse(value);
                            case DataType.INT:
                                return int.Parse(value);
                            case DataType.SHORT:
                                return short.Parse(value);
                            default:
                                return 0;
                        }
                    }
                case DataType.STRING:
                case DataType.UOL:
                    {
                        string? value = attributes.GetNamedItem("value")?.Value;
                        return value;
                    }
                case DataType.VECTOR:
                    {
                        string x = attributes.GetNamedItem("x")?.Value ?? "0";
                        string y = attributes.GetNamedItem("y")?.Value ?? "0";
                        return new Point(int.Parse(x), int.Parse(y));
                    }
                default:
                    return null;
            }
        }
    }

    public DataType? getType()
    {
        lock (getDataLock)
        {
            string nodeName = node.Name;

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
            return null;
        }
    }

    object getParentLock = new object();
    public DataEntity getParent()
    {
        lock (getParentLock)
        {
            var parentNode = node.ParentNode;
            if (parentNode == null || parentNode.NodeType == XmlNodeType.Document)
            {
                return null!;
            }
            return new XMLDomMapleData(parentNode);
        }
    }

    object getNameLock = new object();
    public string? getName()
    {
        lock (getNameLock)
        {
            return node.Attributes?.GetNamedItem("name")?.Value;
        }
    }

    public IEnumerator<Data> GetEnumerator()
    {
        return new XMLDomDataEnumerator(getChildren());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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