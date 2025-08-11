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


using Application.Core.net.server.coordinator.matchchecker.listener;

namespace net.server.coordinator.matchchecker;

public enum MatchCheckerType
{
    GUILD_CREATION,
    CPQ_CHALLENGE
}

public static class MatchCheckerTypeUtils
{
    public static AbstractMatchCheckerListener getListener(this MatchCheckerType type)
    {
        if (type == MatchCheckerType.GUILD_CREATION)
            return MatchCheckerStaticFactory.Context.matchCheckerGuildCreationListener;
        if (type == MatchCheckerType.CPQ_CHALLENGE)
            return MatchCheckerStaticFactory.Context.matchCheckerCPQChallengeListener;

        throw new Exception();
    }
}

public class MatchCheckerStaticFactory
{
    public static MatchCheckerStaticFactory Context { get; set; }
    public MatchCheckerGuildCreationListener matchCheckerGuildCreationListener { get; }
    public MatchCheckerCPQChallengeListener matchCheckerCPQChallengeListener { get; }

    public MatchCheckerStaticFactory(MatchCheckerGuildCreationListener matchCheckerGuildCreationListener, MatchCheckerCPQChallengeListener matchCheckerCPQChallengeListener)
    {
        this.matchCheckerGuildCreationListener = matchCheckerGuildCreationListener;
        this.matchCheckerCPQChallengeListener = matchCheckerCPQChallengeListener;
    }

}