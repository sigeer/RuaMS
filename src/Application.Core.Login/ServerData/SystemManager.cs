using Application.Shared.Message;
using Config;
using System.Threading.Tasks;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task ToggleMonitor(ToggleMonitorPlayerRequest request)
        {
            var res = new ToggleMonitorPlayerResponse { Request = request };
            var chr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (chr == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeMonitor, res, [request.MasterId]);
                return;
            }

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeMonitor, res, [request.MasterId]);
                return;
            }


            if (_monitored.Contains(chr.Character.Id))
            {
                _monitored.Remove(chr.Character.Id);
                res.IsMonitored = false;
            }
            else
            {
                _monitored.Add(chr.Character.Id);
                res.IsMonitored = true;
            }

            await _server.Transport.SendMessageN(ChannelRecvCode.InvokeMonitor, res, [request.MasterId]);
            await _server.DropWorldMessage(5, master.Character.Name + (res.IsMonitored ? " has started monitoring " : " has stopped monitoring ") + request.TargetName + ".", true);
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
        public async Task ToggleAutoBanIgnored(ToggleAutoBanIgnoreRequest request)
        {
            var res = new ToggleAutoBanIgnoreResponse() {Request = request };
            var chr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (chr == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeAutoBanIgnore, res, [request.MasterId]);
                    return;
            }

            var master = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (master == null)
            {
                res.Code = 2;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeAutoBanIgnore, res, [request.MasterId]);
                return;
            }

            if (_autoBanIgnores.Contains(chr.Character.Id))
            {
                _autoBanIgnores.Remove(chr.Character.Id);
                res.IsIgnored = false;
            }
            else
            {
                _autoBanIgnores.Add(chr.Character.Id);
                res.IsIgnored = true;
            }

            await _server.Transport.SendMessageN(ChannelRecvCode.InvokeAutoBanIgnore, res, [request.MasterId, chr.Character.Id]);

            await _server.DropWorldMessage(5, master.Character.Name + (res.IsIgnored ? " has started ignoring " : " has stopped ignoring ") + request.TargetName + ".", true);
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
