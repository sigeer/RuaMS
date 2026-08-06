using Application.Host.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Host.Middlewares
{
    internal class DataWrapperFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                // 判断值是否已经为 ResponseData
                if (objectResult.Value is not null &&
                    objectResult.Value.GetType().IsGenericType &&
                    objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ResponseData<>))
                {
                    // 已包装，跳过
                }
                else
                {
                    // 包装
                    objectResult.Value = new ResponseData<object>(objectResult.Value);
                    objectResult.DeclaredType = objectResult.Value?.GetType();
                }
            }

            await next();
        }
    }
}
