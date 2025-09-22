using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Application.Protos
{
    public class GlobalHeaderInterceptor : Interceptor
    {
        private readonly string _key;
        private readonly string _value;

        public GlobalHeaderInterceptor(string key, string value)
        {
            _key = key;
            _value = value;
        }
        private Metadata MergeHeaders(Metadata? existing)
        {
            var headers = new Metadata();
            if (existing != null)
            {
                foreach (var entry in existing)
                    headers.Add(entry);
            }
            headers.Add(_key, _value);
            return headers;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = MergeHeaders(context.Options.Headers);

            var newOptions = context.Options.WithHeaders(headers);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

            return continuation(request, newContext);
        }
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = MergeHeaders(context.Options.Headers);

            var newOptions = context.Options.WithHeaders(headers);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);

            return continuation(request, newContext);
        }
    }
}
