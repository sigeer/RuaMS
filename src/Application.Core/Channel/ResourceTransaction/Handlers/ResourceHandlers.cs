using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Channel.ResourceTransaction.Handlers
{
    public interface IResourceHandler
    {
        // 尝试冻结指定数量资源，返回是否成功
        bool TryFreeze();

        // 提交冻结资源（正式扣除）
        void Commit();

        // 回滚冻结资源（返还）
        void Rollback();
    }
}
