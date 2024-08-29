namespace server;

public class ChatLogger
{
    private static ILogger _log = LogFactory.GetLogger(LogType.Chat);

    /**
     * Log a chat message (if enabled in the config)
     */
    public static void log(IClient c, string chatType, string message)
    {
        if (YamlConfig.config.server.USE_ENABLE_CHAT_LOG)
        {
            _log.Information("({ChatType}) {CharacterName}: {ChatMessage}", chatType, c.OnlinedCharacter.getName(), message);
        }
    }
}
