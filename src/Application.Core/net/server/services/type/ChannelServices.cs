/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
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


using net.server.services.task.channel;

namespace net.server.services.type;

/**
 * @author Ronan
 */
public class ChannelServices : EnumClass, ServiceType<ChannelServices>
{
    public static readonly ChannelServices MOB_STATUS = new ChannelServices(typeof(MobStatusService));
    public static readonly ChannelServices MOB_ANIMATION = new ChannelServices(typeof(MobAnimationService));
    public static readonly ChannelServices MOB_CLEAR_SKILL = new ChannelServices(typeof(MobClearSkillService));
    public static readonly ChannelServices MOB_MIST = new ChannelServices(typeof(MobMistService));
    public static readonly ChannelServices EVENT = new ChannelServices(typeof(EventService));
    public static readonly ChannelServices OVERALL = new ChannelServices(typeof(OverallService));

    private Type s;

    private ChannelServices(Type service)
    {
        s = service;
    }

    public Service createService()
    {
        return new Service(s);
    }

    public ChannelServices[] enumValues()
    {
        return GetValues<ChannelServices>().ToArray();
    }

    int ServiceType<ChannelServices>.ordinal()
    {
        return ordinal();
    }
}

