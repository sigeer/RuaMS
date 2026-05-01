namespace Application.Utility.Tickables
{
    public static class TickableManager
    {
        public static void ProcessSubTickables(this ITickableTree root, long now)
        {
            if (root.Status != TickableStatus.Active)
                return;

            var copyedTickables = root.SubTickables.ToArray();
            foreach (var item in copyedTickables)
            {
                item.OnTick(now);

                if (item.Status == TickableStatus.Remove)
                    root.SubTickables.Remove(item);
            }
        }

        public static bool IsAvailable(this ITickable a)
        {
            return a.Status != TickableStatus.Remove;
        }
    }
}
