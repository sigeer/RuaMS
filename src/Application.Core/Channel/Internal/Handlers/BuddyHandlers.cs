using Application.Core.Channel.Commands;
using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Application.Shared.Message;
using BuddyProto;
using Google.Protobuf;
using MessageProto;
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

            protected override void HandleMessage(AddBuddyResponse res)
            {
                _server.Broadcast(worldChannel =>
                {
                    var actor = worldChannel.getPlayerStorage().GetCharacterActor(res.MasterId);
                    if (actor != null)
                    {
                        if (res.Code == 1)
                        {
                            actor.Send(map =>
                            {
                                map.FindPlayer(res.MasterId)?.Popup(nameof(ClientMessage.PlayerNotFound), res.TargetName);
                            });
                            return;
                        }

                        if (res.Code == 0)
                        {
                            actor.Send(map =>
                            {
                                var masterChr = map.FindPlayer(res.MasterId);
                                if (masterChr != null)
                                {
                                    masterChr.BuddyList.Set(worldChannel.Mapper.Map<BuddyCharacter>(res.Buddy));
                                    masterChr.sendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
                                }
                            });
                        }
                    }

                    var toActor = worldChannel.getPlayerStorage().GetCharacterActor(res.TargetId);
                    if (toActor != null)
                    {
                        if (res.Code == 0)
                        {
                            toActor.Send(map =>
                            {
                                var chr = map.FindPlayer(res.Buddy.Id);
                                if (chr != null)
                                {
                                    if (chr.BuddyList.Contains(res.Buddy.Id))
                                    {
                                        chr.BuddyList.Set(worldChannel.Mapper.Map<BuddyCharacter>(res.Buddy));
                                        chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                                    }
                                    else
                                    {
                                        chr.sendPacket(PacketCreator.requestBuddylistAdd(res.Buddy.Id, res.MasterId, res.Buddy.Name));
                                    }
                                }
                            });
                        }
                    }
                });
            }

            protected override AddBuddyResponse Parse(ByteString data) => AddBuddyResponse.Parser.ParseFrom(data);
        }

        internal class InvokeBuddyDeleteHandler : InternalSessionChannelHandler<BuddyProto.DeleteBuddyResponse>
        {
            public InvokeBuddyDeleteHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyRemove;

            protected override void HandleMessage(DeleteBuddyResponse res)
            {
                _server.PushChannelCommand(new InvokeRemoveBuddyCallbackCommand(res));
            }

            protected override DeleteBuddyResponse Parse(ByteString data) => DeleteBuddyResponse.Parser.ParseFrom(data);
        }

        internal class GetLocation : InternalSessionChannelHandler<BuddyProto.GetLocationResponse>
        {
            public GetLocation(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnBuddyLocation;

            protected override void HandleMessage(GetLocationResponse res)
            {
                _server.PushChannelCommand(new InvokeBuddyGetLocationCommand(res));
            }

            protected override GetLocationResponse Parse(ByteString data) => GetLocationResponse.Parser.ParseFrom(data);
        }

        internal class Whisper : InternalSessionChannelHandler<MessageProto.SendWhisperMessageResponse>
        {
            public Whisper(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnWhisper;

            protected override void HandleMessage(SendWhisperMessageResponse res)
            {
                _server.PushChannelCommand(new InvokeWhisperCommand(res));
            }

            protected override SendWhisperMessageResponse Parse(ByteString data) => SendWhisperMessageResponse.Parser.ParseFrom(data);
        }
    }
}
