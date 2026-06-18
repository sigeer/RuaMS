using Application.Core.Game.Relation;
using Application.Resources.Messages;
using Application.Shared.Constants.Buddy;
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
                                map.getCharacterById(res.MasterId)?.Popup(nameof(ClientMessage.PlayerNotFound), res.TargetName);
                            });
                            return;
                        }

                        if (res.Code == 0)
                        {
                            actor.Send(async map =>
                            {
                                var masterChr = map.getCharacterById(res.MasterId);
                                if (masterChr != null)
                                {
                                    masterChr.BuddyList.Set(worldChannel.Mapper.Map<BuddyCharacter>(res.Buddy));
                                    await masterChr.SendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
                                }
                            });
                        }
                    }

                    var toActor = worldChannel.getPlayerStorage().GetCharacterActor(res.TargetId);
                    if (toActor != null)
                    {
                        if (res.Code == 0)
                        {
                            toActor.Send(async map =>
                            {
                                var chr = map.getCharacterById(res.Buddy.Id);
                                if (chr != null)
                                {
                                    if (chr.BuddyList.Contains(res.Buddy.Id))
                                    {
                                        chr.BuddyList.Set(worldChannel.Mapper.Map<BuddyCharacter>(res.Buddy));
                                        await chr.SendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                                    }
                                    else
                                    {
                                        await chr.SendPacket(PacketCreator.requestBuddylistAdd(res.Buddy.Id, res.MasterId, res.Buddy.Name));
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
                _server.Broadcast(w =>
                {
                    var masterChrActor = w.getPlayerStorage().GetCharacterActor(res.MasterId);
                    masterChrActor?.Send(async m =>
                    {
                        var masterChr = m.getCharacterById(res.MasterId);
                        if (masterChr != null)
                        {
                            masterChr.BuddyList.Remove(res.Buddyid);
                            await masterChr.SendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
                        }
                    });

                    var chrActor = w.getPlayerStorage().GetCharacterActor(res.Buddyid);
                    chrActor?.Send(async m =>
                    {
                        var chr = m.getCharacterById(res.Buddyid);
                        if (chr != null)
                        {
                            if (chr.BuddyList.Contains(res.MasterId))
                            {
                                await chr.SendPacket(PacketCreator.updateBuddyChannel(res.Buddyid, -1));
                            }
                        }
                    });

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

            protected override void HandleMessage(GetLocationResponse res)
            {
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(res.MasterId)?.Send(async m =>
                    {
                        var chr = m.getCharacterById(res.MasterId);
                        if (chr != null)
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
                        }
                    });
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

            protected override void HandleMessage(SendWhisperMessageResponse res)
            {
                _server.Broadcast(w =>
                {
                    if (res.Code != 0)
                    {
                        w.getPlayerStorage().GetCharacterActor(res.Request.FromId)?.Send(async m =>
                        {
                            var chr = m.getCharacterById(res.Request.FromId);
                            if (chr != null)
                                await chr.SendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, false));
                        });
                    }
                    else
                    {
                        w.getPlayerStorage().GetCharacterActor(res.ReceiverId)?.Send(async m =>
                        {
                            var chr = m.getCharacterById(res.ReceiverId);
                            if (chr != null)
                            {
                                await chr.SendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, true));
                                await chr.SendPacket(PacketCreator.getWhisperReceive(res.FromName, res.FromChannel - 1, res.IsFromGM, res.Request.Text));
                            }
                        });
                    }
                });
            }

            protected override SendWhisperMessageResponse Parse(ByteString data) => SendWhisperMessageResponse.Parser.ParseFrom(data);
        }
    }
}
