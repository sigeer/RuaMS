namespace Application.Core.Game.Trades
{
    public record Visitor(IPlayer chr, DateTimeOffset enteredAt) { }

    public record PastVisitor(string chrName, TimeSpan visitDuration) { }
}
