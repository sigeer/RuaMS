
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// WZ 未声明 script 节点、运行期键回退为 <c>n&lt;id&gt;</c> 的 NPC 脚本。
    /// 这些方法只包含单一固定对话（<c>cm.sendOk</c>）或打开仓库，逻辑无分支。
    /// </summary>
    internal partial class NpcScript
    {
        /// <summary>
        /// 只做一件事：打开仓库（JS: <c>cm.OpenStorage(); cm.dispose();</c>）。
        /// JS 中每个文件内容完全相同，这里合并为一个方法供多个 NPC 共用。
        /// </summary>
        // Npc: 1002005, 1012009, 1022005, 1032006, 1052017, 1061008, 1091004, 1100000, 1200000, 2010006, 2020004, 2041008, 2050004, 2060008, 2070000, 2080005, 2090000, 2093003, 2100000, 2110000, 9030100, 9120009, 9201081, 9270042, 9270054
        [ScriptName("n1002005", "n1012009", "n1022005", "n1032006", "n1052017", "n1061008", "n1091004", "n1100000", "n1200000", "n2010006", "n2020004", "n2041008", "n2050004", "n2060008", "n2070000", "n2080005", "n2090000", "n2093003", "n2100000", "n2110000", "n9030100", "n9120009", "n9201081", "n9270042", "n9270054")]
        public async Task OpenStoragePlain()
        {
            await OpenStorage();
        }

        // Npc: 1092015
        [ScriptName("n1092015")]
        public async Task NpcScript_1092015()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_ImBored));
        }

        // Npc: 1096001
        [ScriptName("n1096001")]
        public async Task NpcScript_1096001()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_EverFinish));
        }

        // Npc: 1103000
        [ScriptName("n1103000")]
        public async Task NpcScript_1103000()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_AdvancedKnight));
        }

        // Npc: 1104202
        [ScriptName("n1104202")]
        public async Task NpcScript_1104202()
        {
            await SayOK("...");
        }

        // Npc: 1104203
        [ScriptName("n1104203")]
        public async Task NpcScript_1104203()
        {
            await SayOK("...");
        }

        // Npc: 1104204
        [ScriptName("n1104204")]
        public async Task NpcScript_1104204()
        {
            await SayOK("...");
        }

        // Npc: 1104205
        [ScriptName("n1104205")]
        public async Task NpcScript_1104205()
        {
            await SayOK("...");
        }

        // Npc: 1104206
        [ScriptName("n1104206")]
        public async Task NpcScript_1104206()
        {
            await SayOK("...");
        }

        // Npc: 1104207
        [ScriptName("n1104207")]
        public async Task NpcScript_1104207()
        {
            await SayOK("...");
        }

        // Npc: 1209001
        [ScriptName("n1209001")]
        public async Task NpcScript_1209001()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_BlackMage));
        }

        // Npc: 1209002
        [ScriptName("n1209002")]
        public async Task NpcScript_1209002()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_PleaseCalm));
        }

        // Npc: 1209003
        [ScriptName("n1209003")]
        public async Task NpcScript_1209003()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_DepartingVictoria));
        }

        // Npc: 1209004
        [ScriptName("n1209004")]
        public async Task NpcScript_1209004()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_HopeTravel));
        }

        // Npc: 1209005
        [ScriptName("n1209005")]
        public async Task NpcScript_1209005()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_AlrightEmbarking));
        }

        // Npc: 1300001
        [ScriptName("n1300001")]
        public async Task NpcScript_1300001()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_LetCeremony));
        }

        // Npc: 1300006
        [ScriptName("n1300006")]
        public async Task NpcScript_1300006()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_GuysWhats));
        }

        // Npc: 2041017
        [ScriptName("n2041017")]
        public async Task NpcScript_2041017()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_AdventurerKeep));
        }

        // Npc: 2041024
        [ScriptName("n2041024")]
        public async Task NpcScript_2041024()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_CapableGreat));
        }

        // Npc: 2041029
        [ScriptName("n2041029")]
        public async Task NpcScript_2041029()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_AheadMysterious));
        }

        // Npc: 2101002
        [ScriptName("n2101002")]
        public async Task NpcScript_2101002()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_StayAway));
        }

        // Npc: 2120003
        [ScriptName("n2120003")]
        public async Task NpcScript_2120003()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_WelcomeDear));
        }

        // Npc: 2131000
        [ScriptName("n2131000")]
        public async Task NpcScript_2131000()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_WhileSince));
        }

        // Npc: 2131002
        [ScriptName("n2131002")]
        public async Task NpcScript_2131002()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_NameSee));
        }

        // Npc: 2132000
        [ScriptName("n2132000")]
        public async Task NpcScript_2132000()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_HmmmMake));
        }

        // Npc: 9120023
        [ScriptName("n9120023")]
        public async Task NpcScript_9120023()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_QualityMovies));
        }

        // Npc: 9270031
        [ScriptName("n9270031")]
        public async Task NpcScript_9270031()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain_HadWedding));
        }


        // Npc: 1094000, 2101000, 2101001, 2111004, 2111007, 2111008, 2111009, 2111016, 9250045
        /// <summary>JS 只调用 <c>cm.sendDefault()</c>（无参），或完全无行为。</summary>
        [ScriptName("n1094000", "n2101000", "n2101001", "n2111004", "n2111007", "n2111008", "n2111009", "n2111016", "n9250045")]
        public async Task SendDefaultPlain()
        {
            await sendDefault();
        }

        // Npc: 2111001
        /// <summary>JS: <c>cm.sendDefault(3315)</c> —— 带任务检查的默认对话。</summary>
        [ScriptName("n2111001")]
        public async Task NpcScript_2111001()
        {
            await sendDefault(3315);
        }

        // Npc: 2111005
        /// <summary>JS: <c>cm.sendDefault(3333)</c> —— 带任务检查的默认对话。</summary>
        [ScriptName("n2111005")]
        public async Task NpcScript_2111005()
        {
            await sendDefault(3333);
        }

        // Npc: 9040004, 9040008
        [ScriptName("n9040004", "n9040008")]
        public async Task DisplayGuildRanksPlain()
        {
            await displayGuildRanks();
        }
    }
}
