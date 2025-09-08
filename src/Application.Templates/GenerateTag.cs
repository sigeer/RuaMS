using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Templates
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateTagAttribute: Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GenerateIgnorePropertyAttribute: Attribute
    {

    }
}
