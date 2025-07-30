# 开发笔记

记录一些代码、文件等

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
