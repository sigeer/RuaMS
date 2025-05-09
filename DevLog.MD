
## 脚本相关

### 事件脚本

__BaseEvent 包含了所有可能被调用的方法，所有事件的基类  

AreaBoss 地图BOSS  
__BasePQ 组队任务  
__BaseTransport 交通工具  
__BasePrivateTransport 单人交通工具  
__BaseChallenge 单人事件  


### 代码实现

 - Channel 和 EventScriptManager 是一对一

 - EventScriptManager 和 EventManager（em） 是一对多，在服务器启动时生成，直接装载了所有event下的脚本（EventManager与event脚本一对一），并调用了init方法。
EventManager一般在js中通过`newInstance`，创建 EventInstanceManager（eim） ，可以将`EventInstanceManager`视作`EventManager`的实例，一对多

- NPCScriptManager 是单例，当与NPC对话时，调用`start`方法开启对话，同时生成 NPCConversationManager（cm）。
之后调用`action`，通过参数进入对话的不同阶段


#### 副本相关

##### 月妙

`/event/HenesysPQ.js`
入口：`/npc/1012112.js`

##### 废都

`/event/KerningPQ.js`
入口：`/npc/9020000.js`
过关NPC：`/npc/9020001.js`


##### 101层

`/event/LudiPQ.js`
入口：`/npc/2040034.js`
过关NPC：`/npc/2040036.js`~`/npc/2040044.js`

##### 艾琳森林

`/event/EllinPQ.js`
入口：`/npc/2133000.js`
传送点过关，脚本`/portal/party6_*.js`
部分关卡NPC：`/npc/2133001.js` 
跳跳关卡NPC：`/npc/2133004.js`
奖励：`/portal/party6_stage800.js`



```
BuffStat：状态

StatEffect ： 状态效果，可能来自技能、物品道具，可以附加多个BuffStat

BuffStatValueHolder：玩家身上的状态
```

### Server、World、Channel拆分

不在需要分区，`Server`和`World`合并成新的 `MasterServer`  
对于`Party`， `Guild`等需要全服互通的数据，不允许频道直接更新，由于目前代码没有进行物理隔离，可以直接互相取到数据，需要注意
1. 在`WorldChannel中调用（相当于原先的`world.xxx`）
2. 通过`Transport`对`MasterServer`发起请求
3. `MasterServer`更新后，通过`Transport`推送给所有`Channel`
4. `Channel`接受到请求后完成更新

当单进程部署时，Transport的实现都选择使用local实现