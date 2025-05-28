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

using Application.Utility.Compatible;
using Application.Utility.Configs;
using System.Collections.Concurrent;

namespace Application.Core.Login.Session;

/**
 * @author Ronan
 */
public class LoginStorage
{
    private ConcurrentDictionary<int, List<DateTimeOffset>> loginHistory = new(); // Key: accountId
    readonly MasterServer _server;

    public LoginStorage(MasterServer server)
    {
        _server = server;
    }

    public bool registerLogin(int accountId)
    {
        List<DateTimeOffset> attempts = loginHistory.GetOrAdd(accountId, k => new());

        lock (attempts)
        {
            DateTimeOffset attemptExpiry = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime() + YamlConfig.config.server.LOGIN_ATTEMPT_DURATION);

            if (attempts.Count > YamlConfig.config.server.MAX_ACCOUNT_LOGIN_ATTEMPT)
            {
                Collections.fill(attempts, attemptExpiry);
                return false;
            }

            attempts.Add(attemptExpiry);
            return true;
        }
    }

    public void clearExpiredAttempts()
    {
        DateTimeOffset now = DateTimeOffset.FromUnixTimeMilliseconds(_server.getCurrentTime());
        List<int> accountIdsToClear = new();

        foreach (var loginEntries in loginHistory)
        {
            List<DateTimeOffset> attempts = loginEntries.Value;
            lock (attempts)
            {
                List<DateTimeOffset> attemptsToRemove = attempts
                        .Where(attempt => attempt < now)
                        .ToList();

                foreach (var attemptToRemove in attemptsToRemove)
                {
                    attempts.Remove(attemptToRemove);
                }

                if (attempts.Count == 0)
                {
                    accountIdsToClear.Add(loginEntries.Key);
                }
            }
        }

        foreach (int accountId in accountIdsToClear)
        {
            loginHistory.TryRemove(accountId, out _);
        }
    }
}
