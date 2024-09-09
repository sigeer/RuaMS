using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utility.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException()
        {
        }

        public BusinessException(string? message) : base(message)
        {
        }

        public BusinessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class BusinessDataNullException : BusinessException
    {
        public BusinessDataNullException() : base("此处的变量不应该为null")
        {
        }

        public BusinessDataNullException(string? message) : base(message)
        {
        }

        public BusinessDataNullException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }


    /// <summary>
    /// 1. wz（或其他）资源未获取到或者不合法 - 用存在的id去获取一个应该存在但实际上不存在的数据
    /// 2. 数据库基础数据未获取到
    /// </summary>
    public class BusinessResException : BusinessDataNullException
    {
        public BusinessResException(): base()
        {
        }

        public BusinessResException(string? message) : base(message)
        {
        }

        public BusinessResException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 角色已下线
    /// </summary>
    public class BusinessCharacterOfflineException : BusinessException
    {
        public BusinessCharacterOfflineException():base("Client not onlined")
        {
        }
    }

    public class BusinessCharacterNotFoundException : BusinessException
    {
        public BusinessCharacterNotFoundException() : base("Client not found")
        {
        }
    }
}
