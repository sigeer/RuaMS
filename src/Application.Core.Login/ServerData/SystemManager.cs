using Config;

namespace Application.Core.Login.ServerData
{
    public class SystemManager
    {
        HashSet<int> _monitored = new();
        readonly MasterServer _server;

        public SystemManager(MasterServer server)
        {
            _server = server;
        }

        public ToggleMonitorPlayerResponse ToggleMonitor(ToggleMonitorPlayerRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (chr == null)
                return new ToggleMonitorPlayerResponse { IsSuccess = false };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
                return new ToggleMonitorPlayerResponse();

            bool isMonitored = false;
            if (_monitored.Contains(chr.Character.Id))
            {
                _monitored.Remove(chr.Character.Id);
                isMonitored = false;
            }
            else
            {
                _monitored.Add(chr.Character.Id);
                isMonitored = true;
            }

            _server.Transport.MonitorChangedNotify(new MonitorDataChangedNotifyDto { OperatorName = master.Character.Name, TargetName = request.TargetName, IsMonitored = isMonitored });
            return new ToggleMonitorPlayerResponse { IsMonitored = isMonitored, IsSuccess = true };
        }

        public MonitorDataWrapper LoadMonitorData()
        {
            var data = new MonitorDataWrapper();
            data.List.AddRange(_monitored.Select(x => new Config.PlayerBaseDto()
            {
                Id = x,
                Name = _server.CharacterManager.GetPlayerName(x)
            }));
            return data;
        }


        HashSet<int> _autoBanIgnores = new();
        public ToggleAutoBanIgnoreResponse ToggleAutoBanIgnored(ToggleAutoBanIgnoreRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (chr == null)
                return new ToggleAutoBanIgnoreResponse { IsSuccess = false };

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
                return new ToggleAutoBanIgnoreResponse();

            bool isIgnore = false;
            if (_autoBanIgnores.Contains(chr.Character.Id))
            {
                _autoBanIgnores.Remove(chr.Character.Id);
                isIgnore = false;
            }
            else
            {
                _autoBanIgnores.Add(chr.Character.Id);
                isIgnore = true;
            }

            _server.Transport.AutobanIgnoresChangedNotify(new AutoBanIgnoredChangedNotifyDto { OperatorName = master.Character.Name, TargetName = request.TargetName, IsIgnored = isIgnore });
            return new ToggleAutoBanIgnoreResponse { IsIgnored = isIgnore, IsSuccess = true };
        }

        public AutoBanIgnoredWrapper LoadAutobanIgnoreData()
        {
            var data = new AutoBanIgnoredWrapper();
            data.List.AddRange(_autoBanIgnores.Select(x => new Config.PlayerBaseDto()
            {
                Id = x,
                Name = _server.CharacterManager.GetPlayerName(x)
            }));
            return data;
        }
    }
}
