using Application.Core.ServerTransports;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Application.Core.Channel.ServerData
{
    public class MonitorManager
    {
        readonly IChannelServerTransport _transport;
        readonly WorldChannelServer _server;
        readonly ILogger<MonitorManager> _logger;


        public MonitorManager(IChannelServerTransport transport, WorldChannelServer server, ILogger<MonitorManager> logger)
        {
            _transport = transport;
            _server = server;
            _logger = logger;
        }

        private static bool isRecvBlocked(RecvOpcode op)
        {
            return new RecvOpcode[] {
            RecvOpcode.MOVE_PLAYER ,
            RecvOpcode.GENERAL_CHAT ,
            RecvOpcode.TAKE_DAMAGE ,
            RecvOpcode.MOVE_PET ,
            RecvOpcode.MOVE_LIFE ,
            RecvOpcode.NPC_ACTION ,
            RecvOpcode.FACE_EXPRESSION
            }.Contains(op);
        }
        private Dictionary<int, string>? _monitoredChrIds;
        private readonly object _monitorLock = new object();

        public Dictionary<int, string> GetMonitor()
        {
            // 快路径：避免进入锁
            var snapshot = Volatile.Read(ref _monitoredChrIds);
            if (snapshot != null)
                return snapshot;

            // 慢路径：首次加载
            lock (_monitorLock)
            {
                if (_monitoredChrIds == null)
                {
                    _monitoredChrIds = LoadMonitorSet();
                }
                return _monitoredChrIds;
            }
        }

        public void ReloadMonitor()
        {
            var newSet = LoadMonitorSet();
            // 原子交换引用，确保其他线程读到的是完整的新对象
            Interlocked.Exchange(ref _monitoredChrIds, newSet);
        }

        private Dictionary<int, string> LoadMonitorSet()
        {
            return _transport.LoadMonitor().List.ToDictionary(x => x.Id, x => x.Name);
        }

        public void LogPacketIfMonitored(IChannelClient c, short packetId, byte[] packetContent)
        {
            var chr = c.Character;
            if (chr == null)
            {
                return;
            }

            if (!GetMonitor().ContainsKey(chr.getId()))
                return;

            RecvOpcode op = (RecvOpcode)packetId;
            if (isRecvBlocked(op))
            {
                return;
            }

            string packet = packetContent.Length > 0 ? HexTool.toHexString(packetContent) : "<empty>";
            _logger.LogInformation("{AccountId}.{CharacterName} {PacketId}-{Packet}", c.AccountEntity!.Id, chr.getName(), packetId, packet);
        }

        public async Task ToggleMonitor(Player chr, string name)
        {
            await _transport.SetMonitor(new Config.ToggleMonitorPlayerRequest { TargetName = name });
        }
    }
}
