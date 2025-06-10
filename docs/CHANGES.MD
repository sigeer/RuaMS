- `async/await` 的处理，如果使用`async/await`，js脚本部分也要进行相应的调整，需要大量修改。改为同步调用
- Java中的byte，对应C#中的sbyte，在网络请求中，byte与sbyte没有区别。但是作为数值参与计算时，两者有区别。
- 移除`GuildSummary`
- 角色在线功能的判定：原先的`lodgeIn`字段被移除，通过`Client`的实现来判断
- 通过分部类将`Character`按功能拆分 （未完成）
- 提取了一些静态方法到`XXXManager`中
- 提取了一些接口
- Party重命名为Team
- 调整了PlayerStorage，通过`OnChannelAddPlayer`事件更新WorldPlayerStorage
	- 修改了机制，之前PlayerStorage中只会存放在线的角色。现在改为存放所有关联到的角色，所有角色都从`WorldStorage`中获取。所以之前通过null判断在线的，还需要进行在线判断
- 移除`GuildCharacter`、`PartyCharacter`
	- 还有一些Party.members循环的null判断，但是并没有看到为null的情况（下线不会置null）
- `Gachapon` 相关数据改为从数据库读取

##### 2024年11月20日
- worldconfig 从配置文件 移到数据库。另外，之前的逻辑中，worldId 是基于worldCount的。新的逻辑中不再有这种关联，可能成为隐患
- 由于代码迁移不知道什么时候生成了一个`drop_data.mobid1`的索引导致报错，**需要删库重新运行**

##### 2024年11月21日
- 为了能够通过配置调整服务器，对Server - World -WorldChannel 相关代码进行了调整。考虑到服务器修改是一个低频次的操作，移除了一些相关的锁，后续需要修改的话，可以使用一些线程安全的集合，当前搁置

##### 2025年2月10日
- 初始化数据通过数据库迁移完成，此前插入的数据由于不在迁移中，**需要删库重新运行**

##### 2025年3月4日
- 提取了World、Channel类中的dojo、wedding相关代码


### Server、World、Channel拆分

#### 注意事项

目前都是用的单进程部署，Transport的实现都选择使用local实现，再加上代码没有物理隔离，看似是可以直接交互的，
但是为了后期的拆分不能这样做  
同样 有些代码是静态方法，也影响了一些调用的判断

#### 交互流程

不再需要分区，`Server`和`World`合并成新的 `MasterServer`  
对于`Party`， `Guild`等需要全服互通的数据，不允许频道直接更新，由于目前代码没有进行物理隔离，可以直接互相取到数据，需要注意
1. 在`WorldChannel中调用（相当于原先的`world.xxx`）
2. 通过`Transport`对`MasterServer`发起请求
3. `MasterServer`更新后，再通过`Transport`更新到`Channel`

#### 登录相关

登录的client、session的相关代码没有看懂，暂时是将其保留在LoginClient相关操作里（可能是为了反作弊，但是不了解相关情况暂且搁置）

正常来说登录相关限制、控制有LoginClient就可以了，ChannelClient只需要在PlayerLogdin时确保和LoginClient是同一个人操作就行

#### 数据保存

原逻辑：`saveCharToDB`直接更新数据库。  
新逻辑：`saveCharToDB`推送给`MasterServer`，`MasterServer`更新缓存
并标记需要更新角色id，不过对于`Account`相关的数据则是立即保存到数据库。`MasterServer`然后再定时保存或者特殊事件保存到数据库。

#### 组队相关

- 移除了队伍搜索
- 移除了`MapleMap`中的`mapParty`
- 移除`CharacterEntity`的`Party`属性：组队数据并没有保存到数据库，这意味着重启服务器后组队数据消失，但是`Party`却仍有值。`Party`将与组队数据一起保存在内存中。

#### 新的内容

##### 增加`ChannelServerContainer`，也就是原先的`MultiRunner`

可能存在一台机器（一个进程）上部署多个频道的情况，此时`MasterServer`广播时，没必要对这N个`WorldChannel`请求N次，而是在传给`WorldChannelServer`再本地处理  
也就是服务器的请求节点不再是`MasterServer`与`WorldChannel`，而是`MasterServer`与`WorldChannelServer`。 
wz资源，在同一进程时，没必要每个频道都读取一次 ，所以都会通过单例注入（`AddSingleton<SkillbookInformationProvider>()`） 
重新对`IServerBase`进行调整，区分功能范围