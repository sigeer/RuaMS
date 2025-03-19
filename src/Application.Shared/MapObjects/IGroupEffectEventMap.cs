using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.MapObjects
{
    public interface IGroupEffectEventMap
    {
        public string EffectWin { get; set; }
        public string EffectLose { get; set; }

        string GetDefaultEffectWin();
        string GetDefaultEffectLose();
    }
}
