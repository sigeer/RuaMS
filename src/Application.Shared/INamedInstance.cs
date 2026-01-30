namespace Application.Shared
{
    public interface INamedInstance
    {
        /// <summary>
        ///供日志、指标等功能使用的名称：包含了一些服务器信息比如端口号
        /// </summary>
        string InstanceName { get; }
    }
}
