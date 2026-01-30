using Application.Core.Login.Models;
using Application.Shared.Message;
using Application.Shared.Team;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TeamProto;

namespace Application.Core.Login.ServerData
{
    public class TeamManager : IDisposable
    {
        ConcurrentDictionary<int, TeamModel> _dataSource = new();
        private int _currentId = 1000000001;

        readonly MasterServer _server;
        IMapper _mapper;
        ILogger<TeamManager> _logger;

        public TeamManager(MasterServer server, IMapper mapper, ILogger<TeamManager> logger)
        {
            _server = server;
            _mapper = mapper;
            _logger = logger;
        }

        public TeamProto.TeamDto? GetTeamDto(int teamId)
        {
            var data = GetTeamModel(teamId);
            if (data == null)
                return null;

            return MapTeamDto(data);
        }

        public TeamProto.TeamDto? MapTeamDto(TeamModel data)
        {
            var response = new TeamProto.TeamDto();
            response.Id = data.Id;
            response.LeaderId = data.LeaderId;
            response.Members.AddRange(_mapper.Map<TeamProto.TeamMemberDto[]>(data.GetMembers().Select(_server.CharacterManager.FindPlayerById)));
            return response;
        }
        public TeamModel? GetTeamModel(int teamId) => _dataSource.GetValueOrDefault(teamId);

        public CreateTeamResponse CreateTeam(CreateTeamRequest request)
        {
            var res = new CreateTeamResponse() { Request = request };
            var chrFrom = _server.CharacterManager.FindPlayerById(request.LeaderId)!;
            if (chrFrom.Character.Party > 0)
            {
                res.Code = 1;
                return res;
            }

            var newTeam = new TeamModel(Interlocked.Increment(ref _currentId), chrFrom);
            _dataSource[newTeam.Id] = newTeam;

            chrFrom.Character.Party = newTeam.Id;

            res.TeamDto = MapTeamDto(newTeam);
            return res;
        }
        bool RemoveTeam(int leaderId, int teamId)
        {
            if (_dataSource.TryGetValue(teamId, out var d) && leaderId == d.LeaderId)
                return _dataSource.TryRemove(teamId, out _);
            return false;
        }
        public async Task UpdateParty(int partyid, PartyOperation operation, int fromId, int toId, int reason = 0)
        {
            var response = new TeamProto.UpdateTeamResponse() { 
                Request = new UpdateTeamRequest { 
                    TeamId = partyid, 
                    Operation = (int)operation,
                    FromId = fromId, 
                    TargetId = toId ,
                    Reason = reason
                } 
            };
            UpdateTeamCheckResult errorCode = UpdateTeamCheckResult.Success;

            var party = GetTeamModel(partyid);
            if (party == null)
            {
                errorCode = UpdateTeamCheckResult.TeamNotExsited;
                response.Code = (int)errorCode;

                await SendError(response);
                return;
            }

            var chrFrom = fromId > 0 ? _server.CharacterManager.FindPlayerById(fromId) : null;
            var chrTo = fromId == toId ? chrFrom : _server.CharacterManager.FindPlayerById(toId)!;
            switch (operation)
            {
                case PartyOperation.JOIN:
                    party.TryAddMember(chrTo!, out errorCode);
                    break;
                case PartyOperation.EXPEL:
                    party.TryExpel(fromId, chrTo!, out errorCode);
                    break;
                case PartyOperation.LEAVE:
                    if (fromId == party.LeaderId)
                    {
                        operation = PartyOperation.DISBAND;
                        response.Request.Operation = (int)operation;
                        goto case PartyOperation.DISBAND;
                    }
                    party.TryRemoveMember(chrFrom!, out errorCode);
                    break;
                case PartyOperation.DISBAND:
                    if (fromId != party.LeaderId)
                        errorCode = UpdateTeamCheckResult.Disband_NotLeader;
                    else if (!RemoveTeam(fromId, partyid))
                        errorCode = UpdateTeamCheckResult.Leave_InnerError;
                    else
                        party.Disband();
                    break;
                case PartyOperation.SILENT_UPDATE:
                case PartyOperation.LOG_ONOFF:
                    break;
                case PartyOperation.CHANGE_LEADER:
                    party.TryChangeLeader(fromId, chrTo!, out errorCode);
                    break;
                default:
                    _logger.LogWarning("Unhandled updateParty operation: {PartyOperation}", operation.ToString());
                    break;
            }


            response.Code = (int)errorCode;

            if (errorCode == UpdateTeamCheckResult.Success)
            {
                response.Team = MapTeamDto(party);
                response.TargetName = chrTo!.Character.Name;
                await _server.Transport.SendMessageN(ChannelRecvCode.OnTeamUpdate, response, party.GetMembers().Concat([fromId, toId]));
            }
            else
                await SendError(response);
        }

        async Task SendError(TeamProto.UpdateTeamResponse response)
        {
            var operation = (PartyOperation)response.Request.Operation;
            if (operation != PartyOperation.SILENT_UPDATE && operation != PartyOperation.LOG_ONOFF)
                await _server.Transport.SendMessageN(ChannelRecvCode.OnTeamUpdate, response, [response.Request.FromId]);
        }

        public async Task SendTeamChatAsync(string nameFrom, string chatText)
        {
            var sender = _server.CharacterManager.FindPlayerByName(nameFrom);
            if (sender != null)
            {
                if (_dataSource.TryGetValue(sender.Character.Party, out var team))
                {
                    await _server.Transport.SendMultiChatAsync(1, nameFrom, team.GetMemberObjectss(), chatText);
                }
            }
        }

        public void Dispose()
        {
            _dataSource.Clear();
        }
    }
}
