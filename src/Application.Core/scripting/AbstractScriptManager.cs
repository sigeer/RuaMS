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


using Application.Core.Game.Life;
using Application.Core.Game.Relation;
using Application.Core.Managers;
using client;
using client.command;
using client.inventory;
using client.inventory.manipulator;
using constants.game;
using constants.id;
using Microsoft.ClearScript.V8;
using net.server;
using net.server.channel;
using net.server.channel.handlers;
using server;
using server.expeditions;
using server.gachapon;
using server.life;
using tools;
using tools.packets;

namespace scripting;

/**
 * @author Matze
 */
public abstract class AbstractScriptManager
{
    protected ILogger log;

    protected AbstractScriptManager()
    {
        log = LogFactory.GetLogger($"script/{GetType().Name}");
    }

    protected V8ScriptEngine? getInvocableScriptEngine(string path)
    {
        path = GetFullScriptPath(path);
        if (!File.Exists(path))
        {
            log.Fatal($"script {path} not found");
            return null;
        }

        V8ScriptEngine engine = new V8ScriptEngine();
        try
        {
            engine.AddHostType(typeof(Console));
            engine.AddHostType(typeof(Item));
            engine.AddHostType(typeof(InventoryManipulator));
            engine.AddHostType(typeof(Guild));
            engine.AddHostType(typeof(BuffStat));
            engine.AddHostType(typeof(MapId));
            engine.AddHostType(typeof(Rectangle));
            engine.AddHostType(typeof(RingActionHandler));
            engine.AddHostType("Channel", typeof(WorldChannel));
            engine.AddHostType(typeof(CommandsExecutor));
            engine.AddHostType(typeof(CharacterManager));
            engine.AddHostType(typeof(Gachapon));
            engine.AddHostType(typeof(ItemInformationProvider));
            engine.AddHostType(typeof(MonsterBook));
            engine.AddHostType(typeof(Job));
            engine.AddHostType(typeof(ExpTable));
            engine.AddHostType(typeof(ExpeditionType));
            engine.AddHostType(typeof(ListExtensions));
            engine.AddHostType(typeof(Enumerable));
            engine.AddHostType(typeof(Server));
            engine.AddHostType(typeof(Point));
            engine.AddHostType(typeof(LifeFactory));
            engine.AddHostType("Wedding", typeof(WeddingPackets));
            engine.AddHostType(typeof(GameConstants));
            engine.AddHostType(typeof(PlayerNPC));
            engine.AddHostType(typeof(ShopFactory));
            engine.AddHostType(typeof(PacketCreator));
            engine.AddHostType(typeof(InventoryType));
            engine.AddHostType(typeof(YamlConfig));
            engine.AddHostType(typeof(MakerProcessor));
            engine.Execute(File.ReadAllText(path));
            return engine;
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
            return null;
        }
    }

    protected V8ScriptEngine getInvocableScriptEngine(string path, IClient c)
    {
        V8ScriptEngine? engine = c.getScriptEngine(path);
        if (engine == null)
        {
            engine = getInvocableScriptEngine(path);
            c.setScriptEngine(path, engine);
        }

        return engine;
    }



    protected void resetContext(string path, IClient c)
    {
        c.removeScriptEngine("scripts/" + path);
    }

    protected string GetFullScriptPath(string relativePath)
    {
        if (relativePath.StartsWith("scripts"))
            return relativePath;

        return "scripts/" + relativePath;
    }

    protected string GetScriptPath(string type, string path)
    {
        return type + "/" + (path.EndsWith(".js") ? path : (path + ".js"));
    }
    protected string GetNpcScriptPath(string path) => GetScriptPath("npc", path);
    protected string GetItemScriptPath(string path) => GetScriptPath("item", path);
    protected string GetQuestScriptPath(string path) => GetScriptPath("quest", path);
    protected string GetEventScriptPath(string path) => GetScriptPath("event", path);
    protected string GetPortalScriptPath(string path) => GetScriptPath("portal", path);
    protected string GetReactorScriptPath(string path) => GetScriptPath("reactor", path);
    protected string GetMapScriptPath(string path) => GetScriptPath("map", path);
}
