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
using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Life;
using Application.Core.Game.Skills;
using Application.Core.Managers;
using Application.Core.model;
using Microsoft.EntityFrameworkCore;
using server;
using server.quest;
using System.Diagnostics;
using static server.CashShop;

namespace net.server;

public class Server
{
    private static ILogger log = LogFactory.GetLogger(LogType.Server);
    private static Lazy<Server> instance = new Lazy<Server>(new Server());

    public static Server getInstance() => instance.Value;


    private volatile bool availableDeveloperRoom = false;
    public bool IsOnline { get; set; }
    public static DateTimeOffset uptime = DateTimeOffset.UtcNow;
    ReaderWriterLockSlim lgnLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Server()
    {
    }

    public void setAvailableDeveloperRoom()
    {
        availableDeveloperRoom = true;
    }

    public bool canEnterDeveloperRoom()
    {
        return availableDeveloperRoom;
    }



    bool basedCached = false;
    private void Initialize(bool ignoreCache = false)
    {
        if (!ignoreCache && basedCached)
            return;

        if (!Directory.Exists(ScriptResFactory.ScriptDirName) || !Directory.Exists(WZFiles.DIRECTORY))
            throw new DirectoryNotFoundException();

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SkillFactory.LoadAllSkills();
            sw.Stop();
            log.Debug("WZ - 技能加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Quest.loadAllQuests();
            sw.Stop();
            log.Debug("WZ - 任务加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        OpcodeConstants.generateOpcodeNames();

        basedCached = true;
    }

    public bool IsStarting { get; set; }
    public async Task Start(bool ignoreCache = false)
    {
        if (IsStarting || IsOnline)
            return;

        IsStarting = true;
        log.Information("服务器配置加载中...");

        Stopwatch totalSw = new Stopwatch();
        totalSw.Start();

        try
        {
            Initialize(ignoreCache);

            IsOnline = true;

            totalSw.Stop();
            log.Information("服务器配置加载完成，耗时 {StartupCost}s.", totalSw.Elapsed.TotalSeconds);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            log.Error(ex, "服务器配置加载失败");
            throw;
        }
        finally
        {
            IsStarting = false;
        }
    }

    //public bool isGmOnline(int world)
    //{
    //    return getWorld(world).Players.GetAllOnlinedPlayers().Any(x => x.isGM());
    //}


    public void Reset()
    {
        instance = new Lazy<Server>(new Server());
    }

    //synchronized
    public async Task Stop(bool restart, bool force = false)
    {
        if (!IsOnline)
        {
            log.Warning("不能停止未启动的服务");
            return;
        }

        ThreadManager.getInstance().stop();

        IsOnline = false;
        if (force)
            basedCached = false;

        log.Information("Worlds and channels are offline.");
        if (restart)
        {
            log.Information("Restarting the server...");
            Reset();
            await getInstance().Start();//DID I DO EVERYTHING?! D:
        }
    }
}
