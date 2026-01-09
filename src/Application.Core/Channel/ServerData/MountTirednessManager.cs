namespace Application.Core.Channel.ServerData
{
    public class MountTirednessManager : TaskBase
    {

        private Lock activeMountsLock = new ();
        private Dictionary<int, int> activeMounts = new();
        private DateTime mountUpdate;

        readonly WorldChannelServer _server;

        public MountTirednessManager(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(MountTirednessManager)}", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            mountUpdate = DateTime.UtcNow;
            this._server = server;
        }

        public void registerMountHunger(Player chr)
        {
            if (chr.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
            {
                return;
            }

            int key = chr.getId();
            activeMountsLock.Enter();
            try
            {
                int initProc;
                if (DateTime.UtcNow - mountUpdate > TimeSpan.FromSeconds(45))
                {
                    initProc = YamlConfig.config.server.MOUNT_EXHAUST_COUNT - 2;
                }
                else
                {
                    initProc = YamlConfig.config.server.MOUNT_EXHAUST_COUNT - 1;
                }

                activeMounts.AddOrUpdate(key, initProc);
            }
            finally
            {
                activeMountsLock.Exit();
            }
        }
        public void unregisterMountHunger(Player chr)
        {
            int key = chr.getId();

            activeMountsLock.Enter();
            try
            {
                activeMounts.Remove(key);
            }
            finally
            {
                activeMountsLock.Exit();
            }
        }

        protected override void HandleRun()
        {
            Dictionary<int, int> deployedMounts;
            activeMountsLock.Enter();
            try
            {
                mountUpdate = DateTime.UtcNow;
                deployedMounts = new(activeMounts);
            }
            finally
            {
                activeMountsLock.Exit();
            }

            foreach (var dp in deployedMounts)
            {
                var chr = _server.FindPlayerById(dp.Key);
                if (chr == null || !chr.isLoggedinWorld())
                {
                    continue;
                }

                int dpVal = dp.Value + 1;
                if (dpVal == YamlConfig.config.server.MOUNT_EXHAUST_COUNT)
                {
                    if (!chr.runTirednessSchedule())
                    {
                        continue;
                    }
                    dpVal = 0;
                }

                activeMountsLock.Enter();
                try
                {
                    activeMounts.AddOrUpdate(dp.Key, dpVal);
                }
                finally
                {
                    activeMountsLock.Exit();
                }
            }
        }
    }
}
