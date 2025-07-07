using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.MTS.Master.Models
{
    /// <summary>
    /// 购物车
    /// </summary>
    public class MTSCartModel
    {
        public int PlayerId { get; set; }
        public List<int> Products { get; set; } = [];
    }
}
