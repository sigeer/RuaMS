using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Application.Core.Game.Commands.Gm6;

public class DevtestCommand : ParamsCommandBase
{
    public DevtestCommand() : base(["<script>"], 6, "devtest")
    {
        Description = "直接执行代码";
    }

    public override void Execute(IChannelClient client, string[] paramsValue)
    {
        try
        {
            _ = CSharpScript.EvaluateAsync<int>(GetParam("script"), globals: client.OnlinedCharacter);
        }
        catch (CompilationErrorException)
        {
            client.OnlinedCharacter.Pink("代码错误");
        }
        return;
    }
}
