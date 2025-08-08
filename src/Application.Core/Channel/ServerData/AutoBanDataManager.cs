using Application.Core.Managers;
using Application.Core.ServerTransports;
using client.autoban;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.ServerData
{
    public class AutoBanDataManager
    {
        private Dictionary<int, string>? _ignoredData;
        private readonly object _dataLock = new object();

        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;
        readonly ILogger<AutoBanDataManager> _logger;

        public AutoBanDataManager(IChannelServerTransport transport, WorldChannelServer server, ILogger<AutoBanDataManager> logger)
        {
            _transport = transport;
            _server = server;
            _logger = logger;
        }

        public Dictionary<int, string> GetAutobanIngores()
        {
            // 快路径：避免进入锁
            var snapshot = Volatile.Read(ref _ignoredData);
            if (snapshot != null)
                return snapshot;

            // 慢路径：首次加载
            lock (_dataLock)
            {
                if (_ignoredData == null)
                {
                    _ignoredData = GetDataFromRemote();
                }
                return _ignoredData;
            }
        }

        public void ReloadData()
        {
            var newSet = GetDataFromRemote();
            // 原子交换引用，确保其他线程读到的是完整的新对象
            Interlocked.Exchange(ref _ignoredData, newSet);
        }

        private Dictionary<int, string> GetDataFromRemote()
        {
            return _transport.LoadAutobanIgnoreData().List.ToDictionary(x => x.Id, x => x.Name);
        }


        public void ToggleIgnore(IPlayer chr, string name)
        {
            Config.ToggleAutoBanIgnoreResponse res = _transport.SetAutoBanIgnored(new Config.ToggleAutoBanIgnoreRequest { TargetName = name });
            if (res.IsSuccess)
            {
                chr.yellowMessage(name + " is " + (res.IsIgnored ? "now being ignored." : "no longer being ignored."));
            }
            else
            {
                chr.dropMessage($"未找到玩家：{name}");
            }
        }

        public void OnIgoreDataChanged(Config.AutoBanIgnoredChangedNotifyDto data)
        {
            foreach (var gmId in data.GmId)
            {
                var gmChr = _server.FindPlayerById(gmId);
                if (gmChr != null)
                {
                    gmChr.dropMessage(5, data.OperatorName + (data.IsIgnored ? " has started ignoring " : " has stopped ignoring ") + data.TargetName + ".");
                }
            }
        }

        public void AddPoint(AutobanFactory type, IPlayer chr, string reason)
        {
            chr.getAutobanManager().addPoint(type, reason);
        }


        public void Autoban(AutobanFactory type, IPlayer chr, string value)
        {
            if (YamlConfig.config.server.USE_AUTOBAN)
            {
                chr.autoban("Autobanned for (" + type.name() + " : " + value + ")");
                //chr.sendPolice("You will be disconnected for(" + this.name() + " : " + value + ")");
            }
        }

        public void Alert(AutobanFactory type, IPlayer chr, string reason)
        {
            if (YamlConfig.config.server.USE_AUTOBAN)
            {
                if (chr == null || GetAutobanIngores().ContainsKey(chr.getId()))
                {
                    return;
                }
                chr.Client.CurrentServerContainer.SendBroadcastWorldGMPacket(
                    PacketCreator.sendYellowTip((chr != null ? CharacterManager.makeMapleReadable(chr.getName()) : "") + " caused " + type.name() + " " + reason));
            }
            if (YamlConfig.config.server.USE_AUTOBAN_LOG)
            {
                string chrName = chr != null ? CharacterManager.makeMapleReadable(chr.getName()) : "";
                _logger.LogInformation("Autoban alert - chr {CharacterName} caused {AutoBanType}-{AutoBanReason}", chrName, type.name(), reason);
            }
        }
    }
}
