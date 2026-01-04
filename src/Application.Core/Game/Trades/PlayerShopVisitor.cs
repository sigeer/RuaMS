namespace Application.Core.Game.Trades
{
    public record Visitor(Player chr, DateTimeOffset enteredAt) { }

    public record PastVisitor(string chrName, TimeSpan visitDuration) { }
}
