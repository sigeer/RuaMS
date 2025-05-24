namespace Application.Core.Game.Commands.Gm6;

public class SaveDBCommand : CommandBase
{
    public SaveDBCommand() : base(6, "savedb")
    {
        Description = "保存到数据库";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.saveCharToDB();
        c.CurrentServer.Transport.CallSaveDB();
        c.OnlinedCharacter.message("保存成功");
    }
}
