namespace Application.Core.Game.Commands
{
    public class CommandArgumentException : Exception
    {
        public CommandArgumentException()
        {
        }

        public CommandArgumentException(string? message) : base(message)
        {
        }
    }
}
