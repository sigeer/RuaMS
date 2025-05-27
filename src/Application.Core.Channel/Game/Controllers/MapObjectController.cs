using Application.Core.Game.TheWorld;

namespace Application.Core.Game.Controllers
{

    public class MapObjectController : TimelyControllerBase
    {
        private Dictionary<Action, DateTime> registeredTimedMapObjects = new();
        private object timedMapObjectLock = new object();

        public MapObjectController(IWorldChannel channel) : base($"MapObjectController_{channel.InstanceId}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
        }

        public void RegisterTimedMapObject(Action r, long duration)
        {
            Monitor.Enter(timedMapObjectLock);
            try
            {
                registeredTimedMapObjects[r] = DateTime.UtcNow.AddMilliseconds(duration);
            }
            finally
            {
                Monitor.Exit(timedMapObjectLock);
            }
        }

        protected override void HandleRun()
        {
            List<Action> toRemove = new();

            Monitor.Enter(timedMapObjectLock);
            try
            {
                var timeNow = DateTime.UtcNow;

                foreach (var rtmo in registeredTimedMapObjects)
                {
                    if (rtmo.Value <= timeNow)
                    {
                        toRemove.Add(rtmo.Key);
                    }
                }

                foreach (Action r in toRemove)
                {
                    registeredTimedMapObjects.Remove(r);
                }
            }
            finally
            {
                Monitor.Exit(timedMapObjectLock);
            }

            foreach (Action r in toRemove)
            {
                r.Invoke();
            }
        }
    }
}
