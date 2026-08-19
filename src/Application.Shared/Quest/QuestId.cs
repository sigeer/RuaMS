using Application.Shared.Constants.Job;

namespace Application.Shared.Quest
{
    /// <summary>
    /// 自定义任务Id，不在quest.wz中，仅用作记录状态
    /// 5xxxxx
    /// </summary>
    public class QuestId
    {
        public const int Warrior2nd0 = 512000;

        public const int Warrior3rd0 = 513000;
        /// <summary>
        /// 力量测试
        /// </summary>
        public const int Warrior3rd1 = 513001;
        /// <summary>
        /// 智慧测试
        /// </summary>
        public const int Warrior3rd2 = 513002;

        public const int Magician2nd0 = 522000;


        public const int Archer2nd0 = 532000;


        public const int Thief2nd0 = 542000;


        public const int Pirate3rd0 = 553000;


        public const int ZakumBattle = 100200;

        /// <summary>
        /// 根据职业2转的自定义任务id
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static int Get2ndJobQuest(Job job)
        {
            return 500000 + job.GetJobNiche() * 10000 + 2000;
        }

        public static int Get3rdJobQuest(Job job)
        {
            return 500000 + job.GetJobNiche() * 10000 + 3000;
        }
    }
}
