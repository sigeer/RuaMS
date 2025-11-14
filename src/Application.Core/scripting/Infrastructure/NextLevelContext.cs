namespace Application.Core.scripting.Infrastructure
{
    public enum NextLevelType
    {
        /// <summary>
        /// sendNextLevel
        /// </summary>
        SEND_NEXT,
        /// <summary>
        /// sendLastLevel
        /// </summary>
        SEND_LAST,
        /// <summary>
        /// sendLastNextLevel
        /// </summary>
        SEND_LAST_NEXT,
        /// <summary>
        /// sendOkLevel
        /// </summary>
        SEND_OK,
        /// <summary>
        /// sendSelectLevel
        /// </summary>
        SEND_SELECT,
        /// <summary>
        /// sendNextSelectLevel
        /// </summary>
        SEND_NEXT_SELECT,
        /// <summary>
        /// getInputNumberLevel
        /// </summary>
        GET_INPUT_NUMBER,
        /// <summary>
        /// getInputTextLevel
        /// </summary>
        GET_INPUT_TEXT,
        /// <summary>
        /// sendAcceptDeclineLevel
        /// </summary>
        SEND_ACCEPT_DECLINE,
        /// <summary>
        /// sendYesNoLevel
        /// </summary>
        SEND_YES_NO,

        SEND_PARAMED_NEXT,
    }

    public record NextLevelFunction(string Name, params object?[]? Params);

    public class NextLevelContext
    {
        public NextLevelType? LevelType { get; set; }
        private string? lastLevel;
        private string? nextLevel;
        private object?[]? paramsValue;

        public bool TryGetInvokeFunction(IChannelClient c, sbyte mode, sbyte type, int selection, out NextLevelFunction? nextLevelFunction)
        {
            nextLevelFunction = null;
            switch (LevelType)
            {
                case NextLevelType.SEND_SELECT:
                    if (mode == 0)
                        return false;

                    nextLevelFunction = new NextLevelFunction("level" + nextLevel + selection);
                    break;
                case NextLevelType.GET_INPUT_NUMBER:
                case NextLevelType.SEND_NEXT_SELECT:
                    if (mode == 0)
                        return false;

                    nextLevelFunction = new NextLevelFunction("level" + nextLevel, selection);
                    break;
                case NextLevelType.GET_INPUT_TEXT:
                    if (mode == 0)
                        return false;

                    nextLevelFunction = new NextLevelFunction("level" + nextLevel, c.NPCConversationManager?.getText());
                    break;
                case NextLevelType.SEND_LAST_NEXT:
                case NextLevelType.SEND_NEXT:
                case NextLevelType.SEND_LAST:
                case NextLevelType.SEND_OK:
                case NextLevelType.SEND_ACCEPT_DECLINE:
                case NextLevelType.SEND_YES_NO:
                    if (mode == -1)
                        return false;

                    var nextOption = (mode == 0 ? lastLevel : nextLevel);
                    if (string.IsNullOrWhiteSpace(nextOption) || nextOption.Equals("dispose", StringComparison.OrdinalIgnoreCase))
                        return false;

                    nextLevelFunction = new NextLevelFunction("level" + nextOption, paramsValue);
                    break;
                default:
                    LogFactory.GetLogger("Script/NextLevel").Error("Unsupported level type: {LevelType}", LevelType);
                    return false;
            }
            return true;
        }

        public void OneOption(NextLevelType levelType, string? nameValue, params object?[] paramValue)
        {
            if (levelType == NextLevelType.SEND_LAST)
                TwoOption(levelType, nameValue, null, paramValue);
            else
                TwoOption(levelType, null, nameValue, paramValue);
        }

        public void TwoOption(NextLevelType nextLevelType, string? lastLevel, string? nextLevel, params object?[] paramValue)
        {
            Clear();
            this.lastLevel = lastLevel;
            this.nextLevel = nextLevel;
            this.LevelType = nextLevelType;
            this.paramsValue = paramValue;
        }

        object? tempData;
        public void SetContextData(object? obj)
        {
            tempData = obj;
        }

        public object? GetContextData() => tempData;
        public void Clear()
        {
            this.LevelType = null;
            this.lastLevel = null;
            this.nextLevel = null;
            paramsValue = null;
            tempData = null;
        }
    }
}
