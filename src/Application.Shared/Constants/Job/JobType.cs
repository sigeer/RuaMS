using Application.Shared.Constants.Skill;

namespace Application.Shared.Constants.Job
{
    /// <summary>
    /// 职业群
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// 冒险家
        /// </summary>
        Adventurer,
        /// <summary>
        /// 骑士团
        /// </summary>
        Cygnus,
        /// <summary>
        /// 英雄（战神、龙神）
        /// </summary>
        Legend,
    }

    public static class JobTypeExtensions
    {

        /// <summary>
        /// 群宠技能Id，客户端会根据职业群分别判断，所以不能一律用Beginner.FOLLOW_THE_LEADER
        /// </summary>
        /// <returns></returns>
        public static int GetMultiPetSkillId(this JobType type)
        {
            switch (type)
            {
                case JobType.Adventurer:
                    return Beginner.FOLLOW_THE_LEADER;
                case JobType.Cygnus:
                    return Noblesse.FOLLOW_THE_LEADER;
                case JobType.Legend:
                    return Legend.FOLLOW_THE_LEADER;
                default:
                    break;
            }
            return 0;
        }
    }
}
