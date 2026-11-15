namespace Application.Core.scripting.Infrastructure
{

    public record TalkMoreAction(sbyte Mode, sbyte type, long selection, string? inputText);

    public class ConversationInterruptException() : BusinessException;
    /// <summary>
    /// 在A地图与B地图的NPC对话（脚本依赖当前所在地图有不同处理）
    /// </summary>
    public class ConversationDiffMapException() : BusinessException("对话的Npc不在这个地图内");
    /// <summary>
    /// 非FB成员与FB NPC对话
    /// </summary>
    public class ConversationDiffInstanceException() : BusinessException("玩家身处事件地图却没有EIM");
    public class ConversationException() : Exception;

    public record SpeechText(string Text, NpcTalkSpeaker Speaker, int SpeakerNpc = 0);
}
