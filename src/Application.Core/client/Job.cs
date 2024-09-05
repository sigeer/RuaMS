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
using constants.skills;

namespace client;

public enum Job
{

    BEGINNER = 0,

    WARRIOR = 100,
    FIGHTER = 110, CRUSADER = 111, HERO = 112,
    PAGE = 120, WHITEKNIGHT = 121, PALADIN = 122,
    SPEARMAN = 130, DRAGONKNIGHT = 131, DARKKNIGHT = 132,

    MAGICIAN = 200,
    FP_WIZARD = 210, FP_MAGE = 211, FP_ARCHMAGE = 212,
    IL_WIZARD = 220, IL_MAGE = 221, IL_ARCHMAGE = 222,
    CLERIC = 230, PRIEST = 231, BISHOP = 232,

    BOWMAN = 300,
    HUNTER = 310, RANGER = 311, BOWMASTER = 312,
    CROSSBOWMAN = 320, SNIPER = 321, MARKSMAN = 322,

    THIEF = 400,
    ASSASSIN = 410, HERMIT = 411, NIGHTLORD = 412,
    BANDIT = 420, CHIEFBANDIT = 421, SHADOWER = 422,

    PIRATE = 500,
    BRAWLER = 510, MARAUDER = 511, BUCCANEER = 512,
    GUNSLINGER = 520, OUTLAW = 521, CORSAIR = 522,

    MAPLELEAF_BRIGADIER = 800,
    GM = 900, SUPERGM = 910,

    NOBLESSE = 1000,
    DAWNWARRIOR1 = 1100, DAWNWARRIOR2 = 1110, DAWNWARRIOR3 = 1111, DAWNWARRIOR4 = 1112,
    BLAZEWIZARD1 = 1200, BLAZEWIZARD2 = 1210, BLAZEWIZARD3 = 1211, BLAZEWIZARD4 = 1212,
    WINDARCHER1 = 1300, WINDARCHER2 = 1310, WINDARCHER3 = 1311, WINDARCHER4 = 1312,
    NIGHTWALKER1 = 1400, NIGHTWALKER2 = 1410, NIGHTWALKER3 = 1411, NIGHTWALKER4 = 1412,
    THUNDERBREAKER1 = 1500, THUNDERBREAKER2 = 1510, THUNDERBREAKER3 = 1511, THUNDERBREAKER4 = 1512,

    LEGEND = 2000, EVAN = 2001,
    ARAN1 = 2100, ARAN2 = 2110, ARAN3 = 2111, ARAN4 = 2112,

    EVAN1 = 2200, EVAN2 = 2210, EVAN3 = 2211, EVAN4 = 2212, EVAN5 = 2213, EVAN6 = 2214,
    EVAN7 = 2215, EVAN8 = 2216, EVAN9 = 2217, EVAN10 = 2218
}

public static class JobUtils
{
    public static int getMax()
    {
        return 22;
    }

    public static Job getById(int id)
    {
        return (Job)(id);
    }

    public static Job getBy5ByteEncoding(int encoded)
    {
        switch (encoded)
        {
            case 2:
                return Job.WARRIOR;
            case 4:
                return Job.MAGICIAN;
            case 8:
                return Job.BOWMAN;
            case 16:
                return Job.THIEF;
            case 32:
                return Job.PIRATE;
            case 1024:
                return Job.NOBLESSE;
            case 2048:
                return Job.DAWNWARRIOR1;
            case 4096:
                return Job.BLAZEWIZARD1;
            case 8192:
                return Job.WINDARCHER1;
            case 16384:
                return Job.NIGHTWALKER1;
            case 32768:
                return Job.THUNDERBREAKER1;
            default:
                return Job.BEGINNER;
        }
    }

    public static int getJobNiche(this int jobid)
    {
        return (jobid / 100) % 10;

        /*
        case 0: BEGINNER;
        case 1: WARRIOR;
        case 2: MAGICIAN;
        case 3: BOWMAN;  
        case 4: THIEF;
        case 5: PIRATE;
        */
    }
    public static int getJobNiche(this Job jobid)
    {
        return ((int)jobid / 100) % 10;

        /*
        case 0: BEGINNER;
        case 1: WARRIOR;
        case 2: MAGICIAN;
        case 3: BOWMAN;  
        case 4: THIEF;
        case 5: PIRATE;
        */
    }

    public static int getJobMapChair(this Job job)
    {
        switch (job.getId() / 1000)
        {
            case 0:
                return Beginner.MAP_CHAIR;
            case 1:
                return Noblesse.MAP_CHAIR;
            default:
                return Legend.MAP_CHAIR;
        }
    }
}
public static class JobExtensions
{
    public static int getId(this Job m)
    {
        return (int)m;
    }

    public static bool isA(this Job thisJob, Job basejob)
    {  
        // thanks Steve (kaito1410) for pointing out an improvement here
        int basebranch = basejob.getId() / 10;
        return (thisJob.getId() / 10 == basebranch && thisJob.getId() >= basejob.getId()) || (basebranch % 10 == 0 && thisJob.getId() / 100 == basejob.getId() / 100);
    }


    public static bool IsBeginningJob(this Job job)
    {
        return (job.getId() == 0 || job.getId() == 1000 || job.getId() == 2000);
    }
}