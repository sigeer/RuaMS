using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Scripting
{
    public abstract class ScriptResultWrapper
    {
        public abstract TObject ToObject<TObject>();
        public abstract object? ToObject();
    }
}
