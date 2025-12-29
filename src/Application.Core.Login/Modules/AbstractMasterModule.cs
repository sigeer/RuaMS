using Application.Core.Login.Models;
using Application.EF;
using Application.Utility.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Modules
{
    public abstract class AbstractMasterModule
    {
        protected readonly MasterServer _server;
        protected readonly ILogger<MasterModule> _logger;
        protected string _moduleName;
        protected AbstractMasterModule(MasterServer server, ILogger<MasterModule> logger)
        {
            _server = server;
            _logger = logger;

            _moduleName = GetType().Assembly.GetName().Name ?? "unknown";
        }

        /// <summary>
        /// 启动Master服务器时调用
        /// </summary>
        /// <returns></returns>
        public virtual Task InitializeAsync()
        {
            _logger.LogInformation("模块 {Name}：初始化", _moduleName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Master服务器注册定时任务时调用
        /// </summary>
        public virtual void RegisterTask(ITimerManager timerManager)
        {
            _logger.LogInformation("模块 {Name}：注册定时任务", _moduleName);
        }


        /// <summary>
        /// 停止Master服务器时调用
        /// </summary>
        /// <returns></returns>
        public virtual Task UninstallAsync()
        {
            _logger.LogInformation("模块 {Name}：卸载", _moduleName);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 首次数据库加载玩家
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="chrModel"></param>
        public virtual Task OnPlayerLoad(DBContext dbContext, CharacterModel chrModel)
        {
            return Task.CompletedTask;
        }

        public virtual async Task OnPlayerServerChanged(CharacterLiveObject obj, int lastChannel)
        {
            if (lastChannel == 0 && obj.Channel > 0)
            {
                await OnPlayerLogin(obj);
            }
            if (lastChannel != 0 && obj.Channel == 0)
            {
                await OnPlayerLogoff(obj);
            }
            if (obj.Channel == -1)
            {
                await OnPlayerEnterCashShop(obj);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="obj"></param>
        public virtual Task OnPlayerLogin(CharacterLiveObject obj)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 离开游戏
        /// </summary>
        /// <param name="obj"></param>
        public virtual Task OnPlayerLogoff(CharacterLiveObject obj)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// 切换地图
        /// </summary>
        /// <param name="character"></param>
        public virtual Task OnPlayerMapChanged(CharacterLiveObject character)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// 玩家职业变化
        /// </summary>
        /// <param name="character"></param>
        public virtual Task OnPlayerJobChanged(CharacterLiveObject character)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// 玩家等级变化
        /// </summary>
        /// <param name="character"></param>
        public virtual Task OnPlayerLevelChanged(CharacterLiveObject character)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// 进入商城
        /// </summary>
        /// <param name="character"></param>
        public virtual Task OnPlayerEnterCashShop(CharacterLiveObject character)
        {
            return Task.CompletedTask;
        }

        public virtual int DeleteCharacterCheck(int id)
        {
            return 0;
        }
        /// <summary>
        /// 删除玩家
        /// </summary>
        /// <param name="id"></param>
        public virtual void OnPlayerDeleted(int id)
        {

        }
    }
}
