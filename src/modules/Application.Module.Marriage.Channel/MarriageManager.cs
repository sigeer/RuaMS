using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Module.Marriage.Channel.Models;
using Application.Module.Marriage.Channel.Net;
using AutoMapper;
using MarriageProto;
using System.Collections.Concurrent;

namespace Application.Module.Marriage.Channel
{
    public class MarriageManager
    {
        ConcurrentDictionary<int, MarriageInfo?> _dataSource = new();

        readonly IChannelServerTransport _transport;
        readonly IMapper _mapper;
        readonly WorldChannelServer _server;

        public MarriageManager(IChannelServerTransport transport, IMapper mapper, WorldChannelServer server)
        {
            _transport = transport;
            _mapper = mapper;
            _server = server;
        }

        public MarriageInfo? GetPlayerMarriageInfo(int chrId)
        {
            if (_dataSource.TryGetValue(chrId, out var d))
                return d;

            var res = _transport.LoadMarriageInfo(new MarriageProto.LoadMarriageInfoRequest { MasterId = chrId });
            d = _mapper.Map<MarriageInfo>(res.Data);
            SetPlayerMarriageInfo(d);
            return d;
        }

        public void SetPlayerMarriageInfo(MarriageInfo d)
        {
            _dataSource[d.HusbandId] = d;
            _dataSource[d.WifeId] = d;
        }

        public int GetPartnerId(int chrId)
        {
            return GetPlayerMarriageInfo(chrId)?.GetPartnerId(chrId) ?? 0;
        }

        public void RemoveLocalData(MarriageInfo data)
        {
            RemoveLocalData(data.HusbandId, data.WifeId);
        }

        public void RemoveLocalData(int husbandId, int wifeId)
        {
            _dataSource.TryRemove(husbandId, out _);
            _dataSource.TryRemove(wifeId, out _);
        }

        internal void NotifyPartnerWhenTransfer(PlayerTransferDto dto)
        {
            var chr = _server.FindPlayerById(dto.ToPlayerId);
            if (chr != null)
            {
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(dto.PlayerId, dto.MapId));
            }
        }

        public void SendSpouseChat(IPlayer chr, string text)
        {
            var res = _transport.SendSpouseChat(new MarriageProto.SendSpouseChatRequest { SenderId = chr.Id, Text = text });
            if (res.Code == 1)
            {
                chr.dropMessage(5, "You don't have a spouse.");
            }

            if(res.Code == 2)
            {
                chr.dropMessage(5, "Your spouse is currently offline.");
            }
        }

        public void OnReceivedSpouseChat(MarriageProto.OnSpouseChatCallback data)
        {
            var sender = _server.FindPlayerById(data.SenderId);
            if (sender != null)
            {
                sender.sendPacket(WeddingPackets.OnCoupleMessage(data.SenderName, data.Text, true));
            }

            var receiver = _server.FindPlayerById(data.SenderPartnerId);
            if (receiver != null)
            {
                receiver.sendPacket(WeddingPackets.OnCoupleMessage(data.SenderName, data.Text, true));
            }
        }
    }
}
