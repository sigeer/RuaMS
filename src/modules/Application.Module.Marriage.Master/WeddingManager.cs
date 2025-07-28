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

        public WeddingManager(MasterTransport transport, MasterServer server, IMapper mapper, MarriageManager marriageManager)
        {
            _transport = transport;
            _server = server;
            _mapper = mapper;
            _marriageManager = marriageManager;
        }

        public MarriageProto.ReserveWeddingResponse ReserveWedding(MarriageProto.ReserveWeddingRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null)
            {
                return new ReserveWeddingResponse() { Code = 2 };
            }

            if (registeredWedding.ContainsKey(chr.Character.EffectMarriageId))
            {
                return new ReserveWeddingResponse() { Code = (int)ReserveErrorCode.AlreadyReserved };
            }

            var weddingInfo = new WeddingInfo(chr.Character.EffectMarriageId, request.Channel, request.IsCathedral, request.IsPremium, request.MasterId, chr.Character.PartnerId, [], request.StartTime + (long)TimeSpan.FromMinutes(30).TotalMilliseconds);
            registeredWedding[chr.Character.EffectMarriageId] = weddingInfo;

            _transport.BroadcastWedding(new BroadcastWeddingDto
            {
                IsCathedral = weddingInfo.IsCathedral,
                BrideName = _server.CharacterManager.GetPlayerName(weddingInfo.BrideId),
                GroomName = _server.CharacterManager.GetPlayerName(weddingInfo.GroomId),
                Channel = weddingInfo.Channel
            });

            return new ReserveWeddingResponse() { StartTime = weddingInfo.StartTime };
        }

        public void CloseWedding(CloseWeddingRequest request)
        {
            registeredWedding.TryRemove(request.MarriageId, out _);
        }

        public MarriageProto.CompleteWeddingResponse CompleteWedding(CompleteWeddingRequest request)
        {
            if (registeredWedding.TryGetValue(request.MarriageId, out var wedding) && _marriageManager.CompleteMarriage(wedding.Id))
            {
                var ring = _server.RingManager.CreateRing(request.MarriageItemId, wedding.GroomId, wedding.BrideId, wedding.Id);
                return new CompleteWeddingResponse
                {
                    GroomRingId = ring.RingId1,
                    BrideRingId = ring.RingId2,
                    MarriageId = wedding.Id,
                    MarriageItemId = ring.ItemId,
                    RingSourceId = ring.Id,
                    BrideId = wedding.BrideId,
                    BrideName = _server.CharacterManager.GetPlayerName(wedding.BrideId),
                    GroomId = wedding.GroomId,
                    GroomName = _server.CharacterManager.GetPlayerName(wedding.GroomId),
                };
            }
            return new MarriageProto.CompleteWeddingResponse { Code = 1 };
        }

        public MarriageProto.InviteGuestResponse InviteGuest(MarriageProto.InviteGuestRequest request)
        {
            InviteErrorCode code = InviteErrorCode.Success;
            var guestChr = _server.CharacterManager.FindPlayerByName(request.GuestName);
            if (guestChr == null)
            {
                code = InviteErrorCode.GuestNotFound;
                return new InviteGuestResponse() { Code = (int)code };
            }

            if (!registeredWedding.TryGetValue(request.MarriageId, out var wedding))
            {
                code = InviteErrorCode.MarriageNotFound;
                return new InviteGuestResponse() { Code = (int)code };
            }

            if (wedding.Guests.Contains(guestChr.Character.Id))
            {
                code = InviteErrorCode.DuplicateInvitation;
                return new InviteGuestResponse() { Code = (int)code };
            }

            if (_server.getCurrentTime() >= wedding.StartTime)
            {
                code = InviteErrorCode.WeddingUnderway;
                return new InviteGuestResponse() { Code = (int)code };
            }

            var res = new MarriageProto.InviteGuestCallback
            {
                WeddingId = wedding.Id,
                BrideName = _server.CharacterManager.GetPlayerName(wedding.BrideId),
                GroomName = _server.CharacterManager.GetPlayerName(wedding.GroomId),
                GuestId = guestChr.Character.Id,
                IsCathedral = wedding.IsCathedral
            };
            wedding.Guests.Add(guestChr.Character.Id);
            if (guestChr.Channel <= 0)
            {
                // duey发送
            }
            _transport.ReturnGuestInvitation(res);
            return new InviteGuestResponse { Code = (int)code };
        }

        public MarriageProto.LoadInvitationResponse GetInvitationContent(MarriageProto.LoadInvitationRequest request)
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

        public MarriageProto.WeddingInfoListDto QueryWeddings(MarriageProto.LoadWeddingByIdRequest request)
        {
            var data = registeredWedding.Values.Where(x => request.Id.Contains(x.Id)).ToArray();
            var res = new MarriageProto.WeddingInfoListDto();
            res.List.AddRange(_mapper.Map<MarriageProto.WeddingInfoDto[]>(data));
            return res;
        }
    }
}
