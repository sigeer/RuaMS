using System.Text.RegularExpressions;

namespace Application.Core.Game.Commands
{
    /// <summary>
    /// 带参命令，大小写敏感
    /// <para>CurrentCommand [Const1|Const2|...|ConstN] &lt;Variable&gt; <see cref="ValidSytax"/></para>
    /// </summary>
    public abstract class ParamsCommandBase : CommandBase
    {
        protected string[] ValidArguments { get; set; }
        private Dictionary<string, string> NamedArguments { get; set; }

        protected ParamsCommandBase(string[] arugments, int level, params string[] syntax) : base(level, syntax)
        {
            ValidArguments = arugments;
            NamedArguments = new Dictionary<string, string>();
        }

        public override string ValidSytax => $"!{CurrentCommand} {string.Join(' ', ValidArguments)}";
        public override bool CheckArguments(string[] values)
        {
            if (values.Length == ValidArguments.Length)
            {
                for (int i = 0; i < ValidArguments.Length; i++)
                {
                    var mat = NamedReg.Match(ValidArguments[i]);
                    if (mat.Success)
                        NamedArguments[mat.Groups[1].Value] = values[i];

                    NamedArguments[$"I_{i}"] = values[i];
                }

                return !ValidArguments.Where((arg, i) => !IsValidArgument(arg, values[i])).Any();
            }
            return false;
        }

        private Regex ConstReg = new Regex("\\[(\\S+)\\]");
        private Regex NamedReg = new Regex("<(\\S+)>");
        private bool IsValidArgument(string checkRule, string input)
        {
            var constMat = ConstReg.Match(checkRule);
            if (constMat.Success)
                return constMat.Groups[1].Value.Split('|').Contains(input);

            if (NamedReg.IsMatch(checkRule))
                return true;

            return checkRule == input;
        }

        /// <summary>
        /// 获取命令中 &lt;name&gt;的部分
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="CommandArgumentException"></exception>
        protected string GetParam(string name)
        {
            return NamedArguments.GetValueOrDefault(name) ?? throw new CommandArgumentException($"缺少 {name}");
        }

        /// <summary>
        /// 获取命令中 &lt;name&gt;的部分
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="CommandArgumentException"></exception>
        protected int GetIntParam(string name)
        {
            var str = GetParam(name);
            if (!int.TryParse(str, out var d))
                throw new CommandArgumentException($"{name} 应该是数字");
            return d;
        }

        protected float GetFloatParam(string name)
        {
            var str = GetParam(name);
            if (!float.TryParse(str, out var d))
                throw new CommandArgumentException($"{name} 应该是数字");
            return d;
        }

        protected string? GetParamByIndex(int index)
        {
            return NamedArguments.GetValueOrDefault($"I_{index}");
        }
    }
}
