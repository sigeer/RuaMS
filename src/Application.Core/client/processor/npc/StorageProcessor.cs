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


using Application.Core.Channel.DataProviders;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using net.packet;
using server;
using tools;

/**
 * @author Matze
 * @author Ronan - inventory concurrency protection on storing items
 */
public class StorageProcessor
{
    public static bool hasGMRestrictions(IPlayer character)
    {
        return character.isGM() && character.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_USE_STORAGE;
    }
}
