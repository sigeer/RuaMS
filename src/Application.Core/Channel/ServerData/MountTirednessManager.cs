using Application.Core.Channel.Commands;

namespace Application.Core.Channel.ServerData
{
    public class MountTirednessManager
    {
        private Dictionary<int, int> activeMounts = new();
        private DateTime mountUpdate;

        readonly WorldChannel _server;

        public MountTirednessManager(WorldChannel server)
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
        public void unregisterMountHunger(Player chr)
        {
            int key = chr.getId();

            activeMounts.Remove(key);
        }

        public void HandleRun()
        {
            Dictionary<int, int> deployedMounts;
            mountUpdate = DateTime.UtcNow;
            deployedMounts = new(activeMounts);

            foreach (var dp in deployedMounts)
            {

                var chr = _server.getPlayerStorage().getCharacterById(dp.Key);
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

                activeMounts.AddOrUpdate(dp.Key, dpVal);
            }
        }
    }
}
