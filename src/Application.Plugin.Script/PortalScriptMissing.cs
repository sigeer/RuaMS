using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script
{
    /// <summary>
    /// 补齐 Map.wz 中引用、但 JS 与 C# 两侧均无实现的 portal 脚本。
    /// 这些脚本在原始脚本库中没有对应实现，此处的占位方法会抛出
    /// <see cref="NotImplementedException"/>，由 PluginManager 统一记录并提示。
    /// </summary>
    internal class PortalScriptMissing : PortalPlayerInteraction
    {
        public PortalScriptMissing(IChannelClient c, IPortal p) : base(c, p)
        {
        }

        private Task<bool> NotImplemented()
        {
            throw new NotImplementedException();
        }

        [ScriptName("Pachinko_port_in")]
        public Task<bool> p_Pachinko_port_in() => NotImplemented();

        [ScriptName("08_xmas_st")]
        public Task<bool> p_08_xmas_st() => NotImplemented();

        [ScriptName("200951into01")]
        public Task<bool> p_200951into01() => NotImplemented();

        [ScriptName("200951into02")]
        public Task<bool> p_200951into02() => NotImplemented();

        [ScriptName("200951into03")]
        public Task<bool> p_200951into03() => NotImplemented();

        [ScriptName("200951into04")]
        public Task<bool> p_200951into04() => NotImplemented();

        [ScriptName("200951into05")]
        public Task<bool> p_200951into05() => NotImplemented();

        [ScriptName("200951into06")]
        public Task<bool> p_200951into06() => NotImplemented();

        [ScriptName("200951into07")]
        public Task<bool> p_200951into07() => NotImplemented();

        [ScriptName("200951into08")]
        public Task<bool> p_200951into08() => NotImplemented();

        [ScriptName("200951into09")]
        public Task<bool> p_200951into09() => NotImplemented();

        [ScriptName("200951into10")]
        public Task<bool> p_200951into10() => NotImplemented();

        [ScriptName("200951out")]
        public Task<bool> p_200951out() => NotImplemented();

        [ScriptName("A_office")]
        public Task<bool> p_A_office() => NotImplemented();

        [ScriptName("BF_out")]
        public Task<bool> p_BF_out() => NotImplemented();

        [ScriptName("Depart_topFloor")]
        public Task<bool> p_Depart_topFloor() => NotImplemented();

        [ScriptName("Jump_event_pro")]
        public Task<bool> p_Jump_event_pro() => NotImplemented();

        [ScriptName("MD_cakeGL1")]
        public Task<bool> p_MD_cakeGL1() => NotImplemented();

        [ScriptName("MD_cakeGL2")]
        public Task<bool> p_MD_cakeGL2() => NotImplemented();

        [ScriptName("MD_cakeGL3")]
        public Task<bool> p_MD_cakeGL3() => NotImplemented();

        [ScriptName("MD_cakeoutGL")]
        public Task<bool> p_MD_cakeoutGL() => NotImplemented();

        [ScriptName("PB_wich")]
        public Task<bool> p_PB_wich() => NotImplemented();

        [ScriptName("StudioZone_Out")]
        public Task<bool> p_StudioZone_Out() => NotImplemented();

        [ScriptName("aM_start")]
        public Task<bool> p_aM_start() => NotImplemented();

        [ScriptName("beenTreeGate")]
        public Task<bool> p_beenTreeGate() => NotImplemented();

        [ScriptName("been_enter")]
        public Task<bool> p_been_enter() => NotImplemented();

        [ScriptName("been_next")]
        public Task<bool> p_been_next() => NotImplemented();

        [ScriptName("checkWildeye")]
        public Task<bool> p_checkWildeye() => NotImplemented();

        [ScriptName("cheeseEnd")]
        public Task<bool> p_cheeseEnd() => NotImplemented();

        [ScriptName("cheeseEx")]
        public Task<bool> p_cheeseEx() => NotImplemented();

        [ScriptName("cheeseLog")]
        public Task<bool> p_cheeseLog() => NotImplemented();

        [ScriptName("cheeseOut")]
        public Task<bool> p_cheeseOut() => NotImplemented();

        [ScriptName("chimney00_open")]
        public Task<bool> p_chimney00_open() => NotImplemented();

        [ScriptName("chimney01_open")]
        public Task<bool> p_chimney01_open() => NotImplemented();

        [ScriptName("chimney02_open")]
        public Task<bool> p_chimney02_open() => NotImplemented();

        [ScriptName("chimney03_open")]
        public Task<bool> p_chimney03_open() => NotImplemented();

        [ScriptName("chimney10_open")]
        public Task<bool> p_chimney10_open() => NotImplemented();

        [ScriptName("chimney11_open")]
        public Task<bool> p_chimney11_open() => NotImplemented();

        [ScriptName("chimney12_open")]
        public Task<bool> p_chimney12_open() => NotImplemented();

        [ScriptName("chimney13_open")]
        public Task<bool> p_chimney13_open() => NotImplemented();

        [ScriptName("chimney20_open")]
        public Task<bool> p_chimney20_open() => NotImplemented();

        [ScriptName("chimney21_open")]
        public Task<bool> p_chimney21_open() => NotImplemented();

        [ScriptName("chimney22_open")]
        public Task<bool> p_chimney22_open() => NotImplemented();

        [ScriptName("chimney23_open")]
        public Task<bool> p_chimney23_open() => NotImplemented();

        [ScriptName("clearRider")]
        public Task<bool> p_clearRider() => NotImplemented();

        [ScriptName("connetNPC0")]
        public Task<bool> p_connetNPC0() => NotImplemented();

        [ScriptName("crane_MR")]
        public Task<bool> p_crane_MR() => NotImplemented();

        [ScriptName("crane_SS")]
        public Task<bool> p_crane_SS() => NotImplemented();

        [ScriptName("davy_next00")]
        public Task<bool> p_davy_next00() => NotImplemented();

        [ScriptName("donghwa_end")]
        public Task<bool> p_donghwa_end() => NotImplemented();

        [ScriptName("donghwa_out")]
        public Task<bool> p_donghwa_out() => NotImplemented();

        [ScriptName("find_james")]
        public Task<bool> p_find_james() => NotImplemented();

        [ScriptName("fishingOut")]
        public Task<bool> p_fishingOut() => NotImplemented();

        [ScriptName("fishing_out")]
        public Task<bool> p_fishing_out() => NotImplemented();

        [ScriptName("ghost1")]
        public Task<bool> p_ghost1() => NotImplemented();

        [ScriptName("ghost2")]
        public Task<bool> p_ghost2() => NotImplemented();

        [ScriptName("ghost3")]
        public Task<bool> p_ghost3() => NotImplemented();

        [ScriptName("ghostOut1")]
        public Task<bool> p_ghostOut1() => NotImplemented();

        [ScriptName("ghostOut2")]
        public Task<bool> p_ghostOut2() => NotImplemented();

        [ScriptName("ghostOut3")]
        public Task<bool> p_ghostOut3() => NotImplemented();

        [ScriptName("goldkey1")]
        public Task<bool> p_goldkey1() => NotImplemented();

        [ScriptName("goldkey10")]
        public Task<bool> p_goldkey10() => NotImplemented();

        [ScriptName("goldkey2")]
        public Task<bool> p_goldkey2() => NotImplemented();

        [ScriptName("goldkey3")]
        public Task<bool> p_goldkey3() => NotImplemented();

        [ScriptName("goldkey4")]
        public Task<bool> p_goldkey4() => NotImplemented();

        [ScriptName("goldkey5")]
        public Task<bool> p_goldkey5() => NotImplemented();

        [ScriptName("goldkey6")]
        public Task<bool> p_goldkey6() => NotImplemented();

        [ScriptName("goldkey7")]
        public Task<bool> p_goldkey7() => NotImplemented();

        [ScriptName("goldkey8")]
        public Task<bool> p_goldkey8() => NotImplemented();

        [ScriptName("goldkey9")]
        public Task<bool> p_goldkey9() => NotImplemented();

        [ScriptName("hontale_boss1")]
        public Task<bool> p_hontale_boss1() => NotImplemented();

        [ScriptName("hontale_boss2")]
        public Task<bool> p_hontale_boss2() => NotImplemented();

        [ScriptName("hontale_morph")]
        public Task<bool> p_hontale_morph() => NotImplemented();

        [ScriptName("hwqout")]
        public Task<bool> p_hwqout() => NotImplemented();

        [ScriptName("inn00")]
        public Task<bool> p_inn00() => NotImplemented();

        [ScriptName("jnr6_act")]
        public Task<bool> p_jnr6_act() => NotImplemented();

        [ScriptName("mapleTree_out")]
        public Task<bool> p_mapleTree_out() => NotImplemented();

        [ScriptName("move_EliEre")]
        public Task<bool> p_move_EliEre() => NotImplemented();

        [ScriptName("move_EreEli")]
        public Task<bool> p_move_EreEli() => NotImplemented();

        [ScriptName("move_EreOrb")]
        public Task<bool> p_move_EreOrb() => NotImplemented();

        [ScriptName("move_OrbEre")]
        public Task<bool> p_move_OrbEre() => NotImplemented();

        [ScriptName("nooutShip")]
        public Task<bool> p_nooutShip() => NotImplemented();

        [ScriptName("oliviahallOut")]
        public Task<bool> p_oliviahallOut() => NotImplemented();

        [ScriptName("outBabyBird")]
        public Task<bool> p_outBabyBird() => NotImplemented();

        [ScriptName("pField_out")]
        public Task<bool> p_pField_out() => NotImplemented();

        [ScriptName("photoOut")]
        public Task<bool> p_photoOut() => NotImplemented();

        [ScriptName("pinokio_buff")]
        public Task<bool> p_pinokio_buff() => NotImplemented();

        [ScriptName("pinokio_enter")]
        public Task<bool> p_pinokio_enter() => NotImplemented();

        [ScriptName("pinokio_move")]
        public Task<bool> p_pinokio_move() => NotImplemented();

        [ScriptName("pinokio_move2")]
        public Task<bool> p_pinokio_move2() => NotImplemented();

        [ScriptName("pinokio_move3")]
        public Task<bool> p_pinokio_move3() => NotImplemented();

        [ScriptName("pinokio_next")]
        public Task<bool> p_pinokio_next() => NotImplemented();

        [ScriptName("piramid_Chat00")]
        public Task<bool> p_piramid_Chat00() => NotImplemented();

        [ScriptName("piramid_in00")]
        public Task<bool> p_piramid_in00() => NotImplemented();

        [ScriptName("pvp00")]
        public Task<bool> p_pvp00() => NotImplemented();

        [ScriptName("pvp04")]
        public Task<bool> p_pvp04() => NotImplemented();

        [ScriptName("rand_ola")]
        public Task<bool> p_rand_ola() => NotImplemented();

        [ScriptName("rnj6_act")]
        public Task<bool> p_rnj6_act() => NotImplemented();

        [ScriptName("ropeEarth1")]
        public Task<bool> p_ropeEarth1() => NotImplemented();

        [ScriptName("ropeEarth2")]
        public Task<bool> p_ropeEarth2() => NotImplemented();

        [ScriptName("ropeMoon1")]
        public Task<bool> p_ropeMoon1() => NotImplemented();

        [ScriptName("ropeMoon2")]
        public Task<bool> p_ropeMoon2() => NotImplemented();

        [ScriptName("snack_enter")]
        public Task<bool> p_snack_enter() => NotImplemented();

        [ScriptName("summondragon")]
        public Task<bool> p_summondragon() => NotImplemented();

        [ScriptName("tH_Out")]
        public Task<bool> p_tH_Out() => NotImplemented();

        [ScriptName("tutorWorldmap")]
        public Task<bool> p_tutorWorldmap() => NotImplemented();

        [ScriptName("warpEarth")]
        public Task<bool> p_warpEarth() => NotImplemented();

        [ScriptName("warpMoon")]
        public Task<bool> p_warpMoon() => NotImplemented();
    }
}
