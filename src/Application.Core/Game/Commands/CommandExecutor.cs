using constants.id;
using System.Reflection;

namespace Application.Core.Game.Commands;

public class CommandExecutor
{
    private ILogger log = LogFactory.CommandLogger;
    private const char COMMAND_HEADING = '!';

    private Dictionary<string, CommandBase> registeredCommands = new();

    private static CommandExecutor instance = new CommandExecutor();
    public static CommandExecutor getInstance()
    {
        return instance;
    }

    public List<List<CommandInfo>> getGmCommands()
    {
        return registeredCommands.OrderBy(x => x.Value.Rank).GroupBy(x => x.Value.Rank).Select(x => x.Select(y => new CommandInfo(y.Key, y.Value.Description)).ToList()).ToList();
    }

    public static bool isCommand(string content)
    {
        char heading = content.ElementAt(0);
        return heading == COMMAND_HEADING;
    }

    private CommandExecutor()
    {
        var commandBase = typeof(CommandBase);
        var assembly = Assembly.GetAssembly(commandBase)!;
        var commands = assembly.GetTypes().Where(x => x.IsSubclassOf(commandBase) && !x.IsAbstract);
        foreach (var item in commands)
        {
            var obj = (Activator.CreateInstance(item) as CommandBase)!;
            foreach (var sytax in obj.AllSupportedCommand)
            {
                registeredCommands.Add(sytax, obj);
            }
        }
    }

    public void handle(IChannelClient client, string message)
    {
        if (client.tryacquireClient())
        {
            try
            {
                handleInternal(client, message);
            }
            finally
            {
                client.releaseClient();
            }
        }
        else
        {
            client.OnlinedCharacter.dropMessage(5, "Try again in a while... Latest commands are currently being processed.");
        }
    }

    private void handleInternal(IChannelClient client, string message)
    {
        if (client.OnlinedCharacter.getMapId() == MapId.JAIL)
        {
            client.OnlinedCharacter.yellowMessage("You do not have permission to use commands while in jail.");
            return;
        }
        string splitRegex = " ";
        string[] SplitedMessage = message.Substring(1).Split(splitRegex, 2);
        if (SplitedMessage.Length < 2)
        {
            SplitedMessage = new string[] { SplitedMessage[0], "" };
        }

        client.OnlinedCharacter.setLastCommandMessage(SplitedMessage[1]);    // thanks Tochi & Nulliphite for noticing string messages being marshalled lowercase
        string commandName = SplitedMessage[0].ToLower();
        string[] lowercaseParams = SplitedMessage[1].Split(splitRegex);

        var command = registeredCommands.GetValueOrDefault(commandName);
        if (command == null)
        {
            client.OnlinedCharacter.yellowMessage("Command '" + commandName + "' is not available. See !commands for a list of available commands.");
            return;
        }
        if (client.OnlinedCharacter.gmLevel() < command.Rank)
        {
            client.OnlinedCharacter.yellowMessage("You do not have permission to use this command.");
            return;
        }
        string[] paramsValue;
        if (lowercaseParams.Length > 0 && lowercaseParams[0].Count() > 0)
        {
            paramsValue = Arrays.copyOfRange(lowercaseParams, 0, lowercaseParams.Length);
        }
        else
        {
            paramsValue = new string[] { };
        }

        command.CurrentCommand = commandName;
        command.Run(client, paramsValue);
        log.Information("Chr {CharacterName} used command {Command}, Params: {Params}", client.OnlinedCharacter.getName(), command.GetType().Name, string.Join(", ", paramsValue));
    }
}

public record CommandInfo(string Name, string? Description);
