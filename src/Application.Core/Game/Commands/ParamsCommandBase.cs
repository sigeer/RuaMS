namespace Application.Core.Game.Commands
{
    public abstract class ParamsCommandBase : CommandBase
    {
        protected string[][] ValidArguments { get; set; }
        protected ParamsCommandBase(string[][] arugments, int level, params string[] syntax) : base(level, syntax)
        {
            ValidArguments = arugments;
        }

        public override string ValidSytax => $"!{CurrentCommand} {string.Join(' ', ValidArguments.Select(x => "<" + string.Join('|', x) + ">"))}";
        public bool CheckArguments(string[] values)
        {
            return values.Length == ValidArguments.Length &&
                   !ValidArguments.Where((arg, i) => !arg.Contains(values[i])).Any();
        }
    }
}
