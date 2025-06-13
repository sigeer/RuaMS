using Acornima;
using Application.Core.Login.Models.ChatRoom;
using Application.Shared.Constants;
using AutoMapper;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Application.Core.Login.ServerData
{
    public class ChatRoomManager : IDisposable
    {
        /// <summary>
        /// Key: ChatRoomId
        /// </summary>
        ConcurrentDictionary<int, ChatRoomModel> _dataSource = new();
        /// <summary>
        /// Key: PlayerId
        /// </summary>
        ConcurrentDictionary<int, ChatRoomModel> _playerMapper = new();
        int _currentId = 1;

        readonly MasterServer _server;
        readonly ILogger<ChatRoomManager> _logger;
        readonly IMapper _mapper;

        public ChatRoomManager(MasterServer server, ILogger<ChatRoomManager> logger, IMapper mapper)
        {
            _server = server;
            _logger = logger;
            _mapper = mapper;
        }

        public int CreateChatRoom(Dto.CreateChatRoomRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            if (chr == null)
                return -1;

            if (_playerMapper.ContainsKey(request.MasterId))
                return -1;

            var newRoom = new ChatRoomModel(Interlocked.Increment(ref _currentId));
            _dataSource[newRoom.Id] = newRoom;

            JoinChatRoom(new JoinChatRoomRequest { RoomId = newRoom.Id, MasterId = request.MasterId });
            return newRoom.Id;
        }

        public void JoinChatRoom(Dto.JoinChatRoomRequest request)
        {
            var response = new Dto.JoinChatRoomResponse { Request = request };

            if (_playerMapper.ContainsKey(request.MasterId))
            {
                response.Code = (int)JoinChatRoomResult.AlreadyInChatRoom;
                _server.Transport.BroadcastJoinChatRoom(response);
                return;
            }

            if (!_dataSource.TryGetValue(request.RoomId, out var room))
            {
                response.Code = (int)JoinChatRoomResult.NotFound;
                _server.Transport.BroadcastJoinChatRoom(response);
                return;
            }

            if (!room.TryAddMember(request.MasterId, out var position))
            {
                response.Code = (int)JoinChatRoomResult.CapacityFull;
                _server.Transport.BroadcastJoinChatRoom(response);
                return;
            }

            _playerMapper[request.MasterId] = room;

            var roomDto = _mapper.Map<Dto.ChatRoomDto>(room);
            roomDto.Members.AddRange(
                room.Members
                    .Select(_server.CharacterManager.FindPlayerById)
                    .Select(_mapper.Map<Dto.PlayerViewDto>)
            );

            response.Room = roomDto;
            response.NewComerPosition = position;
            response.Code = (int)JoinChatRoomResult.Success;

            _server.Transport.BroadcastJoinChatRoom(response);
        }

        public void LeaveChatRoom(Dto.LeaveChatRoomRequst request)
        {
            if (_playerMapper.TryRemove(request.MasterId, out var room))
            {
                if (room.TryRemoveMember(request.MasterId, out var position))
                {
                    var roomDto = _mapper.Map<Dto.ChatRoomDto>(room);
                    _server.Transport.BroadcastLeaveChatRoom(new Dto.LeaveChatRoomResponse { Code = 0, Room = roomDto, LeftPosition = position });
                }
                if (room.Members.Length == 0)
                {
                    _dataSource.TryRemove(room.Id, out _);
                }
            }
        }

        public void SendMessage(SendChatRoomMessageRequest request)
        {
            if (_playerMapper.TryRemove(request.MasterId, out var room))
            {
                var res = new Dto.SendChatRoomMessageResponse { Code = 0, Text = request.Text };
                res.Members.AddRange(room.Members);
                _server.Transport.BroadcastChatRoomMessage(res);
            }
        }
        public void Dispose()
        {
            _dataSource.Clear();
            _playerMapper.Clear();
        }


    }
}
