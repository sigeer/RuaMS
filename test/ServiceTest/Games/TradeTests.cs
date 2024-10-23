using Application.Core.Game.Trades;

namespace ServiceTest.Games
{
    internal class TradeTests : TestBase
    {
        [Test]
        public void SameTradeCheckTest()
        {
            var chr1 = GetOnlinedTestClient(1).OnlinedCharacter;
            var chr2 = GetOnlinedTestClient(2).OnlinedCharacter;
            TradeManager.StartTrade(chr1);

            var c1Trade = chr1.getTrade()!;
            Assert.That(TradeManager.JoinTrade(c1Trade, chr2));

            var c2Trade = chr2.getTrade()!;

            Assert.That(chr1.getTrade()?.PartnerTrade, Is.EqualTo(c2Trade));

            chr1.gainMeso(10000);

            var c1Meso = chr1.getMeso();
            var c2Meso = chr2.getMeso();

            c1Trade.setMeso(1000);
            c2Trade.setMeso(0);

            TradeManager.CompleteTrade(chr1);
            TradeManager.CompleteTrade(chr2);

            Assert.That(chr1.getMeso(), Is.EqualTo(c1Meso - 1000));
            Assert.That(chr2.getMeso(), Is.EqualTo(c2Meso + 1000));

            Assert.That(chr1.getTrade() == null);
            Assert.That(chr2.getTrade() == null);
        }
    }
}
