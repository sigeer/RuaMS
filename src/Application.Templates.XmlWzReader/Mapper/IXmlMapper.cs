using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Mapper
{
    public interface IXmlMapper
    {
        string? CurrentNode { get; set; }
        void Map(object obj, XElement objNode);
    }
}
