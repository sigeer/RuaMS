using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Core.Login.Dtos.Gachapon
{
    public class GachaponRequestDto
    {
        public int Id { get; set; }
        /// <summary>
        /// -1：全局
        /// </summary>
        public int NpcId { get; set; }
    }

    public class GachaponItemRequestDto
    {
        public int Id { get; set; }
        public int PoolId { get; set; }
        public int ItemId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public int Level { get; set; }
    }
}
