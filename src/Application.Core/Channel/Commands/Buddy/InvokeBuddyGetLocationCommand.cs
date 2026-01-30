using Application.Shared.Constants.Buddy;
using BuddyProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeBuddyGetLocationCommand : IWorldChannelCommand
    {
        GetLocationResponse res;

        public InvokeBuddyGetLocationCommand(GetLocationResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.MasterId);
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
        }
    }
}
