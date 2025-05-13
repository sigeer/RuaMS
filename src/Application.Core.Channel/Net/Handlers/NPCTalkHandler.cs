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


using Application.Core.Game.Life;
using client.processor.npc;
using constants.id;
using net.packet;
using scripting.npc;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class NPCTalkHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (!c.OnlinedCharacter.isAlive())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (currentServerTime() - c.OnlinedCharacter.getNpcCooldown() < YamlConfig.config.server.BLOCK_NPC_RACE_CONDT)
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        int oid = p.readInt();
        var obj = c.OnlinedCharacter.getMap().getMapObject(oid);
        if (obj is NPC npc)
        {
            if (YamlConfig.config.server.USE_DEBUG)
            {
                c.OnlinedCharacter.dropMessage(5, "Talking to NPC " + npc.getId());
            }

            if (npc.getId() == NpcId.DUEY)
            {
                DueyProcessor.dueySendTalk(c, false);
            }
            else
            {
                if (c.NPCConversationManager != null)
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                // Custom handling to reduce the amount of scripts needed.
                if (npc.getId() >= NpcId.GACHAPON_MIN && npc.getId() <= NpcId.GACHAPON_MAX)
                {
                    NPCScriptManager.getInstance().start(c, npc.getId(), "gachapon", null);
                }
                else if (npc.getName().EndsWith("Maple TV"))
                {
                    NPCScriptManager.getInstance().start(c, npc.getId(), "mapleTV", null);
                }
                else
                {
                    bool hasNpcScript = NPCScriptManager.getInstance().start(c, npc.getId(), oid, null);
                    if (!hasNpcScript)
                    {
                        if (!npc.hasShop())
                        {
                            log.Warning("NPC {NPCName} ({NPCId}) is not coded", npc.getName(), npc.getId());
                            return;
                        }
                        else if (c.OnlinedCharacter.getShop() != null)
                        {
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }

                        npc.sendShop(c);
                    }
                }
            }
        }
        else if (obj is PlayerNPC pnpc)
        {
            NPCScriptManager nsm = NPCScriptManager.getInstance();

            if (pnpc.getScriptId() < NpcId.CUSTOM_DEV && !nsm.isNpcScriptAvailable(c, "" + pnpc.getScriptId()))
            {
                nsm.start(c, pnpc.getScriptId(), "rank_user", null);
            }
            else
            {
                nsm.start(c, pnpc.getScriptId(), null);
            }
        }
    }
}
