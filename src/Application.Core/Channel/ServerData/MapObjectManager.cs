namespace Application.Core.Channel.ServerData
{

    public class MapObjectManager : TaskBase
    {
        private Dictionary<Action, DateTime> registeredTimedMapObjects = new();
        private Lock timedMapObjectLock = new ();

        public MapObjectManager(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(MapObjectManager)}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
        }

        public void RegisterTimedMapObject(Action r, long duration)
        {
            timedMapObjectLock.Enter();
            try
            {
                registeredTimedMapObjects[r] = DateTime.UtcNow.AddMilliseconds(duration);
            }
            finally
            {
                timedMapObjectLock.Exit();
            }
        }

        protected override void HandleRun()
        {
            List<Action> toRemove = new();

            timedMapObjectLock.Enter();
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
                timedMapObjectLock.Exit();
            }

            foreach (Action r in toRemove)
            {
                r.Invoke();
            }
        }
    }
}
