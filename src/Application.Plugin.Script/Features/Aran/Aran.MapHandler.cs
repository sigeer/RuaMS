using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Shared.Constants.Map;
using scripting.map;

namespace Application.Plugin.Script.Features.Aran
{
    /// <summary>
    /// 地图脚本：Map.wz 的 <c>onUserEnter</c> 与 <c>onFirstUserEnter</c> 共用同一实现，
    /// 两者只是触发时机不同，因此不再区分种类。
    /// </summary>
    internal sealed class AranMapHandler : MapScriptMethods
    {
        public AranMapHandler(IChannelClient c, IMap map) : base(c, map)
        {
        }

        // Map: 130000000
        public async Task startEreb()
        {
            if (getJobId() == 1000 && getLevel() >= 10)
            {
                await unlockUI();
            }

        }

        // Map: 140000000
        public async Task rien()
        {
            if (isQuestCompleted(21101) && containsAreaInfo(21019, "miss=o;arr=o;helper=clear"))
            {
                await updateAreaInfo(21019, "miss=o;arr=o;ck=1;helper=clear");
            }
            await unlockUI();

        }

        // Map: 140010000
        public async Task rienArrow()
        {
            if (containsAreaInfo(21019, "miss=o;helper=clear"))
            {
                await updateAreaInfo(21019, "miss=o;arr=o;helper=clear");
                await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }

        }

        // Map: 140030000
        public async Task mirrorCave()
        {
            // TODO

        }

        // Map: 140090000
        public async Task iceCave()
        {
            await teachSkill(20000014, -1, 0, -1);
            await teachSkill(20000015, -1, 0, -1);
            await teachSkill(20000016, -1, 0, -1);
            await teachSkill(20000017, -1, 0, -1);
            await teachSkill(20000018, -1, 0, -1);
            await unlockUI();
            await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/ClickLilin");

        }

        // Map: 914000000, 914000300, 914000400, 914000410, 914000420, 914000500
        public async Task aranTutorAlone()
        {

        }

        // Map: 914090000, 914090001, 914090002, 914090003, 914090004, 914090005 … (共 17 张)
        public async Task aranDirection()
        {
            string intro = (c.OnlinedCharacter.getMapId()) switch
            {
                MapId.ARAN_TUTO_1 => "Effect/Direction1.img/aranTutorial/Scene0",
                MapId.ARAN_TUTO_2 => "Effect/Direction1.img/aranTutorial/Scene1" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
                MapId.ARAN_TUTO_3 => "Effect/Direction1.img/aranTutorial/Scene2" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
                MapId.ARAN_TUTO_4 => "Effect/Direction1.img/aranTutorial/Scene3",
                MapId.ARAN_POLEARM => "Effect/Direction1.img/aranTutorial/HandedPoleArm" + (c.OnlinedCharacter.getGender() == 0 ? "0" : "1"),
                MapId.ARAN_MAHA => "Effect/Direction1.img/aranTutorial/Maha",
                _ => ""
            };
            await lockUI();
            await ShowDirectionEffect(intro);
        }

    }
}
