using client.inventory;

namespace Application.Core.Client.inventory
{
    public interface IRelatedItemExpiration
    {
        Item SourceItem { get; }
        /// <summary>
        /// 过期触发
        /// </summary>
        void ExpiredInvoke();
    }
}
