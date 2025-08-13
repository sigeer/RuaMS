using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Application.Protos
{
    public class LoggingInterceptor : Interceptor
    {
        readonly ILogger<LoggingInterceptor> _logger;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _logger.LogDebug("Request: " + context.Method); // 打印请求内容
            var response = await continuation(request, context);
            // _logger.LogDebug("Response: " + response.ToString()); // 打印响应内容
            return response;
        }
    }

}
