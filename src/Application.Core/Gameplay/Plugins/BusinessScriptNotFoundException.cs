using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Gameplay.Plugins
{
    public class BusinessScriptNotFoundException(string name) : BusinessException(name)
    {
    }
}
