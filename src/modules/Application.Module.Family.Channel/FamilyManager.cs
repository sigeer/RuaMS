using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Module.Family.Channel.Models;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Net;
using AutoMapper;
using Dto;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using tools;

namespace Application.Module.Family.Channel
{
    public class FamilyManager
    {
        ConcurrentDictionary<int, Models.Family> _dataSource = new();
        ConcurrentDictionary<int, int> _playerFamilyId = new();

        readonly IMapper _mapper;
        readonly IChannelFamilyPluginTransport _transport;
        readonly WorldChannelServer _server;

        public FamilyManager(IMapper mapper, IChannelFamilyPluginTransport transport, WorldChannelServer server)
        {
            _mapper = mapper;
            _transport = transport;
            _server = server;
        }

        public Models.Family? GetFamily(int id)
        {
            if (id <= 0)
                return null;

            if (_dataSource.TryGetValue(id, out var model) && model != null)
                return model;

            model = _mapper.Map<Models.Family>( _transport.GetFamily(id));
            foreach (var member in model.Members)
            {
                _playerFamilyId[member.Key] = model.Id;
            }
            return model;
        }

        public Models.Family? GetFamilyByPlayerId(int id)
        {
            if (_playerFamilyId.TryGetValue(id, out var familyId))
            {
                return GetFamily(familyId);
            }
            return null;
        }

        public void AnnounceToSenior(FamilyEntry? entry, Packet packet, bool includeSuperSenior)
        {
            if (entry == null)
                return;

            var senior = entry.getSenior();
            if (senior != null)
            {
                var seniorChr = _server.FindPlayerById(senior.Channel, senior.Id);
                if (seniorChr != null)
                {
                    seniorChr.sendPacket(packet);
                }

                if (includeSuperSenior)
                {
                    senior = senior.getSenior();
                    AnnounceToSenior(senior, packet, false);
                }
            }
        }

        public void UpdateSeniorFamilyInfo(FamilyEntry? entry, bool includeSuperSenior)
        {
            if (entry == null)
                return;

            var senior = entry.getSenior();
            if (senior != null)
            {
                var seniorChr = _server.FindPlayerById(senior.Channel, senior.Id);
                if (seniorChr != null)
                {
                    seniorChr.sendPacket(FamilyPacketCreator.getFamilyInfo(senior));
                }

                if (includeSuperSenior)
                {
                    senior = senior.getSenior();
                    if (senior != null)
                    {
                        seniorChr = _server.FindPlayerById(senior.Channel, senior.Id);
                        seniorChr?.sendPacket(FamilyPacketCreator.getFamilyInfo(senior));
                    }
                }
            }
        }

        public void Fork(int masterId, int cost)
        {
            _transport.Fork(new Dto.CreateForkRequest { MasterId = masterId, Cost = cost });
        }

        //public void OnForked(Dto.CreateForkResponse data)
        //{
        //    if (data.Code == 0)
        //    {
        //        var newFamily = _mapper.Map<Models.Family>(data.NewFamily);
        //        _dataSource[newFamily.Id] = newFamily;

        //        var oldFamily = GetFamily(data.OldFamilyId)!;
        //        var rootMember = newFamily.getLeader();
                
        //        int repCost = separateRepCost(rootMember);
        //        var oldSenior = oldFamily.getEntryByID(data.OldSeniorId);
        //        oldSenior?.gainReputation(-repCost, false);

        //        var oldSeniorChr = _server.FindPlayerById(data.OldSeniorId);
        //        oldSeniorChr?.sendPacket(PacketCreator.serverNotice(5, rootMember.Name + " has left the family."));
        //        oldSeniorChr?.sendPacket(FamilyPacketCreator.getFamilyInfo(oldSenior));

        //        var oldSupperSenior = oldFamily.getEntryByID(data.OldSeniorId);
        //        oldSupperSenior?.gainReputation(-(repCost / 2), false);
        //        var oldSupperSeniorChr = _server.FindPlayerById(data.OldSupperSeniorId);
        //        oldSupperSeniorChr?.sendPacket(PacketCreator.serverNotice(5, rootMember.Name + " has left the family."));
        //        oldSupperSeniorChr?.sendPacket(FamilyPacketCreator.getFamilyInfo(oldSupperSenior));

        //        var chr = _server.FindPlayerById(data.Request.MasterId);
        //        if (chr != null)
        //        {
        //            chr.gainMeso(-data.Request.Cost, inChat: true);

        //            chr.sendPacket(FamilyPacketCreator.getFamilyInfo(rootMember)); //pedigree info will be requested from the client if the window is open

        //            chr.sendPacket(FamilyPacketCreator.sendFamilyMessage(1, 0));
        //        }

        //    }
        //}

        private static int separateRepCost(FamilyEntry junior)
        {
            int level = junior.Level;
            int ret = level / 20;
            ret += 10;
            ret *= level;
            ret *= 2;
            return ret;
        }

        internal async Task CreateInvite(Player chr, string toAdd)
        {
            await _server.Transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = chr.Id, ToName = toAdd, Type = Constants.InviteType_Family });
        }
        internal async Task AnswerInvite(Player chr, int familyId, bool accept)
        {
           await  _server.Transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { Type = Constants.InviteType_Family, CheckKey = familyId, Ok = accept, MasterId = chr.Id });
        }
        internal async Task CreateSummonInvite(Player chr, string toAdd)
        {
            await _server.Transport.SendInvitation(new InvitationProto.CreateInviteRequest { FromId = chr.Id, ToName = toAdd, Type = Constants.InviteType_FamilySummon });
        }
        internal async Task AnswerSummonInvite(Player chr, int familyId, bool accept)
        {
            await _server.Transport.AnswerInvitation(new InvitationProto.AnswerInviteRequest { Type = Constants.InviteType_FamilySummon, CheckKey = familyId, Ok = accept, MasterId = chr.Id });
        }

        public void OnJoinFamily(Dto.JoinFamilyResponse data)
        {
            if (data.Code != 0)
            {
                var inviter = _server.FindPlayerById(data.InviterChannel, data.InviterId);
                if (inviter != null)
                {
                    inviter.sendPacket(FamilyPacketCreator.sendFamilyMessage(data.Code, 0));
                }
            }
            else
            {
                var chrFamily = _mapper.Map<Models.Family>(data.Model);
                chrFamily.broadcast(FamilyPacketCreator.sendFamilyJoinResponse(true, data.NewMember.Name), data.NewMember.Id);
                var chrFamilyEntry = chrFamily.getEntryByID(data.NewMember.Id);
                var seniorEntry = chrFamily.getEntryByID(data.NewMember.SeniorId);
                UpdateSeniorFamilyInfo(chrFamilyEntry, true);

                var receiver = _server.FindPlayerById(data.NewMember.Channel, data.NewMember.Id);
                if (receiver != null)
                {
                    receiver.sendPacket(FamilyPacketCreator.getSeniorMessage(seniorEntry.Name));
                    receiver.sendPacket(FamilyPacketCreator.getFamilyInfo(chrFamilyEntry));
                }

            }
        }

        public void UseEntitlement(Player player, FamilyEntitlement entitlement)
        {
            _transport.UseEntitlement(new UseEntitlementRequest { MatserId = player.Id, EntitlementId = entitlement.ordinal() });
        }

        //public void OnUseEntitlement(UseEntitlementResponse data)
        //{
        //    var entitlement = FamilyEntitlement.Parse(data.Request.EntitlementId);

        //    var family = GetFamily(data.FamilyId)!;

        //    var newEnty = _mapper.Map<FamilyEntry>(data.UpdatedMember);
        //    family.UpdateMember(newEnty);
        //    var chr = _server.FindPlayerById(data.Request.MatserId);
        //    chr?.sendPacket(FamilyPacketCreator.getFamilyInfo(newEnty));
        //}


    }
}
