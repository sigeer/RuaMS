using Application.Utility;
using Application.Utility.Exceptions;

namespace Application.Shared.Constants
{
    public class Job : EnumClass
    {
        public int Id { get; }
        public string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        public int Rank { get; }
        public int MaxLevel { get; }
        /// <summary>
        /// 职业群
        /// </summary>
        public JobType Type { get; }

        public static Job BEGINNER = new Job(JobId.BEGINNER, "新手");

        public static Job WARRIOR = new Job(JobId.WARRIOR, "战士");
        public static Job FIGHTER = new Job(JobId.FIGHTER, "剑客");
        public static Job CRUSADER = new Job(JobId.CRUSADER, "勇士");
        public static Job HERO = new Job(JobId.HERO, "英雄");

        public static Job PAGE = new Job(JobId.PAGE, "准骑士");
        public static Job WHITEKNIGHT = new Job(JobId.WHITEKNIGHT, "骑士");
        public static Job PALADIN = new Job(JobId.PALADIN, "圣骑士");

        public static Job SPEARMAN = new Job(JobId.SPEARMAN, "枪战士");
        public static Job DRAGONKNIGHT = new Job(JobId.DRAGONKNIGHT, "龙骑士");
        public static Job DARKKNIGHT = new Job(JobId.DARKKNIGHT, "黑骑士");


        public static Job MAGICIAN = new Job(JobId.MAGICIAN, "魔法师");
        public static Job FP_WIZARD = new Job(JobId.FP_WIZARD, "法师（火毒）");
        public static Job FP_MAGE = new Job(JobId.FP_MAGE, "巫师（火毒）");
        public static Job FP_ARCHMAGE = new Job(JobId.FP_ARCHMAGE, "魔导师（火毒）");

        public static Job IL_WIZARD = new Job(JobId.IL_WIZARD, "法师（冰雷）");
        public static Job IL_MAGE = new Job(JobId.IL_MAGE, "巫师（冰雷）");
        public static Job IL_ARCHMAGE = new Job(JobId.IL_ARCHMAGE, "魔导师（冰雷）");

        public static Job CLERIC = new Job(JobId.CLERIC, "牧师");
        public static Job PRIEST = new Job(JobId.PRIEST, "祭司");
        public static Job BISHOP = new Job(JobId.BISHOP, "主教");


        public static Job BOWMAN = new Job(JobId.BOWMAN, "弓箭手");
        public static Job HUNTER = new Job(JobId.HUNTER, "猎人");
        public static Job RANGER = new Job(JobId.RANGER, "射手");
        public static Job BOWMASTER = new Job(JobId.BOWMASTER, "神射手");

        public static Job CROSSBOWMAN = new Job(JobId.CROSSBOWMAN, "弩弓手");
        public static Job SNIPER = new Job(JobId.SNIPER, "游侠");
        public static Job MARKSMAN = new Job(JobId.MARKSMAN, "箭神");


        public static Job THIEF = new Job(JobId.THIEF, "飞侠");
        public static Job ASSASSIN = new Job(JobId.ASSASSIN, "刺客");
        public static Job HERMIT = new Job(JobId.HERMIT, "无影人");
        public static Job NIGHTLORD = new Job(JobId.NIGHTLORD, "隐士");

        public static Job BANDIT = new Job(JobId.BANDIT, "侠客");
        public static Job CHIEFBANDIT = new Job(JobId.CHIEFBANDIT, "独行客");
        public static Job SHADOWER = new Job(JobId.SHADOWER, "侠盗");


        public static Job PIRATE = new Job(JobId.PIRATE, "海盗");
        public static Job BRAWLER = new Job(JobId.BRAWLER, "拳手");
        public static Job MARAUDER = new Job(JobId.MARAUDER, "斗士");
        public static Job BUCCANEER = new Job(JobId.BUCCANEER, "冲锋队长");

        public static Job GUNSLINGER = new Job(JobId.GUNSLINGER, "火枪手");
        public static Job OUTLAW = new Job(JobId.OUTLAW, "大副");
        public static Job CORSAIR = new Job(JobId.CORSAIR, "船长");


        public static Job MAPLELEAF_BRIGADIER = new Job(JobId.MAPLELEAF_BRIGADIER, "巡查员");
        public static Job GM = new Job(JobId.GM, "管理员");
        public static Job SUPERGM = new Job(JobId.SUPERGM, "超级管理员");



        public static Job NOBLESSE = new Job(JobId.NOBLESSE, "初心者");

        public static Job DAWNWARRIOR1 = new Job(JobId.DAWNWARRIOR1, "魂骑士(1转)");
        public static Job DAWNWARRIOR2 = new Job(JobId.DAWNWARRIOR2, "魂骑士(2转)");
        public static Job DAWNWARRIOR3 = new Job(JobId.DAWNWARRIOR3, "魂骑士(3转)");
        public static Job DAWNWARRIOR4 = new Job(JobId.DAWNWARRIOR4, "魂骑士(4转)");

        public static Job BLAZEWIZARD1 = new Job(JobId.BLAZEWIZARD1, "炎术士(1转)");
        public static Job BLAZEWIZARD2 = new Job(JobId.BLAZEWIZARD2, "炎术士(2转)");
        public static Job BLAZEWIZARD3 = new Job(JobId.BLAZEWIZARD3, "炎术士(3转)");
        public static Job BLAZEWIZARD4 = new Job(JobId.BLAZEWIZARD4, "炎术士(4转)");

        public static Job WINDARCHER1 = new Job(JobId.WINDARCHER1, "风灵使者(1转)");
        public static Job WINDARCHER2 = new Job(JobId.WINDARCHER2, "风灵使者(2转)");
        public static Job WINDARCHER3 = new Job(JobId.WINDARCHER3, "风灵使者(3转)");
        public static Job WINDARCHER4 = new Job(JobId.WINDARCHER4, "风灵使者(4转)");

        public static Job NIGHTWALKER1 = new Job(JobId.NIGHTWALKER1, "夜行者(1转)");
        public static Job NIGHTWALKER2 = new Job(JobId.NIGHTWALKER2, "夜行者(2转)");
        public static Job NIGHTWALKER3 = new Job(JobId.NIGHTWALKER3, "夜行者(3转)");
        public static Job NIGHTWALKER4 = new Job(JobId.NIGHTWALKER4, "夜行者(4转)");

        public static Job THUNDERBREAKER1 = new Job(JobId.THUNDERBREAKER1, "奇袭者(1转)");
        public static Job THUNDERBREAKER2 = new Job(JobId.THUNDERBREAKER2, "奇袭者(2转)");
        public static Job THUNDERBREAKER3 = new Job(JobId.THUNDERBREAKER3, "奇袭者(3转)");
        public static Job THUNDERBREAKER4 = new Job(JobId.THUNDERBREAKER4, "奇袭者(4转)");


        public static Job LEGEND = new Job(JobId.LEGEND, "战童");
        public static Job EVAN = new Job(JobId.EVAN, "龙初心");

        public static Job ARAN1 = new Job(JobId.ARAN1, "战神(1转)");
        public static Job ARAN2 = new Job(JobId.ARAN2, "战神(2转)");
        public static Job ARAN3 = new Job(JobId.ARAN3, "战神(3转)");
        public static Job ARAN4 = new Job(JobId.ARAN4, "战神(4转)");

        public static Job EVAN1 = new Job(JobId.EVAN1, "龙神1");
        public static Job EVAN2 = new Job(JobId.EVAN2, "龙神2");
        public static Job EVAN3 = new Job(JobId.EVAN3, "龙神3");
        public static Job EVAN4 = new Job(JobId.EVAN4, "龙神4");
        public static Job EVAN5 = new Job(JobId.EVAN5, "龙神5");
        public static Job EVAN6 = new Job(JobId.EVAN6, "龙神6");
        public static Job EVAN7 = new Job(JobId.EVAN7, "龙神7");
        public static Job EVAN8 = new Job(JobId.EVAN8, "龙神8");
        public static Job EVAN9 = new Job(JobId.EVAN9, "龙神9");
        public static Job EVAN10 = new Job(JobId.EVAN10, "龙神10");

        public bool HasSPTable { get; }
        private Job(int value, string name)
        {
            Id = value;
            Name = name;

            HasSPTable = IsEvan();

            Type = (JobType)(Id / 1000);

            if (Id % 1000 == 0)
            {
                Rank = 0;
            }
            else if (Id % 100 == 0)
            {
                Rank = 1;
            }
            else
            {
                Rank = 2 + (Id % 10);
            }


            switch (Rank)
            {
                case 0:
                    MaxLevel = 10;   // beginner
                    break;
                case 1:
                    MaxLevel = 30;   // 1st job
                    break;
                case 2:
                    MaxLevel = 70;   // 2nd job
                    break;
                case 3:
                    MaxLevel = 120;   // 3rd job
                    break;
                default:
                    MaxLevel = Type == JobType.Cygnus ? 120 : 200;
                    break;
            }
        }

        public int getId()
        {
            return Id;
        }

        public int GetJobNiche()
        {
            return (Id / 100) % 10;
        }

        public bool IsBeginningJob()
        {
            return Id == JobId.BEGINNER || Id == JobId.NOBLESSE || Id == JobId.LEGEND;
        }

        public bool isA(Job basejob)
        {
            // thanks Steve (kaito1410) for pointing out an improvement here
            int basebranch = basejob.getId() / 10;
            return (getId() / 10 == basebranch && getId() >= basejob.getId()) || (basebranch % 10 == 0 && getId() / 100 == basejob.getId() / 100);
        }

        public bool IsGmJob()
        {
            return Id == JobId.GM || Id == JobId.SUPERGM || Id == JobId.MAPLELEAF_BRIGADIER;
        }

        public bool IsEvan()
        {
            return Id == JobId.EVAN || HasDragon();
        }

        public bool HasDragon()
        {
            return Id >= JobId.EVAN1 && Id <= JobId.EVAN10;
        }

        public override string ToString()
        {
            return Name;
        }

        public static explicit operator int(Job v)
        {
            return v.getId();
        }
    }
}
