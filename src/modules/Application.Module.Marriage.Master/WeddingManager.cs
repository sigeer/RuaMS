using Application.Core.Login;
using Application.Module.Marriage.Common.ErrorCodes;
using Application.Module.Marriage.Master.Models;
using AutoMapper;
using MarriageProto;
using System.Collections.Concurrent;

namespace Application.Module.Marriage.Master
{
    public class WeddingManager
    {
        /// <summary>
        /// 所有登记的婚礼（婚礼完成后移除）
        /// </summary>
        private ConcurrentDictionary<int, WeddingInfo> registeredWedding = new();

        readonly MasterTransport _transport;
        readonly MasterServer _server;
        readonly IMapper _mapper;
        readonly MarriageManager _marriageManager;

        int _localMarriageId = 0;


        public void ReserveWedding(MarriageProto.ReserveWeddingRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null)
            {
                return;
            }

            if (registeredWedding.ContainsKey(chr.Character.EffectMarriageId))
            {
                return;
            }

            var weddingInfo = new WeddingInfo(chr.Character.EffectMarriageId, request.Channel, request.IsCathedral, request.IsPremium, request.MasterId, chr.Character.PartnerId, [], request.StartTime);
            registeredWedding[chr.Character.EffectMarriageId] = weddingInfo;

            _transport.BroadcastWedding(new BroadcastWeddingDto
            {
                IsCathedral = weddingInfo.IsCathedral,
                BrideName = _server.CharacterManager.GetPlayerName(weddingInfo.BrideId),
                GroomName = _server.CharacterManager.GetPlayerName(weddingInfo.GroomId),
                Channel = weddingInfo.Channel
            });
        }

        public ItemProto.RingDto CompleteWedding(CompleteWeddingRequest request)
        {
            if (registeredWedding.TryGetValue(request.MarriageId, out var wedding))
            {
                _marriageManager.CompleteMarriage(wedding.Id);
                var ring = _server.RingManager.CreateRing(request.MarriageItemId, wedding.GroomId, wedding.BrideId, wedding.Id);
                return _mapper.Map<ItemProto.RingDto>(ring);
            }
            return new ItemProto.RingDto();
        }

        public void InviteGuest(MarriageProto.InviteGuestRequest request)
        {
            var guestChr = _server.CharacterManager.FindPlayerByName(request.GuestName);
            if (guestChr == null)
            {
                _transport.ReturnGuestInvitation(new MarriageProto.InviteGuestResponse { Code = (int)InviteErrorCode.GuestNotFound, Request = request });
                return;
            }

            if (!registeredWedding.TryGetValue(request.MarriageId, out var wedding))
            {
                _transport.ReturnGuestInvitation(new MarriageProto.InviteGuestResponse { Code = (int)InviteErrorCode.MarriageNotFound, Request = request });
                return;
            }

            if (wedding.Guests.Contains(guestChr.Character.Id))
            {
                _transport.ReturnGuestInvitation(new MarriageProto.InviteGuestResponse { Code = (int)InviteErrorCode.DuplicateInvitation, Request = request });
                return;
            }

            if (_server.getCurrentTime() >= wedding.StartTime)
            {
                _transport.ReturnGuestInvitation(new MarriageProto.InviteGuestResponse { Code = (int)InviteErrorCode.WeddingUnderway, Request = request });
                return;
            }

            var res = new MarriageProto.InviteGuestResponse { Code = (int)InviteErrorCode.Success, Request = request };
            wedding.Guests.Add(guestChr.Character.Id);
            if (guestChr.Channel <= 0)
            {
                // duey发送
            }
            _transport.ReturnGuestInvitation(res);
        }

        public MarriageProto.LoadInvitationResponse GetInvitation(MarriageProto.LoadInvitationRequest request)
        {
            if (registeredWedding.TryGetValue(request.WeddingId, out var wedding))
            {
                return new MarriageProto.LoadInvitationResponse
                {
                    MarriageId = wedding.Id,
                    GroomName = _server.CharacterManager.GetPlayerName(wedding.GroomId),
                    BrideName = _server.CharacterManager.GetPlayerName(wedding.BrideId)
                };
            }

            return new MarriageProto.LoadInvitationResponse();
        }
    }
}
