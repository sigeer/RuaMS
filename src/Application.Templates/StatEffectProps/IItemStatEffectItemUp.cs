using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Templates.StatEffectProps
{
    public interface IItemStatEffectItemUp : IItemStatEffectProp
    {
        /// <summary>
        /// 1. 所有，2. 依赖ItemCode，3. 依赖ItemRange
        /// </summary>
        public int ItemUp { get; }
        public int ItemCode { get; }
        public int ItemRange { get; }
        public int Prob { get; }
    }
}
