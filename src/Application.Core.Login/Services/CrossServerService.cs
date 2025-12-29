using Application.Core.Login.Models;
using Application.Shared.Constants;
using Application.Shared.Message;
using Application.Shared.Models;
using Application.Shared.Servers;
using AutoMapper;
using Dto;
using System.Collections.Concurrent;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using XmlWzReader;

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

        public async Task WarpPlayerByName(SystemProto.WrapPlayerByNameRequest request)
        {
            var res = new SystemProto.WrapPlayerByNameResponse() { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel <= 0)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.WarpPlayer, res, [request.MasterId]);
                return;
            }

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

            res.TargetChannel = targetChr.Channel;
            await _server.Transport.SendMessageN(ChannelRecvCode.WarpPlayer, res, [request.MasterId]);
        }

        public async Task SummonPlayerByName(SystemProto.SummonPlayerByNameRequest request)
        {
            var res = new SystemProto.SummonPlayerByNameResponse() { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel <= 0)
            {
                await _server.Transport.SendMessageN(ChannelRecvCode.SummonPlayer, res, [request.MasterId]);
                return;
            }

            res.WarpToChannel = targetChr.Channel;
            res.WarpToName = _server.CharacterManager.GetPlayerName(request.MasterId);
            res.VictimId = targetChr.Character.Id;
            await _server.Transport.SendMessageN(ChannelRecvCode.SummonPlayer, res, [request.MasterId, res.VictimId]);
        }

        public async Task SummonPlayerById(int operatorId, int targetId)
        {
            await SummonPlayer(operatorId, _server.CharacterManager.FindPlayerById(targetId));
        }

        private async Task SummonPlayer(int operatorId, CharacterLiveObject? targetChr)
        {
            var res = new SystemProto.SummonPlayerByNameResponse() { Request = new SystemProto.SummonPlayerByNameRequest() };
            if (targetChr == null || targetChr.Channel <= 0)
            {
                await _server.Transport.SendMessageN(ChannelRecvCode.SummonPlayer, res, [operatorId]);
                return;
            }

            res.WarpToChannel = targetChr.Channel;
            res.WarpToName = _server.CharacterManager.GetPlayerName(operatorId);
            res.VictimId = targetChr.Character.Id;
            await _server.Transport.SendMessageN(ChannelRecvCode.SummonPlayer, res, [operatorId, res.VictimId]);
        }


        public Dto.RemoteCallDto[] GetCallback(int chrId)
        {
            if (_callbacks.TryRemove(chrId, out var d))
                return _mapper.Map<Dto.RemoteCallDto[]>(d);
            return [];
        }

        public async Task DisconnectPlayerByName(SystemProto.DisconnectPlayerByNameRequest request)
        {
            var res = new SystemProto.DisconnectPlayerByNameResponse() { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null || targetChr.Channel == 0)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeDisconnectPlayer, res, [request.MasterId]);
                return;
            }

            res.TargetId = targetChr.Character.Id;
            await _server.Transport.SendMessageN(ChannelRecvCode.InvokeDisconnectPlayer, res, [res.Request.MasterId, res.TargetId]);

        }
    }
}
