using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Utility.Tickables
{
    public static class TickableManager
    {
        public static void ProcessSubTickables(this ITickableTree root, long now)
        {
            if (root.IsTickableCancelled)
                return;

            var copyedTickables = root.SubTickables.ToArray();
            foreach (var item in copyedTickables)
            {
                item.OnTick(now);

                if (item.IsTickableCancelled)
                    root.SubTickables.Remove(item);
            }
        }
    }
}
