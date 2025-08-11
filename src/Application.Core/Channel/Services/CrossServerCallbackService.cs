using Application.Core.ServerTransports;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Application.Core.Channel.Services
{
    /// <summary>
    /// 第一个参数必须是IPlayer
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SupportRemoteCallAttribute : Attribute
    {
        public SupportRemoteCallAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
    public class RemoteCallParams
    {
        public RemoteCallParams(Dto.RemoteCallParamDto item)
        {
            Index = item.Index;
            Schema = item.Schema;
            Value = item.Value;
        }

        public int Index { get; set; }
        public string Schema { get; set; }
        public string Value { get; set; }
        public object GetValue()
        {
            if (Schema == typeof(int).Name)
                return int.Parse(Value);

            if (Schema == typeof(short).Name)
                return short.Parse(Value);

            if (Schema == typeof(long).Name)
                return long.Parse(Value);

            if (Schema == typeof(string).Name)
                return Value;

            return Value;
        }
    }
    public class CrossServerCallbackService
    {
        readonly AdminService _adminService;
        readonly IChannelServerTransport _transport;
        readonly ILogger<CrossServerCallbackService> _logger;

        Dictionary<string, MethodInfo> _cache = new();

        public CrossServerCallbackService(AdminService adminService, IChannelServerTransport transport, ILogger<CrossServerCallbackService> logger)
        {
            _adminService = adminService;
            _transport = transport;
            _logger = logger;
        }

        public void RunEventAfterLogin(IPlayer chr, RepeatedField<Dto.RemoteCallDto> list)
        {
            foreach (var item in list)
            {
                Invoke(item, chr);
            }
        }


        public void Invoke(Dto.RemoteCallDto data, IPlayer? chr = null)
        {
            if (!_cache.TryGetValue(data.CallbackName, out var method))
                method = _adminService.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.GetCustomAttribute<SupportRemoteCallAttribute>()?.Name == data.CallbackName);

            if (method == null)
            {
                _logger.LogWarning("RemoveCallService: 未找到方法, Name: {Name}", data.CallbackName);
                return;
            }

            var methodParams = method.GetParameters();
            var paramsValue = new object?[methodParams.Length];

            if (methodParams.Length == data.Params.Count)
            {
                foreach (var p in data.Params)
                {
                    var paramsObj = new RemoteCallParams(p);
                    paramsValue[paramsObj.Index] = paramsObj.GetValue();
                }
            }
            else if (methodParams.Length == data.Params.Count + 1)
            {
                paramsValue[0] = chr;
                foreach (var p in data.Params)
                {
                    var paramsObj = new RemoteCallParams(p);
                    paramsValue[paramsObj.Index + 1] = paramsObj.GetValue();
                }
            }
            else
            {
                _logger.LogWarning("RemoveCallService: 参数不匹配, Name: {Name}, MethodParamCount: {ParamsCount}, InputParamCount{InputCount}",
                    data.CallbackName, methodParams.Length, data.Params.Count);
                return;
            }

            method.Invoke(_adminService, paramsValue);
        }
    }
}
