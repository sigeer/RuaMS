using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Application.Shared.Constants.Buddy;
using Application.Shared.Message;
using BuddyProto;
using Google.Protobuf;
using MessageProto;
using System.Runtime.ConstrainedExecution;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class BuddyHandlers
    {
        internal class InvokeBuddyAddHandler : InternalSessionChannelHandler<BuddyProto.AddBuddyResponse>
        {
            public InvokeBuddyAddHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyAdd;

            protected override async Task HandleMessage(AddBuddyResponse res)
            {
                if (res.Code == 0)
                {
                    await _server.SendToPlayersAsync([res.MasterId, res.TargetId], async chr =>
                    {
                        if (chr.Id == res.MasterId)
                        {
                            chr.BuddyList.Set(_server.Mapper.Map<BuddyCharacter>(res.Buddy));
                            await chr.SendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                        }
                        else if (chr.Id == res.TargetId)
                        {
                            if (chr.BuddyList.Contains(res.Buddy.Id))
                            {
                                chr.BuddyList.Set(_server.Mapper.Map<BuddyCharacter>(res.Buddy));
                                await chr.SendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                            }
                            else
                            {
                                await chr.SendPacket(PacketCreator.requestBuddylistAdd(res.Buddy.Id, res.MasterId, res.Buddy.Name));
                            }
                        }
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.MasterId, async chr =>
                    {
                        await chr.Popup(nameof(ClientMessage.PlayerNotFound), res.TargetName);
                    });
                }
            }

            protected override AddBuddyResponse Parse(ByteString data) => AddBuddyResponse.Parser.ParseFrom(data);
        }

        internal class InvokeBuddyDeleteHandler : InternalSessionChannelHandler<BuddyProto.DeleteBuddyResponse>
        {
            public InvokeBuddyDeleteHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyRemove;

            protected override async Task HandleMessage(DeleteBuddyResponse res)
            {
                await _server.SendToPlayersAsync([res.MasterId, res.Buddyid], async chr =>
                {
                    if (res.MasterId == chr.Id)
                    {
                        chr.BuddyList.Remove(res.Buddyid);
                        await chr.SendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                    }
                    else if (res.Buddyid == chr.Id)
                    {
                        if (chr.BuddyList.Contains(res.MasterId))
                        {
                            await chr.SendPacket(PacketCreator.updateBuddyChannel(res.Buddyid, -1));
                        }
                    }
                });
            }

            protected override DeleteBuddyResponse Parse(ByteString data) => DeleteBuddyResponse.Parser.ParseFrom(data);
        }

        internal class GetLocation : InternalSessionChannelHandler<BuddyProto.GetLocationResponse>
        {
            public GetLocation(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyLocation;

            protected override async Task HandleMessage(GetLocationResponse res)
            {
                await _server.SendToPlayerAsync(res.MasterId, async chr =>
                {
                    var code = (WhisperLocationResponseCode)res.Code;
                    switch (code)
                    {
                        case WhisperLocationResponseCode.NotFound:
                        case WhisperLocationResponseCode.NotOnlined:
                        case WhisperLocationResponseCode.NoAccess:
                            await chr.SendPacket(PacketCreator.getWhisperResult(res.TargetName, false));
                            break;
                        case WhisperLocationResponseCode.AwayWorld:
                            await chr.SendPacket(PacketCreator.GetFindResult(res.TargetName, WhisperType.RT_CASH_SHOP, -1, WhisperFlag.LOCATION));
                            break;
                        case WhisperLocationResponseCode.DiffChannel:
                            await chr.SendPacket(PacketCreator.GetFindResult(res.TargetName, WhisperType.RT_DIFFERENT_CHANNEL, res.Field, WhisperFlag.LOCATION));
                            break;
                        default:
                            break;
                    }
                });
            }

            protected override GetLocationResponse Parse(ByteString data) => GetLocationResponse.Parser.ParseFrom(data);
        }

        internal class Whisper : InternalSessionChannelHandler<MessageProto.SendWhisperMessageResponse>
        {
            public Whisper(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnWhisper;

            protected override async Task HandleMessage(SendWhisperMessageResponse res)
            {
                if (res.Code == 0)
                {
                    await _server.SendToPlayerAsync(res.ReceiverId, async chr =>
                    {
                        await chr.SendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, true));
                        await chr.SendPacket(PacketCreator.getWhisperReceive(res.FromName, res.FromChannel - 1, res.IsFromGM, res.Request.Text));
                    });
                }
                else
                {
                    await _server.SendToPlayerAsync(res.Request.FromId, async chr =>
                    {
                        await chr.SendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, false));
                    });
                }
            }

            protected override SendWhisperMessageResponse Parse(ByteString data) => SendWhisperMessageResponse.Parser.ParseFrom(data);
        }
    }
}
