using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Utility.Tickables
{
    public static class TickableManager
    {
        public static void ProcessSubTickables(this ITickableTree root, long now)
        {
            if (root.IsTickableCancelled || root is ILifedTickable lifedTickable && lifedTickable.IsExpired)
                return;

            var copyedTickables = root.SubTickables.ToArray();
            foreach (var item in copyedTickables)
            {
                item.OnTick(now);

                if (item is ILifedTickable lifedTickable0 && lifedTickable0.IsExpired)
                    root.SubTickables.Remove(item);
            }
        }
    }
}
