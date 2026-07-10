namespace Application.Core.Gameplay.Plugins
{
    public class BusinessScriptNotFoundException(string name) : BusinessException(name)
    {
    }
}
