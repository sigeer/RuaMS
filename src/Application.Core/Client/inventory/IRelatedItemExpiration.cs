using client.inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Client.inventory
{
    public interface IRelatedItemExpiration
    {
        Item SourceItem { get; }
        /// <summary>
        /// 过期触发
        /// </summary>
        void ExpiredInvoke();
    }
}
