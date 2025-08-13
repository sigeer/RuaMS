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

        public SystemProto.WrapPlayerByNameResponse WarpPlayerByName(SystemProto.WrapPlayerByNameRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel <= 0)
                return new SystemProto.WrapPlayerByNameResponse { Code = 1 };

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

            return new SystemProto.WrapPlayerByNameResponse { TargetChannel = targetChr.Channel };
        }

        public SystemProto.SummonPlayerByNameResponse SummonPlayerByName(SystemProto.SummonPlayerByNameRequest request)
        {
            return SummonPlayer(request.MasterId, _server.CharacterManager.FindPlayerByName(request.Victim));
        }

        public SystemProto.SummonPlayerByNameResponse SummonPlayerById(int operatorId, int targetId)
        {
            return SummonPlayer(operatorId, _server.CharacterManager.FindPlayerById(targetId));
        }

        private SystemProto.SummonPlayerByNameResponse SummonPlayer(int operatorId, CharacterLiveObject? targetChr)
        {
            if (targetChr == null || targetChr.Channel <= 0)
                return new SystemProto.SummonPlayerByNameResponse { Code = 1 };

            _server.Transport.SendWrapPlayerByName(new SystemProto.SummonPlayerByNameBroadcast { WarpToName = _server.CharacterManager.GetPlayerName(operatorId), MasterId = targetChr.Character.Id });

            return new SystemProto.SummonPlayerByNameResponse();
        }


        public Dto.RemoteCallDto[] GetCallback(int chrId)
        {
            if (_callbacks.TryRemove(chrId, out var d))
                return _mapper.Map<Dto.RemoteCallDto[]>(d);
            return [];
        }

        public SystemProto.DisconnectPlayerByNameResponse DisconnectPlayerByName(SystemProto.DisconnectPlayerByNameRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel == 0)
                return new SystemProto.DisconnectPlayerByNameResponse { Code = 1 };

            _server.DisconnectChr(targetChr.Character.Id);

            return new SystemProto.DisconnectPlayerByNameResponse();
        }
    }
}
