using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Models
{
    public class CallbackModel
    {
        public string CallbackName { get; set; }
        public CallbackParamModel[] Params { get; set; }
    }

    public class CallbackParamModel
    {
        public int Index { get; set; }
        public string Schema { get; set; }
        public string Value { get; set; }
    }
}
