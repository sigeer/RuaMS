using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Module.Marriage.Channel.Models;
using Application.Module.Marriage.Channel.Net;
using Application.Module.Marriage.Common.Models;
using Application.Scripting.JS;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using MarriageProto;
using System.Collections.Concurrent;
using System.Numerics;
using ZstdSharp.Unsafe;

namespace Application.Module.Marriage.Channel
{
    public class MarriageManager
    {
        ConcurrentDictionary<int, MarriageInfo?> _dataSource = new();
        HashSet<int> inProposal = new();
        readonly IChannelServerTransport _transport;
        readonly IMapper _mapper;
        readonly WorldChannelServer _server;

        public MarriageManager(IChannelServerTransport transport, IMapper mapper, WorldChannelServer server)
        {
            _transport = transport;
            _mapper = mapper;
            _server = server;
        }

        public void SetInProposal(IPlayer chr)
        {
            inProposal.Add(chr.Id);
        }

        public void CancelProposal(IPlayer chr)
        {
            inProposal.Remove(chr.Id);
        }

        public void CompleteProposal(IPlayer chr)
        {
            inProposal.Remove(chr.Id);
        }

        public bool IsProposalActive(IPlayer from)
        {
            return inProposal.Contains(from.Id);
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

        public void RemoveMarriageItems(IPlayer chr)
        {
            chr.RemoveById(InventoryType.ETC, ItemId.GetEngagementItems(), false);

            var marriageRing = chr.getMarriageRing();
            if (marriageRing != null)
            {
                var it = chr.getInventory(InventoryType.EQUIP).findById(marriageRing.getItemId()) ?? chr.getInventory(InventoryType.EQUIPPED).findById(marriageRing.getItemId());
                if (it != null)
                {
                    Equip eqp = (Equip)it;
                    eqp.ResetRing();
                }
            }

        }

        /// <summary>
        /// 检测婚姻数据并修复
        /// </summary>
        /// <param name="chr"></param>
        public void CheckMarriageData(IPlayer chr)
        {
            var marriageInfo = GetPlayerMarriageInfo(chr.Id);
            if (marriageInfo == null)
            {
                // 未婚状态，移除订婚、结婚戒指，
                RemoveMarriageItems(chr);
            }
            else
            {
                // TODO: 已婚状态，没有订婚戒指、结婚戒指 补发？
                //var isHusband = marriageInfo.HusbandId == chr.Id;
                //if (marriageInfo.Status == MarriageStatusEnum.Engaged)
                //{
                //    var emptyBox = EngageItem.GetEngagementBoxId(marriageInfo.EngagementItemId);
                //    if (isHusband)
                //    {
                //        if (!chr.haveItem(emptyBox))
                //            chr.GainItem(emptyBox, 1, false, false);
                //    }
                //    else
                //    {
                //        var engageRingId = emptyBox + 1;
                //        if (!chr.haveItem(engageRingId))
                //            chr.GainItem(engageRingId, 1, false, false);
                //    }
                //}
                //if (marriageInfo.Status == MarriageStatusEnum.Married)
                //{
                //    var marriageRing = chr.getMarriageRing();
                //    if (marriageRing == null)
                //    {
                //        var ii = ItemInformationProvider.getInstance();
                //        var ringSource = new RingSourceModel()
                //        {
                //            Id = marriageInfo.RingSourceId,
                //            CharacterId1 = marriageInfo.HusbandId,
                //            CharacterName1 = marriageInfo.HusbandName,
                //            RingId1 = res.GroomRingId,
                //            CharacterId2 = res.BrideId,
                //            CharacterName2 = res.BrideName,
                //            RingId2 = res.BrideRingId,
                //            ItemId = marriageInfo.ItemId,
                //        };
                //        Item ringObj = ii.getEquipById(marriageInfo.ItemId);
                //        Equip ringEqp = (Equip)ringObj;
                //        ringEqp.SetRing(ringSource.RingId1, ringSource);
                //        chr.addMarriageRing(ringEqp.Ring);
                //        InventoryManipulator.addFromDrop(chr.Client, ringEqp, false);
                //    }
                //}
            }
        }
    }
}
