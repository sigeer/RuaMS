namespace Application.Utility.Tickables
{
    public static class TickableManager
    {
        public static async Task ProcessSubTickables(this ITickableTree root, long now)
        {
            if (root.Status != TickableStatus.Active)
                return;

            var copyedTickables = root.SubTickables.ToArray();
            foreach (var item in copyedTickables)
            {
                await item.OnTick(now);

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
