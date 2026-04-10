namespace Application.Core.scripting.Infrastructure
{

    public record TalkMoreAction(sbyte Mode, sbyte type, int selection, string? inputText);

    public class ConversationInterruptException() : Exception;
    public class ConversationException() : Exception;
}
