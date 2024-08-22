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

    public class BusinessArgumentNullException : BusinessException
    {
        public BusinessArgumentNullException() : base("此处的变量不应该为null")
        {
        }
    }


    public class BusinessWarningException : BusinessException
    {
        public BusinessWarningException(string? message) : base(message)
        {
        }
    }


    /// <summary>
    /// 1. wz（或其他）资源未获取到 - 用存在的id去获取一个应该存在但实际上不存在的数据
    /// 2. 数据库基础数据未获取到
    /// 3. 其他情况：用不存在的id去获取一个不应该存在的数据
    /// </summary>
    public class BusinessDataNullException : BusinessException
    {
        public BusinessDataNullException()
        {
        }

        public BusinessDataNullException(string? message) : base(message)
        {
        }

        public BusinessDataNullException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
