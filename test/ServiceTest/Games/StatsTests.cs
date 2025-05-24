using Application.Core.Game.Players;
using Application.Core.Game.Skills;
using client;
using constants.skills;

namespace ServiceTest.Games
{
    public class StatsTests : TestBase
    {
        //public StatsTests()
        //{
        //    SkillFactory.LoadAllSkills();
        //}

        //private void PrintHP(string prefix, IPlayer chr)
        //{
        //    Console.WriteLine($"===={prefix}====");
        //    chr.PrintStatsUpdated();
        //}


        //[Test]
        //public void UseHyperBody()
        //{
        //    var client = GetOnlinedTestClient();
        //    PrintHP("初始", client.OnlinedCharacter);
        //    client.OnlinedCharacter.healHpMp();
        //    PrintHP("healHpMp", client.OnlinedCharacter);
        //    var oldMaxHp = client.OnlinedCharacter.ActualMaxHP;
        //    Assert.That(client.OnlinedCharacter.HP, Is.EqualTo(client.OnlinedCharacter.ActualMaxHP));
        //    SkillFactory.GetSkillTrust(Spearman.HYPER_BODY).getEffect(30).applyTo(client.OnlinedCharacter);
        //    PrintHP("Buff后", client.OnlinedCharacter);
        //    Assert.That(client.OnlinedCharacter.ActualMaxHP > oldMaxHp);
        //    client.OnlinedCharacter.cancelEffectFromBuffStat(BuffStat.HYPERBODYHP);
        //    PrintHP("取消Buff", client.OnlinedCharacter);
        //    Assert.That(client.OnlinedCharacter.ActualMaxHP == oldMaxHp);
        //}
    }
}
