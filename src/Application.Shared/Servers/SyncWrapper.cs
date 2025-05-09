using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Servers
{
    public class SyncWrapper<TData>
    {
        public SyncWrapper(TData data, SyncType type)
        {
            Data = data;
            Type = type;
        }

        public TData Data { get; set; }
        public SyncType Type { get; set; }
    }

    public enum SyncType
    {
        Update,
        Add,
        Remove
    }
}
