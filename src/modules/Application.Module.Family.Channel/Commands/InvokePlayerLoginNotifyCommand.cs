using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Module.Family.Channel.Models;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.Family.Channel.Commands
{
    internal class InvokePlayerLoginNotifyCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokePlayerLoginNotifyCommand);
        int _chrId;

        public InvokePlayerLoginNotifyCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(WorldChannel ctx)
        {
            var chr = ctx.Players.getCharacterById(_chrId);
            if (chr == null)
            {
                return;
            }
            var service = ctx.NodeService.ServiceProvider.GetRequiredService<FamilyManager>();
            FamilyEntry? familyEntry = null;

            if (chr.FamilyId > 0)
            {
                var f = service.GetFamilyByPlayerId(chr.Id);
                if (f != null)
                {
                    familyEntry = f.getEntryByID(chr.Id);
                    if (familyEntry != null)
                    {
                        familyEntry.Channel = chr.Channel;
                    }
                }
            }

            chr.sendPacket(FamilyPacketCreator.loadFamily());
            chr.sendPacket(FamilyPacketCreator.getFamilyInfo(familyEntry));
            service.AnnounceToSenior(familyEntry, FamilyPacketCreator.sendFamilyLoginNotice(chr.Name, true), true);
        }
    }
}
