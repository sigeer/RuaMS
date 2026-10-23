using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Aran
{
    internal sealed class AranPortalHandler : PortalPlayerInteraction
    {
        public AranPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 914000000 | pt: 7
        public async Task<bool> aranTutorAloneX()
        {

            await playPortalSound();
            await warp(914000100, 1);
            return true;
        }

        // Map: 914000100 | pt: 9
        public async Task<bool> aranTutorArrow0()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr0=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "arr0=o;mo1=o;mo2=o;mo3=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            return true;
        }

        // Map: 914000200 | pt: 9
        public async Task<bool> aranTutorArrow1()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr1=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;mo1=o;mo2=o;mo3=o;mo4=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return true;
        }

        // Map: 914000210 | pt: 9
        public async Task<bool> aranTutorArrow2()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr2=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;arr2=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return true;
        }

        // Map: 914000220 | pt: 9
        public async Task<bool> aranTutorArrow3()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "arr3=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;arr3=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            return true;
        }

        // Map: 914000200 | pt: 9
        public async Task<bool> aranTutorGuide0()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "normal=o"))
            {
                return false;
            }
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide1");
            await Pink("按一下Ctrl键，能够对怪兽进行一般攻击。");
            await updateAreaInfo(21002, "normal=o;arr0=o;mo1=o;mo2=o;mo3=o");
            return true;
        }

        // Map: 914000210 | pt: 9
        public async Task<bool> aranTutorGuide1()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "chain=o"))
            {
                return false;
            }
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide2");
            await Pink("按住Ctrl键，能够进行连续攻击。");
            await updateAreaInfo(21002, "normal=o;arr0=o;arr1=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            return true;
        }

        // Map: 914000220 | pt: 9
        public async Task<bool> aranTutorGuide2()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "cmd=o"))
            {
                return false;
            }
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialGuide3");
            await Pink("连续攻击后，通过方向键和攻击键可以实现命令攻击。");
            await updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            return true;
        }

        // Map: 914000300 | pt: 9
        public async Task<bool> aranTutorLost()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "fin=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "cmd=o;normal=o;arr0=o;arr1=o;arr2=o;arr3=o;fin=o;mo1=o;chain=o;mo2=o;mo3=o;mo4=o");
            await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/Child");
            await ShowDirectionEffect("Effect/Direction1.img/aranTutorial/ClickChild");
            return true;
        }

        // Map: 914000000 | pt: 9
        public async Task<bool> aranTutorMono0()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo1=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "mo1=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon1");
            return true;
        }

        // Map: 914000000 | pt: 9
        public async Task<bool> aranTutorMono1()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo2=o"))
            {
                return false;
            }
            await playSound("Aran/balloon");
            await updateAreaInfo(21002, "mo1=o;mo2=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon2");
            return true;
        }

        // Map: 914000000 | pt: 9
        public async Task<bool> aranTutorMono2()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo3=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "mo1=o;mo2=o;mo3=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon3");
            return true;
        }

        // Map: 914000200 | pt: 9
        public async Task<bool> aranTutorMono3()
        {

            await blockPortal();
            if (containsAreaInfo(21002, "mo4=o"))
            {
                return false;
            }
            await updateAreaInfo(21002, "normal=o;arr0=o;mo1=o;mo2=o;mo3=o;mo4=o");
            await ShowEffect("Effect/OnUserEff.img/guideEffect/aranTutorial/legendBalloon6");
            return true;
        }

        // Map: 914000100 | pt: 7
        public async Task<bool> aranTutorOut1()
        {

            if (isQuestStarted(21000))
            {
                //lol nexon does this xD
                await teachSkill(20000017, 0, -1, -1);
                await teachSkill(20000018, 0, -1, -1);
                //nexon sends updatePlayerStats Stat.AVAILABLESP 0
                await teachSkill(20000017, 1, 0, -1);
                await teachSkill(20000018, 1, 0, -1);
                //actually nexon does enableActions here :P
                await playPortalSound();
                await warp(914000200, 1);
                return true;
            }
            else
            {
                await Pink("你只有在接受你右边的雅典娜·皮尔斯的任务后才能退出。");
                return false;
            }
        }

        // Map: 914000200 | pt: 7
        public async Task<bool> aranTutorOut2()
        {

            //lol nexon does this xD
            await teachSkill(20000014, 0, -1, -1);
            await teachSkill(20000015, 0, -1, -1);
            //nexon sends updatePlayerStats Stat.AVAILABLESP 0
            await teachSkill(20000014, 1, 0, -1);
            await teachSkill(20000015, 1, 0, -1);
            //actually nexon does enableActions here :P
            await playPortalSound();
            await warp(914000210, 1);
            return true;
        }

        // Map: 914000210 | pt: 7
        public async Task<bool> aranTutorOut3()
        {

            //lol nexon does this xD
            await teachSkill(20000016, 0, -1, -1);
            //nexon sends updatePlayerStats Stat.AVAILABLESP 0
            await teachSkill(20000016, 1, 0, -1);
            //actually nexon does enableActions here :P
            await playPortalSound();
            await warp(914000220, 1);
            return true;
        }
    }
}
