namespace Application.Core.Channel.QuestRecordEx
{
    public interface IQuestRecordEx
    {
        short QuestId { get; }
        Task Flush(Player chr);
    }
}
