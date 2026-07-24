using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Templates.Item
{
    public interface IEnhanceEquipItem
    {
        public int IncReqLevel { get; set; }
        public int IncSTR { get; set; }

        public int IncDEX { get; set; }

        public int IncINT { get; set; }

        public int IncLUK { get; set; }

        public int IncPAD { get; set; }

        /// <summary>
        /// 提升MaxHP
        /// </summary>
        public int IncMHP { get; set; }
        /// <summary>
        /// 提升MaxMP
        /// </summary>
        public int IncMMP { get; set; }

        public int IncMAD { get; set; }

        public int IncPDD { get; set; }

        public int IncMDD { get; set; }

        public int IncACC { get; set; }

        public int IncEVA { get; set; }

        public int IncSpeed { get; set; }

        public int IncJump { get; set; }
    }
}
