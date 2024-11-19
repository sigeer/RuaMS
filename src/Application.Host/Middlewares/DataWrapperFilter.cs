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
                if (objectResult.DeclaredType?.Name != typeof(ResponseData<>).Name)
                {
                    objectResult.Value = new ResponseData<object>(objectResult.Value);
                }
            }


            await next();
        }
    }
}
