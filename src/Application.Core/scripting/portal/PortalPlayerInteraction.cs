/*
 This file is part of the OdinMS Maple Story Server
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


using scripting.map;
using server.maps;
using tools;

namespace scripting.portal;
public class PortalPlayerInteraction : AbstractPlayerInteraction
{
    private Portal portal;

    public PortalPlayerInteraction(IChannelClient c, Portal portal) : base(c)
    {
        this.portal = portal;
    }

    public Portal getPortal()
    {
        return portal;
    }

    public void runMapScript()
    {
        MapScriptManager msm = c.CurrentServer.MapScriptManager;
        msm.runMapScript(c, "onUserEnter/" + portal.getScriptName(), false);
    }

    public bool hasLevel30Character()
    {
        using var dbContext = new DBContext();
        return dbContext.Characters.Where(x => x.AccountId == getPlayer().getAccountID()).Any(x => x.Level >= 30);
    }

    public void blockPortal()
    {
        c.OnlinedCharacter.blockPortal(getPortal().getScriptName());
    }

    public void unblockPortal()
    {
        c.OnlinedCharacter.unblockPortal(getPortal().getScriptName());
    }

    public void playPortalSound()
    {
        c.sendPacket(PacketCreator.playPortalSound());
    }
}