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


using Application.Core.Game.Commands;
using Application.Core.Game.Life;
using Application.Core.Managers;
using Application.Core.Scripting.Infrastructure;
using client;
using client.inventory;
using client.inventory.manipulator;
using constants.game;
using constants.id;
using net.server;
using net.server.channel;
using server;
using server.expeditions;
using server.life;
using System.Collections.Concurrent;
using tools;
using tools.packets;

namespace scripting;

/**
 * @author Matze
 */
public abstract class AbstractScriptManager
{
    protected ILogger log;
    static ConcurrentDictionary<string, string> JsCache { get; set; } = new ConcurrentDictionary<string, string>();

    protected AbstractScriptManager()
    {
        log = LogFactory.GetLogger($"Script/{GetType().Name}");
    }

    protected IEngine? getInvocableScriptEngine(string path)
    {
        path = GetFullScriptPath(path);
        if (!JsCache.TryGetValue(path, out var jsContent))
        {
            if (!File.Exists(path))
            {
                log.Fatal($"script {path} not found");
                return null;
            }
            jsContent = File.ReadAllText(path);
            JsCache[path] = jsContent;
        }


        var engine = new JintEngine();
        try
        {
            engine.AddHostedObject("log", log);

            engine.AddHostedType("Item", typeof(Item));
            engine.AddHostedType("InventoryManipulator", typeof(InventoryManipulator));
            engine.AddHostedType("BuffStat", typeof(BuffStat));
            engine.AddHostedType("MapId", typeof(MapId));
            engine.AddHostedType("Rectangle", typeof(Rectangle));
            engine.AddHostedType("RingManager", typeof(RingManager));
            engine.AddHostedType("Channel", typeof(WorldChannel));
            engine.AddHostedType("CommandExecutor", typeof(CommandExecutor));
            engine.AddHostedType("CharacterManager", typeof(CharacterManager));
            engine.AddHostedType("GachaponManager", typeof(GachaponManager));
            engine.AddHostedType("ItemInformationProvider", typeof(ItemInformationProvider));
            engine.AddHostedType("MonsterBook", typeof(MonsterBook));
            engine.AddHostedType("ExpTable", typeof(ExpTable));
            engine.AddHostedType("ExpeditionType", typeof(ExpeditionType));
            engine.AddHostedType("Server", typeof(Server));
            engine.AddHostedType("Point", typeof(Point));
            engine.AddHostedType("LifeFactory", typeof(LifeFactory));
            engine.AddHostedType("Wedding", typeof(WeddingPackets));
            engine.AddHostedType("GameConstants", typeof(GameConstants));
            engine.AddHostedType("PlayerNPC", typeof(PlayerNPC));
            engine.AddHostedType("ShopFactory", typeof(ShopFactory));
            engine.AddHostedType("PacketCreator", typeof(PacketCreator));
            engine.AddHostedType("YamlConfig", typeof(YamlConfig));
            engine.AddHostedType("MakerProcessor", typeof(MakerProcessor));
            engine.AddHostedType("Guild", typeof(GuildManager));

            engine.AddHostedType("Job", typeof(Job));
            engine.AddHostedType("InventoryType", typeof(InventoryType));

            engine.Evaluate(GetUtilJs());
            engine.Evaluate(jsContent);
            return engine;
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
            return null;
        }
    }

    protected IEngine getInvocableScriptEngine(string path, IClient c)
    {
        var engine = c.getScriptEngine(path);
        if (engine == null)
        {
            engine = getInvocableScriptEngine(path)!;
            c.setScriptEngine(path, engine);
        }

        return engine;
    }

    public static void ClearCache()
    {
        JsCache.Clear();
    }

    protected void resetContext(string path, IClient c)
    {
        c.removeScriptEngine(path);
    }

    protected string GetFullScriptPath(string relativePath)
    {
        return ScriptResFactory.GetScriptFullPath(relativePath);
    }

    protected string GetScriptPath(string type, string path)
    {
        return type + "/" + (path.EndsWith(".js") ? path : (path + ".js"));
    }

    private string GetUtilJs()
    {
        var jsFile = GetFullScriptPath("utils.js");
        if (JsCache.TryGetValue(jsFile, out var jsContent))
            return jsContent;

        return JsCache[jsFile] = File.ReadAllText(jsFile);
    }
    protected string GetCommandScriptPath(string path) => GetScriptPath("commands", path);
    protected string GetNpcScriptPath(string path) => GetScriptPath("npc", path);
    protected string GetItemScriptPath(string path) => GetScriptPath("item", path);
    protected string GetQuestScriptPath(string path) => GetScriptPath("quest", path);
    protected string GetEventScriptPath(string path) => GetScriptPath("event", path);
    protected string GetPortalScriptPath(string path) => GetScriptPath("portal", path);
    protected string GetReactorScriptPath(string path) => GetScriptPath("reactor", path);
    protected string GetMapScriptPath(string path) => GetScriptPath("map", path);
}
