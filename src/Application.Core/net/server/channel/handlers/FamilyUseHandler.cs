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


using Application.Core.Game.Invites;
using client;
using constants.id;
using net.packet;
using net.server.coordinator.world;
using server.maps;
using tools;

namespace net.server.channel.handlers;

/**
 * @author Moogra
 * @author Ubaware
 */
public class FamilyUseHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
        FamilyEntitlement type = EnumClassUtils.GetValues<FamilyEntitlement>()[p.readInt()];
        int cost = type.getRepCost();
        var entry = c.OnlinedCharacter.getFamilyEntry();
        if (entry.getReputation() < cost || entry.isEntitlementUsed(type))
        {
            return; // shouldn't even be able to request it
        }
        c.sendPacket(PacketCreator.getFamilyInfo(entry));
        IPlayer? victim;
        if (type == FamilyEntitlement.FAMILY_REUINION || type == FamilyEntitlement.SUMMON_FAMILY)
        {
            victim = c.getChannelServer().getPlayerStorage().getCharacterByName(p.readString());
            if (victim != null && victim != c.OnlinedCharacter)
            {
                if (victim.getFamily() == c.OnlinedCharacter.getFamily())
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
                                useEntitlement(entry, type);
                            }
                            else
                            {
                                c.sendPacket(PacketCreator.sendFamilyMessage(75, 0)); // wrong message, but close enough. (client should check this first anyway)
                                return;
                            }
                        }
                        else
                        {
                            if (!FieldLimit.CANNOTMIGRATE.check(targetMap.getFieldLimit()) && !FieldLimit.CANNOTVIPROCK.check(ownMap.getFieldLimit())
                                    && (ownMap.getForcedReturnId() == MapId.NONE || MapId.isMapleIsland(ownMap.getId())) && ownMap.getEventInstance() == null)
                            {

                                if (InviteType.FAMILY_SUMMON.HasRequest(victim.getId()))
                                {
                                    c.sendPacket(PacketCreator.sendFamilyMessage(74, 0));
                                    return;
                                }
                                InviteType.FAMILY_SUMMON.CreateInvite(new FamilySummonInviteRequest(c.OnlinedCharacter, victim));
                                victim.sendPacket(PacketCreator.sendFamilySummonRequest(c.OnlinedCharacter.getFamily().getName(), c.OnlinedCharacter.getName()));
                                useEntitlement(entry, type);
                            }
                            else
                            {
                                c.sendPacket(PacketCreator.sendFamilyMessage(75, 0));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    c.sendPacket(PacketCreator.sendFamilyMessage(67, 0));
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

    private bool useEntitlement(FamilyEntry entry, FamilyEntitlement entitlement)
    {
        if (entry.useEntitlement(entitlement))
        {
            entry.gainReputation(-entitlement.getRepCost(), false);
            entry.getChr().sendPacket(PacketCreator.getFamilyInfo(entry));
            return true;
        }
        return false;
    }
}
