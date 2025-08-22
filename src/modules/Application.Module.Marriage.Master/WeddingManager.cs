using Application.Core.Login;
using Application.Module.Marriage.Common.ErrorCodes;
using Application.Module.Marriage.Master.Models;
using MapsterMapper;
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

            var marriageInfo = _marriageManager.GetEffectMarriageModel(chr.Character.Id);
            if (marriageInfo == null)
            {
                return new ReserveWeddingResponse { Code = (int)ReserveErrorCode.NotYetEngaged };
            }

            if (registeredWedding.ContainsKey(marriageInfo.Id))
            {
                return new ReserveWeddingResponse() { Code = (int)ReserveErrorCode.AlreadyReserved };
            }

            var weddingInfo = new WeddingInfo(marriageInfo.Id, request.Channel, request.IsCathedral, request.IsPremium,
                request.MasterId, marriageInfo.GetPartnerId(chr.Character.Id), [], request.StartTime + (long)TimeSpan.FromMinutes(30).TotalMilliseconds);
            registeredWedding[marriageInfo.Id] = weddingInfo;

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
            if (registeredWedding.TryGetValue(request.MarriageId, out var wedding))
            {
                var marriage = _marriageManager.Query(x => x.Id == request.MarriageId).FirstOrDefault();
                if (marriage != null && marriage.Status == 0)
                {
                    var ring = _server.RingManager.CreateRing(request.MarriageItemId, wedding.GroomId, wedding.BrideId);
                    _marriageManager.CompleteMarriage(marriage, ring);

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
            foreach (var item in data)
            {
                var obj = _mapper.Map<MarriageProto.WeddingInfoDto>(item);
                obj.GroomName = _server.CharacterManager.GetPlayerName(item.GroomId);
                obj.BrideName = _server.CharacterManager.GetPlayerName(item.BrideId);
                res.List.Add(obj);
            }
            return res;
        }
    }
}
