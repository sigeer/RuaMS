namespace Application.Utility.Exceptions
{

    public class BusinessException : Exception
    {
        public BusinessException() : this("内部错误")
        {
        }

        public BusinessException(string message) : base(message)
        {

        }

        public BusinessException(string message, Exception inner) : base(message, inner)
        {

        }
    }


    /// <summary>
    /// 1. wz（或其他）资源未获取到或者不合法 - 用存在的id去获取一个应该存在但实际上不存在的数据
    /// 2. 数据库基础数据未获取到
    /// </summary>
    public class BusinessResException(string? message) : BusinessException($"资源未获取到 - {message}")
    {
    }

    /// <summary>
    /// 角色已下线
    /// </summary>
    public class BusinessCharacterOfflineException : BusinessException
    {
        public BusinessCharacterOfflineException() : base("角色不在线")
        {
        }
    }

    public class BusinessCharacterNotFoundException : BusinessException
    {
        public BusinessCharacterNotFoundException(int characterId) : base($"角色 Id={characterId} 不存在")
        {
        }

        public BusinessCharacterNotFoundException(string characterName) : base($"角色 Name={characterName} 不存在")
        {
        }
    }

    public class BusinessFatalException : BusinessException
    {
        public BusinessFatalException() : base()
        {
        }

        public BusinessFatalException(string message) : base(message)
        {
        }
    }

    public class BusinessNotsupportException : BusinessException
    {
        public BusinessNotsupportException(string function) : base($"功能 {function} 未启用")
        {
        }
    }
}
