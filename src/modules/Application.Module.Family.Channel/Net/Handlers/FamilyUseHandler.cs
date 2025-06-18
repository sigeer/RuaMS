/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Core.Game.Invites;
using Application.Core.Game.Players;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Constants.Map;
using Application.Shared.MapObjects;
using Application.Shared.Net;
using Application.Utility;

namespace Application.Module.Family.Channel.Net.Handlers;

/**
 * @author Moogra
 * @author Ubaware
 */
public class FamilyUseHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public FamilyUseHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        FamilyEntitlement type = FamilyEntitlement.Parse(p.readInt());
        int cost = type.getRepCost();
        var family = _familyManager.GetFamily(c.OnlinedCharacter.Id)!;
        var entry = family.getEntryByID(c.OnlinedCharacter.Id)!;
        if (entry.Reputation < cost || entry.isEntitlementUsed(type))
        {
            return; // shouldn't even be able to request it
        }
        c.sendPacket(FamilyPacketCreator.getFamilyInfo(entry));
        var toName = p.readString();
        IPlayer? victim;
        if (type == FamilyEntitlement.FAMILY_REUINION || type == FamilyEntitlement.SUMMON_FAMILY)
        {
            victim = c.CurrentServer.getPlayerStorage().getCharacterByName(toName);
            if (victim != null && victim != c.OnlinedCharacter)
            {
                if (_familyManager.GetFamilyByPlayerId(victim.Id) == _familyManager.GetFamilyByPlayerId(c.OnlinedCharacter.Id))
                {
                    var targetMap = victim.getMap();
                    var ownMap = c.OnlinedCharacter.getMap();
                    if (targetMap != null)
                    {
                        if (type == FamilyEntitlement.FAMILY_REUINION)
                        {
                            if (!FieldLimit.CANNOTMIGRATE.check(ownMap.getFieldLimit()) && !FieldLimit.CANNOTVIPROCK.check(targetMap.getFieldLimit())
                                    && (targetMap.getForcedReturnId() == MapId.NONE || MapId.isMapleIsland(targetMap.getId())) && targetMap.getEventInstance() == null)
                            {

                                c.OnlinedCharacter.changeMap(victim.getMap(), victim.getMap().getPortal(0));
                                _familyManager.UseEntitlement(c.OnlinedCharacter, type);
                            }
                            else
                            {
                                c.sendPacket(FamilyPacketCreator.sendFamilyMessage(75, 0)); // wrong message, but close enough. (client should check this first anyway)
                                return;
                            }
                        }
                        else
                        {
                            if (!FieldLimit.CANNOTMIGRATE.check(targetMap.getFieldLimit()) && !FieldLimit.CANNOTVIPROCK.check(ownMap.getFieldLimit())
                                    && (ownMap.getForcedReturnId() == MapId.NONE || MapId.isMapleIsland(ownMap.getId())) && ownMap.getEventInstance() == null)
                            {

                                _familyManager.CreateSummonInvite(c.OnlinedCharacter, toName);
                            }
                            else
                            {
                                c.sendPacket(FamilyPacketCreator.sendFamilyMessage(75, 0));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    c.sendPacket(FamilyPacketCreator.sendFamilyMessage(67, 0));
                }
            }
        }
        else if (type == FamilyEntitlement.FAMILY_BONDING)
        {
            //not implemented
        }
        else
        {
            //bool party = false;
            //bool isExp = false;
            //float rate = 1.5f;
            //int duration = 15;
            //while (true)
            //{
            //    if (type == FamilyEntitlement.PARTY_EXP_2_30MIN)
            //    {
            //        party = true;
            //        isExp = true;
            //        type = FamilyEntitlement.SELF_EXP_2_30MIN;
            //        continue;
            //    }
            //    else if (type == FamilyEntitlement.PARTY_DROP_2_30MIN)
            //    {
            //        party = true;
            //        type = FamilyEntitlement.SELF_DROP_2_30MIN;
            //        continue;
            //    }
            //    else if (type == FamilyEntitlement.SELF_DROP_2_30MIN)
            //    {
            //        duration = 30;
            //    }
            //    else if (type == FamilyEntitlement.SELF_DROP_2)
            //    {
            //        rate = 2.0f;
            //    }
            //    else if (type == FamilyEntitlement.SELF_EXP_2_30MIN)
            //    {
            //        duration = 30;
            //    }
            //    else if (type == FamilyEntitlement.SELF_EXP_2)
            //    {
            //        rate = 2.0f;
            //    }
            //    else if (type == FamilyEntitlement.SELF_EXP_1_5)
            //    {
            //        isExp = true;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //    break;
            //}
            //not implemented
        }
    }
}
