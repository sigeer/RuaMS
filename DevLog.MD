# 开发笔记

记录一些代码、文件等

## 数据库迁移

```
// 在 Application.Core.EF.MySQL/Sqlite 中执行
dotnet ef migrations add [migration_name] --output-dir Migrations --startup-project ../Application.Host -- --DataBase=MySql/Sqlite

dotnet ef migrations remove --startup-project ../Application.Host -- --DataBase=MySql/Sqlite

dotnet ef database update --startup-project ../Application.Host -- --DataBase=MySql/Sqlite
```

## 发布

通过设置 -p:IsStandalone 切换输出版本

### master-standalone

内置`Application.Core.Channel.InProgress`，可通过设置`ChannelServerConfig`调整频道，也可通过设置`UseExtraChannel=true`接入其他频道服务器

### master

必须设置`UseExtraChannel=true`，不然毫无意义

### channel

通过设置`ChannelServerConfig.MasterServerGrpcAddress`连接到`MasterServer`

### docker（默认单机模式）

#### 生成镜像

`docker build -f ./src/Application.Host/Dockerfile-standalone -t ruams-standalone:latest -t ruams-standalone:0.0.1 .`

#### sqlite数据库备份 
`docker cp [container_id]:/app/database.db ./backup.db`

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

- Channel 和 NPCScriptManager 是一对一，当与NPC对话时，调用`start`方法开启对话，同时生成 NPCConversationManager（cm）。
之后调用`action`，通过参数进入对话的不同阶段


#### 副本相关

##### 月妙

`/event/HenesysPQ.js`
入口：`/npc/1012112.js`
过关NPC：`/npc/1012114.js`

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
离开传送：`/portal/party6_stage800.js`

```
em.startInstance -> script.setup -> em.newInstance
```
     
```
BuffStat：状态

StatEffect ： 状态效果，可能来自技能、物品道具，可以附加多个BuffStat

BuffStatValueHolder：玩家身上的状态
```

## 一些相似的方法

有些方法看起来似乎有着一样的功能，或者名字相近却不知道其功能的方法，一些简单已经被移除，还有一些复杂的记录在这里

### 检查背包空间

| 方法名|   备注 |
| ----------- |----------- |
| `chr.canHold`  | 调用`Inventory.checkSpot`（原先会调用`script.canHold`） |
| `script.canHold`  | 调用`Inventory.checkSpots` |
| `Inventory.checkSpot`  | 转 `Inventory.checkSpotsAndOwnership` |
| `Inventory.checkSpotsAndOwnership`  | 内部会调用`InventoryManipulator.checkSpaceProgressively` |
| `Inventory.checkSpots`  | 逻辑与`checkSpotsAndOwnership`相似，但是不包含owner检测 |
| `InventoryManipulator.checkSpace`  | 内部实现与`checkSpaceProgressively`相似 |
| `InventoryManipulator.checkSpaceProgressively`  | 返回值比`checkSpace`更丰富 |

### 获取物品

| 方法名| 固有道具验证| 背包空间验证|   备注 |
| ----------- |-----------|-----------|----------- |
| `InventoryManipulator.addById` |❌|❗ | 传入 itemId |
| `InventoryManipulator.addFromDrop`|✔|❗  | 传入`Item`，有背包空间验证，`short newSlot = inv.addItem(nItem);`，但是如果物品数量大于1，可能会获取到一部分物品 |
| `chr.GainItem`|✔|✔ | 会通过`InventoryManipulator.checkSpace`进行验证， 虽然传入的是itemId，但是调用`InventoryManipulator.addFromDrop` |

### 移除物品

| 方法名|   备注 |
| ----------- |----------- |
| `InventoryManipulator.removeFromSlot`  |  |
| `InventoryManipulator.removeById`  |  |

### 关闭雇佣商人

| 方法名|   备注 |
| ----------- |----------- |
| `forceClose`  | 内部会调用`closeOwnerMerchant` |
| `closeOwnerMerchant`  | 调用`closeShop` |
| `closeShop`  |  |
| `chr.closeHiredMerchant`  | 调用`forceClose` |

## 多语言

`ClientMessage`: 通过`serverNotice`,`yellowTip`,`TempConversation`方式传给客户端显示  
`SystemMessage`: 主要用于系统日志  
`ScriptTalk`: 主要用于脚本中的对话显示  

### 客户端需要

- [x] 请求`SetLanguageHandler`  
~~- [ ] 使用`UTF-8`编码~~ 做不到


### 替换域

```
#b = 蓝色  
说明: #b后面的字符串都用蓝色显示，用于你想要突出展示的字符串  
示例: 这颗 #b珠子#k 是进入扎昆副本的关键  

#r = 红色  
说明: #r后面的字符串都用红色显示，用于你想要突出展示的字符串  
示例: 这颗 #r珠子#k 是进入扎昆副本的关键  

#d = 紫色  
说明: #d后面的字符串都用紫色显示，用于你想要突出展示的字符串  
示例: 这颗 #d珠子#k 是进入扎昆副本的关键  

#g = 绿色  
说明: #g后面的字符串都用绿色显示，用于你想要突出展示的字符串  
示例: 这颗 #g珠子#k 是进入扎昆副本的关键  

#k = 黑色  
说明: 默认的字体就是黑色，所以#k常常用于把字体从其他颜色恢复成黑色  
示例: 这颗 #g珠子#k 是进入扎昆副本的关键  

#e = 加粗  
说明: #e后面的字符串都用加粗显示，用于你想要突出展示的字符串  
示例: 这颗 #e珠子#n 是进入扎昆副本的关键  

#n = 常规  
说明: #n把加粗或斜体转换成常规格式 
示例: 这颗 #e珠子#n 是进入扎昆副本的关键  

#h # = 玩家名称  
说明: 用于展示玩家的名称  
示例: 准备好了吗 #h # ，我会把你传送到射手村  

#L[数字]# = 选项开始  
说明: 数字从0开始依次往下+1  
示例: 选择一个职业进行转职: \r\n #L[0]# 冰雷 #l \r\n #L[1]# 火毒 #l \r\n #L[2]# 牧师 #l  

#l = 选项结束  
说明: 对应选项开始  
示例: 选择一个职业进行转职: \r\n #L[0]# 冰雷 #l \r\n #L[1]# 火毒 #l \r\n #L[2]# 牧师 #l  

#o[怪物id]# = 怪物名称  
说明: 用于展示怪物的名称  
示例: 打败 #o[怪物id]# 10只后，再来与我对话  

#m[地图id]# = 地图名称  
说明: 用于展示地图的名称  
示例: 去 #m[地图id]# 找到转职教官进行转职  

#p[npcid]# = npc名称  
说明: 用于展示npc的名称  
示例: 我爹是 #p[npcid]#  

#q[技能id]# = 技能名称  
说明: 用于展示技能的名称  
示例: 技能 #q[技能id]# 能够对怪物造成强大的群伤效果  

#s[技能id]# = 技能图片  
说明: 用于展示技能的图片  
示例: 技能 #s[技能id]# 能够对怪物造成强大的群伤效果  

#c[物品id]# = 物品数量  
说明: 用于展示这个物品在你的背包有多少个  
示例: 我需要10个 #t[物品id]#，你的背包只有 #c[物品id]# 个  

#t[物品id]# = 物品名称  
#z[物品id]# = 物品名称  
说明: 用于展示一个物品id对应的名称  
示例: 我需要10个 #t[物品id]#，你的背包只有 #c[物品id]# 个  

#i[物品id]# = 物品图片  
#v[物品id]# = 物品图片  
说明: 用于展示一个物品id对应的图片  
示例: 你获得了 #i[物品id]# * 1 \r\n #v[物品id]# * 3  

#B[%]# = 进度条  
说明: 用于展示一个进度条  
示例: 当前任务进展: #B[%]#  

#f[文件路径]# = 引用文件  
#F[文件路径]# = 引用文件  
说明: 从客户端的wz下取出资源文件展示  
示例: 你获得了 #fUI/UIWindow.img/QuestIcon/4/0# \r\n #v[物品id]# 3 #t[物品id]# \r\n #FUI/UIWindow.img/QuestIcon/8/0# 10 经验  

#j[key]# = 任务 QuestRecordEx表 替换域
说明：服务端通过 MessagePacket.QuestRecordEx(questId, value) 传给客户端，客户端维护QuestRecordEx表，value结构为k0=v0;k1=v1;k2=v2; 客户端通过键读取值

#R[infoNumber]# = 任务infoNumber值替换
见 Id=29004 “共5个区域：已完成#R27018# 地区。”

\r\n = 换行  
说明: 在信息的末尾加一个\r\n，后面的信息会换一行展示  
示例: 你获得了 #fUI/UIWindow.img/QuestIcon/4/0# \r\n #v[物品id]# 3 #t[物品id]# \r\n #FUI/UIWindow.img/QuestIcon/8/0# 10 经验  
```