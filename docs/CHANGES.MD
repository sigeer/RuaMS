## 改动内容

### Server

不再分区，`Server`和`World`重构成 `MasterServer` `WorldChannelServer`。 所有的数据库操作在`MasterServer`进行。  
`MasterServer`和`WorldChannelServer`可以部署在不同机器，也可以同时在多个机器上部署多个`WorldChannelServer`。  
数据传输通过： 
- `IChannelServerTransport` （`WorldChannelServer` -> `MasterServer`）
- `ChannelServerNode`(`MasterServer` -> `WorldChannelServer`）


### Client

`Client`重构成3个不同的实现：
- `LoginClient`：此时客户端与 `MasterServer` 连接
- `ChannelClient`：此时客户端与`WorldChannelServer`连接
- `OfflineClient`：离线

### 数据库支持

增加Sqlite支持。如果需要支持更多数据库，只需要使用相应EF Provider，并生成迁移即可。

### 数据保存

原逻辑：`saveCharToDB`直接更新数据库。  
新逻辑：`saveCharToDB`推送给`MasterServer`，`MasterServer`更新缓存 并标记需要更新角色id。  
然后再定时保存或者特殊事件保存到数据库。

### WZ读取

大部分wz读取不再通过`XMLDomMapleData`读取`Data`，而是使用`Application.Templates.XmlWzReader`读取成确切的对象。保留旧代码用于测试。

### 脚本

- 实现了[BeiDou](https://github.com/BeiDouMS/BeiDou-Server/wiki/%E5%8C%97%E6%96%97NextLevel%E8%84%9A%E6%9C%AC%E6%A1%86%E6%9E%B6) 的`NextLevel`框架。  
- 尝试使用 NLua + lua。由于脚本数量较多，计划逐步更新，但是在两种脚本混用时出现非托管错误，暂时搁置。

### 登录

登录的client、session的相关代码没有看懂，暂时是将其保留在LoginClient相关操作里（可能是为了反作弊，但是不了解相关情况暂且搁置） 

正常来说登录相关限制、控制有LoginClient就可以了，ChannelClient只需要在PlayerLogdin时确保和LoginClient是同一个人操作就行

移除`Account`,`Character`中的大量字段：大部分字段仅在服务器运行时有效，或是未被使用  
新增表`account_bindings` 用于记录登录信息

### 邀请相关

- 重构统一，交易邀请仍旧使用旧逻辑。
- 回复邀请`AnswerInvite`原逻辑会使用第二个参数进行验证，一些不方便的使用-1跳过验证

### 队伍/家族

- 由于`WorldChannelServer`可以不同机器上部署，意味着不同频道的玩家可能处于不同服务器，
而队伍、家族等组织的成员可以处于不同频道，所以无法继续使用`PartyCharacter`,`GuildCharacter`，类似跨频道通信功能都需要重构  
- **队伍搜索** 由于不太了解，目前被移除
- 移除`Character.Party`：组队数据并没有保存到数据库，这意味着重启服务器后组队数据消失，他将只需要保存在内存中。

### 好友

简化邀请（移除`Pendding`字段）

### 玩家商店

- 移除`Character.HasMerchant`，重启服务器后必定是关店的，所以数据库不需要这个字段
- 雇佣商人的过期时间将依赖使用的道具的过期时间（原逻辑是固定1天）
- 移除“商品全部售完后，进行一些操作（整理、离开）都会触发关店”
- 移除“雇佣商人店主在离开店铺后清空消息”

### 模块

将一些可选择使用的功能，作为模块提取出来（`Application.Modules.XXX`），已被提取的模块有
- Duey * 
- BBS
- Family
- Fishing
- Maker * 
- Marriage
- PlayerNPC * 
- ExpeditionBossLog * 

### 扭蛋机

奖池数据移到了数据库。
调整`storage`表用于扩展存放奖品，解决抽奖的背包空间验证BUG，使用仓库的UI展示奖品

### 婚姻系统

大部分流程与原先一致

1. 修改了一些文本
2. 移除婚礼排队（当前的架构排队有点麻烦）
3. 婚姻表修改、保留所有婚姻记录（离婚不删除）
4. 移除`PartnerId`字段、通过`marriage`表来找到配偶
5. 婚姻状态：原先通过有无结婚戒指`marriageRing != null && PartnerId > 0`来判断、现在通过`marriage.Status`判断。婚姻状态将以`marriage`为主。

婚姻同时涉及到 `marriage`, `ring`，也许可能存在婚姻状态尚存，戒指丢失的情况。或者婚姻状态结束，戒指没有销毁的情况。

### EventManager

重构`EventManager`和`EventInstanceManager`，对各种事件进行分类。目的是自定义事件时方便扩展。

- `EventManager` 不会创建副本的事件（比如各种传送）
- `AbstractInstancedEventManager` 会创建副本的事件的抽象类
- `GuildQuestEventManager`：家族任务
- `ExpeditionEventManager`：远征队
- `SoloEventManager`：单人副本
- `PartyQuestEventManager`：组队任务
- `MonsterCarnivalEventManager`：怪物嘉年华