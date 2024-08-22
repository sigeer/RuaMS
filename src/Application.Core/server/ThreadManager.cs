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

namespace server;

/**
 * @author Ronan
 */
public class ThreadManager
{
    private static ThreadManager instance = new ThreadManager();

    public static ThreadManager getInstance()
    {
        return instance;
    }

    private ThreadManager() { }


    private CancellationTokenSource _cts = new CancellationTokenSource();
    private CancellationToken CancellationToken => _cts.Token;
    public void newTask(AbstractRunnable r)
    {
        Task.Run(r.run, CancellationToken).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                // Handle exception
                Log.Logger.Error(t.Exception.ToString());
            }
        });
    }

    public void newTask(Action r)
    {
        Task.Run(r, CancellationToken).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                // Handle exception
                Log.Logger.Error(t.Exception.ToString());
            }
        });
    }

    public void start()
    {

    }

    public void stop()
    {
        _cts.Cancel();
    }

}
