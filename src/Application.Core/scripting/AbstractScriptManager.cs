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


using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Commands;
using Application.Core.Game.Life;
using Application.Core.Managers;
using Application.Scripting.JS;
using Application.Scripting.Lua;
using Application.Shared.Events;
using client;
using client.inventory;
using client.inventory.manipulator;
using constants.game;
using Microsoft.Extensions.Logging;
using server.life;
using System.Collections.Concurrent;
using tools;

namespace scripting;

/**
 * @author Matze
 */
public abstract class AbstractScriptManager
{
    static ConcurrentDictionary<string, ScriptMeta?> JsCache { get; set; } = new();
    protected readonly ILogger<AbstractScriptManager> _logger;
    protected readonly CommandExecutor _commandExecutor;
    protected readonly WorldChannel _channelServer;
    readonly IEnumerable<IAddtionalRegistry> _addtionalRegistries;

    static readonly ScriptFile JsUtil = new ScriptFile("", "utils", ScriptType.Js);

    protected AbstractScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel, IEnumerable<IAddtionalRegistry> addtionalRegistries)
    {
        _logger = logger;
        _commandExecutor = commandExecutor;
        _channelServer = worldChannel;
        _addtionalRegistries = addtionalRegistries;
    }

    protected ScriptMeta? GetScriptMeta(ScriptFile file)
    {
        return JsCache.GetOrAdd(file.CacheKey, (key) =>
        {
            string? fileContent;
            var fullPath = GetFullScriptPath(Path.Combine(file.Category, file.FileName));
            try
            {
                fileContent = File.ReadAllText(fullPath);
            }
            catch (FileNotFoundException)
            {
                file.ToggleType();
                fullPath = GetFullScriptPath(Path.Combine(file.Category, file.FileName));

                try
                {
                    fileContent = File.ReadAllText(fullPath);
                }
                catch (FileNotFoundException)
                {
                    _logger.LogWarning("{Category}脚本{Name}没有找到。", file.Category, file.Name);
                    return null;
                }
            }

            return new ScriptMeta(file, file.Type == ScriptType.Js ? JintEngine.Prepare(fileContent) : NLuaScriptEngine.Prepare(fileContent), fullPath);
        });
    }

    private IEngine getInvocableScriptEngine(ScriptMeta? jsContent)
    {
        if (jsContent == null)
        {
            throw new BusinessException($"脚本不存在");
        }

        IEngine engine;
        if (jsContent.ScriptFile.Type == ScriptType.Js)
            engine = new JintEngine();
        else if (jsContent.ScriptFile.Type == ScriptType.Lua)
            engine = new NLuaScriptEngine();
        else
        {
            throw new BusinessException($"不支持的脚本类型：{jsContent.ScriptFile.Type}");
        }

        try
        {
            InitializeScriptType(engine);

            engine.Evaluate(jsContent.PreparedValue);
            return engine;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File: {ScriptPath}", jsContent.FullPath);
            throw;
        }
    }

    protected IEngine getInvocableScriptEngine(ScriptFile file)
    {
        return getInvocableScriptEngine(GetScriptMeta(file));
    }

    void InitializeScriptType(IEngine engine)
    {
        engine.AddHostedObject("LifeFactory", LifeFactory.Instance);
        engine.AddHostedType("DropItemEntry", typeof(DropItemEntry));
        engine.AddHostedType("YamlConfig", typeof(YamlConfig));
        engine.AddHostedType("PacketCreator", typeof(PacketCreator));
        engine.AddHostedType("Point", typeof(Point));
        engine.AddHostedType("RandomPoint", typeof(RandomPoint));

        foreach (var item in _addtionalRegistries)
        {
            item.AddHostedObject(engine);
            item.AddHostedType(engine);
        }

        if (engine is JintEngine js)
        {
            engine.AddHostedType("Item", typeof(Item));
            engine.AddHostedType("InventoryManipulator", typeof(InventoryManipulator));
            engine.AddHostedType("BuffStat", typeof(BuffStat));
            engine.AddHostedType("MapId", typeof(MapId));
            engine.AddHostedType("Rectangle", typeof(Rectangle));
            engine.AddHostedType("CommonManager", typeof(CommonManager));
            engine.AddHostedObject("CommandExecutor", _commandExecutor);
            engine.AddHostedType("CharacterManager", typeof(CharacterManager));
            engine.AddHostedObject("GachaponManager", _channelServer.NodeService.GachaponManager);
            engine.AddHostedType("ItemInformationProvider", typeof(ItemInformationProvider));
            engine.AddHostedType("MonsterBook", typeof(MonsterBook));
            engine.AddHostedType("ExpTable", typeof(ExpTable));
            engine.AddHostedType("ExpeditionType", typeof(ExpeditionType));
            engine.AddHostedType("GameConstants", typeof(GameConstants));
            engine.AddHostedObject("PlayerNPC", _channelServer.NodeService.PlayerNPCService);
            engine.AddHostedType("Guild", typeof(Application.Core.Managers.GuildManager));
            engine.AddHostedType("Job", typeof(Job));
            engine.AddHostedType("InventoryType", typeof(InventoryType));

            var jsUtils = GetScriptMeta(JsUtil);
            if (jsUtils != null)
            {
                engine.Evaluate(jsUtils.PreparedValue);
            }
        }
    }

    protected IEngine? getInvocableScriptEngine(ScriptMeta? meta, IChannelClient c)
    {
        if (meta == null)
            return null;

        var engine = c.ScriptEngines[meta.ScriptFile.CacheKey];
        if (engine == null)
        {
            engine = getInvocableScriptEngine(meta);
            c.ScriptEngines[meta.ScriptFile.CacheKey] = engine;
        }

        return engine;
    }

    protected IEngine? getInvocableScriptEngine(ScriptFile file, IChannelClient c)
    {
        return getInvocableScriptEngine(GetScriptMeta(file), c);
    }

    public static void ClearCache()
    {
        JsCache.Clear();
    }

    protected void resetContext(ScriptFile file, IChannelClient c)
    {
        c.ScriptEngines.Remove(file.CacheKey);
    }

    protected string GetFullScriptPath(string relativePath)
    {
        return ScriptSource.Instance.GetScriptFullPath(relativePath);
    }

    protected ScriptFile GetNpcScriptPath(string path) => new ScriptFile("npc", path, ScriptSource.Instance.DefaultScriptType);
    protected ScriptFile GetItemScriptPath(string path) => new ScriptFile("item", path, ScriptSource.Instance.DefaultScriptType);
    protected ScriptFile GetQuestScriptPath(string path) => new ScriptFile("quest", path, ScriptSource.Instance.DefaultScriptType);
    protected ScriptFile GetEventScriptPath(string path) => new ScriptFile(ScriptSource.Instance.EventDirName, path, ScriptSource.Instance.DefaultScriptType);
    protected ScriptFile GetPortalScriptPath(string path) => new ScriptFile("portal", path, ScriptSource.Instance.DefaultScriptType);
    protected ScriptFile GetReactorScriptPath(string path) => new ScriptFile("reactor", path, ScriptSource.Instance.DefaultScriptType);
    protected ScriptFile GetMapScriptPath(string path) => new ScriptFile("map", path, ScriptSource.Instance.DefaultScriptType);
}
