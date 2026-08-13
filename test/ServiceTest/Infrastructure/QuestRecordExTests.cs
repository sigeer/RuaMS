using Application.Core.Channel.QuestRecordEx;
using Application.Utility;

namespace ServiceTest.Infrastructure;

public class QuestRecordExTests
{
    private const short QuestId = 1200;

    static Dictionary<string, string> ParseKeys(string text)
    {
        return KeyValueStringParser.Parse(text);
    }

    [Test]
    public void DefaultState()
    {
        var model = new PartyQuestRecordEx(QuestId);
        Assert.That(model.QuestId, Is.EqualTo(QuestId));
        Assert.That(model.Rank, Is.EqualTo("F"));
        Assert.That(model.Cmp, Is.EqualTo(0));
        Assert.That(model.Try, Is.EqualTo(0));
        Assert.That(model.TotalCost, Is.EqualTo(0));
        Assert.That(model.CompleteTime, Is.EqualTo(0));
    }

    [Test]
    public void ToString_NoTime_OnlyStandardKeys()
    {
        var model = new PartyQuestRecordEx(QuestId);
        var text = model.ToString();

        var keys = ParseKeys(text);
        Assert.That(keys.Keys, Is.EquivalentTo(new[] { "rank", "cmp", "try", "TotalCost", "CompleteTime" }));
        Assert.That(keys["rank"], Is.EqualTo("F"));
        Assert.That(keys["cmp"], Is.EqualTo("0"));
        Assert.That(keys["try"], Is.EqualTo("0"));
        Assert.That(keys["TotalCost"], Is.EqualTo("0"));
        Assert.That(keys["CompleteTime"], Is.EqualTo("0"));
        Assert.That(text, Does.Not.Contain("min="));
        Assert.That(text, Does.Not.Contain("date="));
    }

    [Test]
    public void ToString_WithTime_IncludesRawAndDisplayKeys()
    {
        var model = new PartyQuestRecordEx(QuestId)
        {
            TotalCost = 305_000,
            CompleteTime = 1_700_000_000_000
        };
        var text = model.ToString();

        var keys = ParseKeys(text);
        Assert.That(keys["TotalCost"], Is.EqualTo("305000"));
        Assert.That(keys["CompleteTime"], Is.EqualTo("1700000000000"));
        Assert.That(keys["min"], Is.EqualTo("5"));
        Assert.That(keys["sec"], Is.EqualTo("5"));
        Assert.That(text, Does.Contain("date="));
    }

    [Test]
    public void RoundTrip_TryIncrement()
    {
        var first = new PartyQuestRecordEx(QuestId);
        first.Try++;
        var saved = first.ToString();

        var second = new PartyQuestRecordEx(QuestId, saved);
        second.Try++;

        Assert.That(second.Try, Is.EqualTo(2));
        Assert.That(second.Cmp, Is.EqualTo(0));
        Assert.That(second.Rank, Is.EqualTo("F"));
        Assert.That(second.QuestId, Is.EqualTo(QuestId));
    }

    [Test]
    public void RoundTrip_CompleteSettlement()
    {
        var first = new PartyQuestRecordEx(QuestId);
        first.Cmp++;
        first.TotalCost = 305_000;
        first.CompleteTime = 1_700_000_000_000;
        var saved = first.ToString();

        var second = new PartyQuestRecordEx(QuestId, saved);

        Assert.That(second.Rank, Is.EqualTo("F"));
        Assert.That(second.Cmp, Is.EqualTo(1));
        Assert.That(second.Try, Is.EqualTo(0));
        Assert.That(second.TotalCost, Is.EqualTo(305_000));
        Assert.That(second.CompleteTime, Is.EqualTo(1_700_000_000_000));
    }

    [Test]
    public void RoundTrip_PreservesRawPrecision()
    {
        var first = new PartyQuestRecordEx(QuestId)
        {
            TotalCost = 305_123,
            CompleteTime = 1_700_000_123_456
        };
        var saved = first.ToString();

        var second = new PartyQuestRecordEx(QuestId, saved);

        Assert.That(second.TotalCost, Is.EqualTo(305_123));
        Assert.That(second.CompleteTime, Is.EqualTo(1_700_000_123_456));
    }

    [Test]
    public void DateUsesFixedFormat()
    {
        var model = new PartyQuestRecordEx(QuestId)
        {
            CompleteTime = 1_700_000_000_000
        };
        var text = model.ToString();

        var keys = ParseKeys(text);
        Assert.That(keys.TryGetValue("date", out var date), Is.True);
        Assert.That(date, Does.Match(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$"));
    }

    [Test]
    public void ToStringAndConstructRoundTrip_AllPropertiesEqual()
    {
        var source = new PartyQuestRecordEx(QuestId)
        {
            Rank = "S",
            Cmp = 7,
            Try = 12,
            TotalCost = 305_123,
            CompleteTime = 1_700_000_123_456
        };
        var text = source.ToString();

        var restored = new PartyQuestRecordEx(QuestId, text);

        Assert.That(restored.QuestId, Is.EqualTo(source.QuestId));
        Assert.That(restored.Rank, Is.EqualTo(source.Rank));
        Assert.That(restored.Cmp, Is.EqualTo(source.Cmp));
        Assert.That(restored.Try, Is.EqualTo(source.Try));
        Assert.That(restored.TotalCost, Is.EqualTo(source.TotalCost));
        Assert.That(restored.CompleteTime, Is.EqualTo(source.CompleteTime));
    }

    [Test]
    public void Parse_Empty_NoThrow()
    {
        Assert.DoesNotThrow(() => new PartyQuestRecordEx(QuestId, ""));
        Assert.DoesNotThrow(() => new PartyQuestRecordEx(QuestId));
    }

    [Test]
    public void Parse_Malformed_NoThrow()
    {
        var model = new PartyQuestRecordEx(QuestId, "garbage;;=x;rank;min=abc;date=notadate");

        Assert.That(model.Rank, Is.EqualTo("F"));
        Assert.That(model.Try, Is.EqualTo(0));
        Assert.That(model.Cmp, Is.EqualTo(0));
        Assert.That(model.TotalCost, Is.EqualTo(0));
        Assert.That(model.CompleteTime, Is.EqualTo(0));
    }

    [Test]
    public void Parse_NonNumeric_NoThrow()
    {
        var model = new PartyQuestRecordEx(QuestId, "rank=F;cmp=abc;try=;TotalCost=x;CompleteTime=yz");

        Assert.That(model.Rank, Is.EqualTo("F"));
        Assert.That(model.Cmp, Is.EqualTo(0));
        Assert.That(model.Try, Is.EqualTo(0));
        Assert.That(model.TotalCost, Is.EqualTo(0));
        Assert.That(model.CompleteTime, Is.EqualTo(0));
    }

    //[Test]
    //public void ConfrontQuestEx_RoundTrip()
    //{
    //    var source = new ConfrontQuestEx
    //    {
    //        Try = 3,
    //        VicCount = 2,
    //        LoseCount = 1,
    //        DrawCount = 0,
    //        GiveUpCount = 1
    //    };
    //    var text = source.ToString();

    //    var parsed = new ConfrontQuestEx(text);

    //    Assert.That(parsed.Try, Is.EqualTo(3));
    //    Assert.That(parsed.VicCount, Is.EqualTo(2));
    //    Assert.That(parsed.LoseCount, Is.EqualTo(1));
    //    Assert.That(parsed.DrawCount, Is.EqualTo(0));
    //    Assert.That(parsed.GiveUpCount, Is.EqualTo(1));
    //}

    //[Test]
    //public void ConfrontQuestEx_MissingKeys_DefaultZero()
    //{
    //    var partial = new ConfrontQuestEx("try=5");
    //    Assert.That(partial.Try, Is.EqualTo(5));
    //    Assert.That(partial.VicCount, Is.EqualTo(0));
    //    Assert.That(partial.LoseCount, Is.EqualTo(0));
    //    Assert.That(partial.DrawCount, Is.EqualTo(0));
    //    Assert.That(partial.GiveUpCount, Is.EqualTo(0));

    //    var garbage = new ConfrontQuestEx("garbage;;=x;vic=notanumber");
    //    Assert.That(garbage.Try, Is.EqualTo(0));
    //    Assert.That(garbage.VicCount, Is.EqualTo(0));
    //}
}
