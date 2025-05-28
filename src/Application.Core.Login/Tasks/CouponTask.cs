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

using Application.Core.Login.Datas;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks;



/// <summary>
///  @author Ronan
///  @info Thread responsible for maintaining coupons EXP & DROP effects active
///  整点开始，每小时执行
/// </summary>
public class CouponTask : AbstractRunnable
{
    readonly MasterServer _server;

    public CouponTask(MasterServer server)
    {
        _server = server;
    }

    public override void HandleRun()
    {
        try
        {
            _server.CouponManager.UpdateActiveCoupons();

            _server.CouponManager.CommitActiveCoupons();
        }
        catch (Exception sqle)
        {
            log.Error(sqle, "Error updating coupon effects");
        }
    }
}
