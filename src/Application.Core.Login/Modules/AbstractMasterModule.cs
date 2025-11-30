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
        public virtual void OnPlayerLoad(DBContext dbContext, CharacterModel chrModel)
        {

        }
        /// <summary>
        /// 进入频道服务器
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isNewComer"></param>
        public virtual void OnPlayerLogin(CharacterLiveObject obj, bool isNewComer)
        {

        }
        /// <summary>
        /// 离开游戏
        /// </summary>
        /// <param name="obj"></param>
        public virtual void OnPlayerLogoff(CharacterLiveObject obj)
        {

        }
        /// <summary>
        /// 切换地图
        /// </summary>
        /// <param name="character"></param>
        public virtual void OnPlayerMapChanged(CharacterLiveObject character)
        {

        }
        /// <summary>
        /// 玩家职业变化
        /// </summary>
        /// <param name="character"></param>
        public virtual void OnPlayerJobChanged(CharacterLiveObject character)
        {

        }
        /// <summary>
        /// 玩家等级变化
        /// </summary>
        /// <param name="character"></param>
        public virtual void OnPlayerLevelChanged(CharacterLiveObject character)
        {

        }
        /// <summary>
        /// 进入商城
        /// </summary>
        /// <param name="character"></param>
        public virtual void OnPlayerEnterCashShop(CharacterLiveObject character)
        {

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
