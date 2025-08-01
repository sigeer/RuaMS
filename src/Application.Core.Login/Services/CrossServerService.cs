using Application.Core.Login.Models;
using Application.Shared.Constants;
using AutoMapper;
using Dto;
using System.Collections.Concurrent;

namespace Application.Core.Login.Services
{
    public class CrossServerService
    {
        readonly MasterServer _server;
        readonly IMapper _mapper;

        ConcurrentDictionary<int, List<CallbackModel>> _callbacks = new();

        public CrossServerService(MasterServer server, IMapper mapper)
        {
            _server = server;
            _mapper = mapper;
        }

        public Dto.WrapPlayerByNameResponse WarpPlayerByName(Dto.WrapPlayerByNameRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel <= 0)
                return new Dto.WrapPlayerByNameResponse { Code = 1 };

            if (!_callbacks.TryGetValue(request.MasterId, out var list))
            {
                list = new List<CallbackModel>();
                _callbacks[request.MasterId] = list;
            }

            list.Add(new CallbackModel
            {
                CallbackName = RemoteCallMethods.WarpPlayerByName,
                Params = [new CallbackParamModel() { Index = 0, Schema = typeof(string).Name, Value = request.Victim }]
            });

            return new Dto.WrapPlayerByNameResponse { TargetChannel = targetChr.Channel };
        }

        public Dto.SummonPlayerByNameResponse SummonPlayerByName(Dto.SummonPlayerByNameRequest request)
        {
            return SummonPlayer(request.MasterId, _server.CharacterManager.FindPlayerByName(request.Victim));
        }

        public Dto.SummonPlayerByNameResponse SummonPlayerById(int operatorId, int targetId)
        {
            return SummonPlayer(operatorId, _server.CharacterManager.FindPlayerById(targetId));
        }

        private Dto.SummonPlayerByNameResponse SummonPlayer(int operatorId, CharacterLiveObject? targetChr)
        {
            if (targetChr == null || targetChr.Channel <= 0)
                return new Dto.SummonPlayerByNameResponse { Code = 1 };

            _server.Transport.SendWrapPlayerByName(new Dto.SummonPlayerByNameBroadcast { WarpToName = _server.CharacterManager.GetPlayerName(operatorId), MasterId = targetChr.Character.Id });

            return new Dto.SummonPlayerByNameResponse();
        }


        public Dto.RemoteCallDto[] GetCallback(int chrId)
        {
            if (_callbacks.TryRemove(chrId, out var d))
                return _mapper.Map<Dto.RemoteCallDto[]>(d);
            return [];
        }

        public DisconnectPlayerByNameResponse DisconnectPlayerByName(DisconnectPlayerByNameRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel <= 0)
                return new Dto.DisconnectPlayerByNameResponse { Code = 1 };

            _server.Transport.SendPlayerDisconnect(new Dto.DisconnectPlayerByNameBroadcast { MasterId = targetChr.Character.Id });

            return new Dto.DisconnectPlayerByNameResponse();
        }
    }
}
