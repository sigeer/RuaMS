using Application.Core.Game.Relation;
using Application.Resources.Messages;
using BuddyProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeAddBuddyCallbackCommand : IWorldChannelCommand
    {
        AddBuddyResponse res;

        public InvokeAddBuddyCallbackCommand(AddBuddyResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.MasterId);
            if (masterChr != null)
            {
                if (res.Code == 1)
                {
                    masterChr.Popup(nameof(ClientMessage.PlayerNotFound), res.TargetName);
                    return;
                }

                if (res.Code == 0)
                {
                    masterChr.BuddyList.Set(ctx.WorldChannel.Mapper.Map<BuddyCharacter>(res.Buddy));
                    masterChr.sendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
                }
            }

            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.TargetId);
            if (chr != null)
            {
                if (res.Code == 0)
                {
                    if (chr.BuddyList.Contains(res.Buddy.Id))
                    {
                        chr.BuddyList.Set(ctx.WorldChannel.Mapper.Map<BuddyCharacter>(res.Buddy));
                        chr.sendPacket(PacketCreator.updateBuddylist(chr.BuddyList.getBuddies()));
                    }
                    else
                    {
                        chr.sendPacket(PacketCreator.requestBuddylistAdd(res.Buddy.Id, res.MasterId, res.Buddy.Name));
                    }
                }
            }
        }
    }
}
