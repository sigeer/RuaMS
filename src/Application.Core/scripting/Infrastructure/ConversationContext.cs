namespace Application.Core.scripting.Infrastructure
{

    public record TalkMoreAction(sbyte Mode, sbyte type, int selection, string? inputText);

    public class ConversationInterruptException() : Exception;
    /// <summary>
    /// 在A地图与B地图的NPC对话
    /// </summary>
    public class ConversationDiffMapException() : Exception;
    /// <summary>
    /// 非FB成员与FB NPC对话
    /// </summary>
    public class ConversationDiffInstanceException() : Exception;
    public class ConversationException() : Exception;

    public record SpeechText(string Text, byte Speaker, int SpeakerNpc = 0);
}
