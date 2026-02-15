using Application.Core.Login.Models;
using Application.Core.Login.Services.PlayerCreator.Novice;
using Application.Utility.Exceptions;

namespace Application.Core.Login.Services.PlayerCreator;


public class CharacterFactory
{
    public static NoviceCreator GetNoviceCreator(int type)
    {
        if (type == 0) return new NoblesseCreator();
        if (type == 1) return new BeginnerCreator();
        if (type == 2) return new LegendCreator();
        throw new BusinessFatalException("不支持的创建类型");
    }
}
