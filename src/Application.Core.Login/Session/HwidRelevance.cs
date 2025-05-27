namespace Application.Core.Login.Session;

public record HwidRelevance(string hwid, int relevance)
{
    public int getIncrementedRelevance()
    {
        return relevance < sbyte.MaxValue ? relevance + 1 : relevance;
    }
}
