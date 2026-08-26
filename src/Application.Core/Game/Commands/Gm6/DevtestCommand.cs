using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Application.Core.Game.Commands.Gm6;

public class DevtestCommand : CommandBase
{
    static string devtestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Devtest.txt");
    public DevtestCommand() : base(6, "devtest")
    {
        Description = "直接执行代码";
    }

    public override async Task Execute(IChannelClient client, string[] paramsValue)
    {
        try
        {
            await CSharpScript.EvaluateAsync<int>(File.ReadAllText(devtestPath), globals: new ScriptGlobals(client.OnlinedCharacter));
        }
        catch (CompilationErrorException ex)
        {
            await client.OnlinedCharacter.Pink("代码错误");
        }
    }
}

public class ScriptGlobals
{
    public ScriptGlobals(Player chr)
    {
        this.chr = chr;
    }

    public Player chr { get; set; }
}

