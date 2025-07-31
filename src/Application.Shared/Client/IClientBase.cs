using Application.Shared.Models;

namespace Application.Shared.Client
{
    public interface IClientBase : ISocketClient, IDisposable
    {

        /// <summary>
        /// 对于LoginClient而言，登录后为true
        /// 对于ChannelClient而言，进入游戏后为true
        /// </summary>
        bool IsOnlined { get; }
        bool IsActive { get; }
        /// <summary>
        /// 正在切换连接的服务器
        /// <para>从登录->频道服务器</para>
        /// <para>切换频道</para>
        /// <para>每次切换服务器都会创建新的client，这也意味着当IsServerTransition为true时，这个client也将停止使用</para>
        /// </summary>
        bool IsServerTransition { get; }
        AccountCtrl? AccountEntity { get; set; }
        int AccountId { get; }
        string AccountName { get; }
        int AccountGMLevel { get; }
        void SetCharacterOnSessionTransitionState(int cid);
    }
}
