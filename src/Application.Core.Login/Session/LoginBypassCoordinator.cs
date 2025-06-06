/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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


using Application.Utility.Configs;
using System.Collections.Concurrent;

namespace Application.Core.Login.Session;

/**
 * @author Ronan
 */
public class LoginBypassCoordinator
{

    readonly MasterServer _server;

    private ConcurrentDictionary<HwidAccountPair, KeyValuePair<bool, long>> loginBypass = new();   // optimized PIN & PIC check

    public LoginBypassCoordinator(MasterServer server)
    {
        _server = server;
    }

    public bool canLoginBypass(Hwid hwid, int accId, bool pic)
    {
        try
        {
            var entry = new HwidAccountPair(hwid, accId);
            bool p = loginBypass.GetValueOrDefault(entry).Key;

            return !pic || p;
        }
        catch (NullReferenceException)
        {
            return false;
        }
    }

    public void registerLoginBypassEntry(Hwid hwid, int accId, bool pic)
    {
        long expireTime = (pic ? YamlConfig.config.server.BYPASS_PIC_EXPIRATION : YamlConfig.config.server.BYPASS_PIN_EXPIRATION);
        if (expireTime > 0)
        {
            var entry = new HwidAccountPair(hwid, accId);
            expireTime = _server.getCurrentTime() + 60 * 1000 * (expireTime);
            try
            {
                var value = loginBypass.GetValueOrDefault(entry);
                pic |= value.Key;
                expireTime = Math.Max(value.Value, expireTime);
            }
            catch (NullReferenceException)
            {
            }

            loginBypass[entry] = new(pic, expireTime);
        }
    }

    public void unregisterLoginBypassEntry(Hwid? hwid, int accId)
    {
        if (hwid == null)
            return;

        var entry = new HwidAccountPair(hwid, accId);
        loginBypass.TryRemove(entry, out _);
    }

    public void runUpdateLoginBypass()
    {
        if (loginBypass.Count > 0)
        {
            List<HwidAccountPair> toRemove = new();
            HashSet<int> onlineAccounts = new();
            long timeNow = _server.getCurrentTime();

            //foreach (var w in _server.getWorlds())
            //{
            //    foreach (var chr in w.getPlayerStorage().GetAllOnlinedPlayers())
            //    {
            //        var c = chr.getClient();
            //        if (c != null)
            //        {
            //            onlineAccounts.Add(c.AccountEntity.Id);
            //        }
            //    }
            //}

            foreach (var e in loginBypass)
            {
                if (onlineAccounts.Contains(e.Key.AccountId))
                {
                    long expireTime = timeNow + 60 * 1000 * 2;
                    if (expireTime > e.Value.Value)
                    {
                        loginBypass[e.Key] = new(e.Value.Key, expireTime);
                    }
                }
                else if (e.Value.Value < timeNow)
                {
                    toRemove.Add(e.Key);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var p in toRemove)
                {
                    loginBypass.TryRemove(p, out _);
                }
            }
        }
    }

}
