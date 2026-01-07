using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Application.Shared.Constants.Buddy;
using Application.Shared.Internal;
using Application.Shared.Message;
using AutoMapper;
using BuddyProto;
using Google.Protobuf;
using MessageProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;
using XmlWzReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class BuddyHandlers
    {
        internal class InvokeBuddyAddHandler : InternalSessionChannelHandler<BuddyProto.AddBuddyResponse>
        {
            readonly IMapper _mapper;
            public InvokeBuddyAddHandler(WorldChannelServer server, IMapper mapper) : base(server)
            {
                _mapper = mapper;
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyAdd;

            protected override Task HandleAsync(AddBuddyResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.MasterId);
                if (masterChr != null)
                {
                    if (res.Code == 1)
                    {
                        masterChr.Popup(nameof(ClientMessage.PlayerNotFound), res.TargetName);
                        return Task.CompletedTask;
                    }

                    if (res.Code == 0)
                    {
                        masterChr.BuddyList.Set(_mapper.Map<BuddyCharacter>(res.Buddy));
                        masterChr.sendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
                    }
                }

                var chr = _server.FindPlayerById(res.TargetId);
                if (chr != null)
                {
                    if (res.Code == 0)
                    {
                        if (chr.BuddyList.Contains(res.Buddy.Id))
                        {
                            chr.BuddyList.Set(_mapper.Map<BuddyCharacter>(res.Buddy));
                            chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                        }
                        else
                        {
                            chr.sendPacket(PacketCreator.requestBuddylistAdd(res.Buddy.Id, res.MasterId, res.Buddy.Name));
                        }
                    }
                }
                return Task.CompletedTask;
            }

            protected override AddBuddyResponse Parse(ByteString data) => AddBuddyResponse.Parser.ParseFrom(data);
        }

        internal class InvokeBuddyDeleteHandler : InternalSessionChannelHandler<BuddyProto.DeleteBuddyResponse>
        {
            public InvokeBuddyDeleteHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyRemove;

            protected override Task HandleAsync(DeleteBuddyResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.MasterId);
                if (masterChr != null)
                {
                    masterChr.BuddyList.Remove(res.Buddyid);
                    masterChr.sendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
                }

                var chr = _server.FindPlayerById(res.Buddyid);
                if (chr != null)
                {
                    if (chr.BuddyList.Contains(res.MasterId))
                    {
                        chr.sendPacket(PacketCreator.updateBuddyChannel(res.Buddyid, -1));
                    }
                }
                return Task.CompletedTask;
            }

            protected override DeleteBuddyResponse Parse(ByteString data) => DeleteBuddyResponse.Parser.ParseFrom(data);
        }

        internal class GetLocation : InternalSessionChannelHandler<BuddyProto.GetLocationResponse>
        {
            public GetLocation(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyLocation;

            protected override Task HandleAsync(GetLocationResponse res, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(res.MasterId);
                if (chr != null)
                {
                    var code = (WhisperLocationResponseCode)res.Code;
                    switch (code)
                    {
                        case WhisperLocationResponseCode.NotFound:
                        case WhisperLocationResponseCode.NotOnlined:
                        case WhisperLocationResponseCode.NoAccess:
                            chr.sendPacket(PacketCreator.getWhisperResult(res.TargetName, false));
                            break;
                        case WhisperLocationResponseCode.AwayWorld:
                            chr.sendPacket(PacketCreator.GetFindResult(res.TargetName, WhisperType.RT_CASH_SHOP, -1, WhisperFlag.LOCATION));
                            break;
                        case WhisperLocationResponseCode.DiffChannel:
                            chr.sendPacket(PacketCreator.GetFindResult(res.TargetName, WhisperType.RT_DIFFERENT_CHANNEL, res.Field, WhisperFlag.LOCATION));
                            break;
                        default:
                            break;
                    }
                }

                return Task.CompletedTask;
            }

            protected override GetLocationResponse Parse(ByteString data) => GetLocationResponse.Parser.ParseFrom(data);
        }

        internal class Whisper : InternalSessionChannelHandler<MessageProto.SendWhisperMessageResponse>
        {
            public Whisper(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnWhisper;

            protected override Task HandleAsync(SendWhisperMessageResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.Request.FromId);
                if (masterChr != null)
                {
                    if (res.Code != 0)
                    {
                        masterChr.sendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, false));
                        return Task.CompletedTask;
                    }
                }

                var chr = _server.FindPlayerById(res.ReceiverId);
                if (chr != null)
                {
                    if (res.Code == 0)
                    {
                        chr.sendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, true));
                        chr.sendPacket(PacketCreator.getWhisperReceive(res.FromName, res.FromChannel - 1, res.IsFromGM, res.Request.Text));
                    }
                }

                return Task.CompletedTask;
            }

            protected override SendWhisperMessageResponse Parse(ByteString data) => SendWhisperMessageResponse.Parser.ParseFrom(data);
        }
    }
}
