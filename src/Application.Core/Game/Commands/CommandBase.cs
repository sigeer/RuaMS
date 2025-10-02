using Application.Resources.Messages;

namespace Application.Core.Game.Commands
{
    public abstract class CommandBase
    {
        protected ILogger log;
        protected string _commandName;
        public CommandBase(int level, params string[] syntax)
        {
            var commandType = GetType();
            log = LogFactory.CommandLogger;

            Rank = level;
            AllSupportedCommand = syntax;
            _commandName = GetType().Name!;
        }

        public virtual string ValidSytax => $"!{CurrentCommand}";
        public string[] AllSupportedCommand { get; set; }
        public int Rank { get; set; }
        public string? Description { get; set; }
        protected virtual string GetDescriptionKey()
        {
            return $"{_commandName}_Description";
        }

        protected virtual string GetSyntaxKey()
        {
            return $"{_commandName}_Syntax";
        }
        public virtual string? GetDescription(IChannelClient client)
        {
            return ClientMessage.ResourceManager.GetString(GetDescriptionKey(), client.GetCulture()) ?? Description;
        }

        private string? _currentCommand;
        public string CurrentCommand
        {
            get
            {
                if (_currentCommand == null)
                    return AllSupportedCommand.ElementAtOrDefault(0)!;
                return _currentCommand;
            }
            set => _currentCommand = value;
        }

        public virtual void Run(IChannelClient client, string[] values)
        {
            try
            {
                if (!CheckArguments(values))
                {
                    client.OnlinedCharacter.yellowMessage($"命令语法错误：{ValidSytax}");
                    return;
                }

                Execute(client, values);
            }
            catch (CommandArgumentException ex)
            {
                client.OnlinedCharacter.yellowMessage($"命令语法错误：{ValidSytax}, {ex.Message}");
                return;
            }
        }
        public virtual bool CheckArguments(string[] values)
        {
            return true;
        }
        public abstract void Execute(IChannelClient client, string[] values);
        protected string joinStringFrom(string[] arr, int start)
        {
            return string.Join(' ', arr, start, arr.Length - start);
        }
    }
}
