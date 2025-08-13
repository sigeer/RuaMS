using Application.Core.ServerTransports;

namespace Application.Core.Channel.Services
{
    public class ExpeditionService
    {
        readonly IChannelServerTransport _transport;

        public ExpeditionService(IChannelServerTransport transport)
        {
            _transport = transport;
        }

        public void RegisterExpedition(int[] cids, int channel, string bossName)
        {
            var request = new ExpeditionProto.ExpeditionRegistry();
            request.CidList.AddRange(cids);
            request.Channel = channel;
            request.BossName = bossName;
            _transport.RegisterExpedition(request);
        }
        public bool CanStartExpedition(int cid, int channel, string bossName)
        {
            return _transport.CanStartExpedition(new ExpeditionProto.ExpeditionCheckRequest { Cid = cid, Channel = channel, BossName = bossName }).IsSuccess;
        }
    }
}
