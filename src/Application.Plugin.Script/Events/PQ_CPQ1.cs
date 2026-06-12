using Application.Core.Game.Players;
using Application.Core.model;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{
    internal sealed class PQ_CPQ1 : AbstractMonsterCarnivalEventTemplate
    {
        public PQ_CPQ1(string name, int recruitMap) : base(name, recruitMap + 1)
        {
            RecruitMap = recruitMap;
            EntryMap = recruitMap + 1;

            ExitMap = 980000010;

            MinMap = recruitMap;
            MaxMap = recruitMap + 99;

            MinLevel = 30;
            MaxLevel = 50;

            AllClearRewards = new()
            {
                { 1, new([new (4001129, 1),new (4001129, 2)],[],[50000, 25500, 21000, 19505, 17500, 12000, 5000, 2500] ) }
            };
        }

        public override RewardOptions GetAllClearRewardOptions(Player chr, int point = 1)
        {
            return new RewardOptions(ExpPoolIndex: point);
        }
    }
}
