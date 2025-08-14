using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Application.Protos
{
    public class TimeoutInterceptor : Interceptor
    {
        private readonly TimeSpan _defaultTimeout;

        public TimeoutInterceptor(TimeSpan defaultTimeout)
        {
            _defaultTimeout = defaultTimeout;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var options = context.Options.WithDeadline(DateTime.UtcNow + _defaultTimeout);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host, options);
            return continuation(request, newContext);
        }
    }
}
