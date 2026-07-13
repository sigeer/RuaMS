---
name: refactor-from-js
description: "将JavaScript脚本用C#重写"
---

## 总览

本项目将MapleStory游戏脚本从JavaScript重写为C#。所有C#脚本类都是通过 `ScriptService` 注册的，使用 `TypeUtils.ExtractMethodsToDictionary` 通过方法名或 `[ScriptName]` 特性将脚本ID映射到C#方法。

### 核心机制：方法注册

`TypeUtils.ExtractMethodsToDictionary` 扫描指定类型的所有实例方法，构建 `Dictionary<string, MethodInfo>`：
- 如果方法有 `[ScriptName("name1", "name2")]` 特性，则特性中指定的每个名称都会作为键注册
- 否则使用方法名作为键

```csharp
[ScriptName("08_xmas_out")] // 通过特性指定脚本名称
public async Task<bool> _08_xmas_out() { ... }

public async Task<bool> advice00() { ... } // 方法名 advice00 就是键
```

### 脚本类型一览

| 类型 | JS目录 | C#类 | 上下文对象 | JS入口函数 | C#返回值 |
|------|--------|------|-----------|-----------|---------|
| NPC | `scripts/npc/` | `NpcScript` (partial) | `cm` | `start()` + `action(mode,type,selection)` | `Task` |
| Quest | `scripts/quest/` | `QuestScript` (partial) | `qm` | `start(mode,type,selection)` / `end(mode,type,selection)` | `Task` |
| Portal | `scripts/portal/` | `PortalScript` | `pi` | `enter(pi)` | `Task<bool>` |
| Item | `scripts/item/` | `ItemScript` | `im` | `start()` + `action(mode,type,selection)` | `Task` |
| Reactor Act | `scripts/reactor/` | `ReactorActScript` | `rm` | `act()` | `Task` |
| Reactor Hit | `scripts/reactor/` | `ReactorHitScript` | `rm` | `hit()` | `Task` |
| Reactor Touch | `scripts/reactor/` | `ReactorTouchScript` | `rm` | `touch()` | `Task` |
| Reactor Untouch | `scripts/reactor/` | `ReactorUntouchScript` | `rm` | `untouch()` | `Task` |
| Map Enter | 无JS | `MapEnterScript` | - | - | `Task` |
| Map FirstEnter | 无JS | `MapFirstEnterScript` | - | - | `Task` |

### 通用规则

- C# 方法体中有 `// TODO` 的需要重写实现，没有 `TODO` 的说明已处理，跳过
- 如果对应的JS脚本不存在，则跳过
- 对字符串中的英文进行汉化（保留特殊替换域 `#b`、`#k`、`#n`、`#e`、`#r`、`#t`、`#m`、`#i`、`#p`、`#f`、`#s`、`#v`、`#z` 等）
- C# 中没有 `var` 定义 `status` 变量和 `action` 回调的概念，直接使用 `await` 等待用户交互
- JS 中的 `cm.xxx()` / `qm.xxx()` / `pi.xxx()` / `rm.xxx()` / `im.xxx()` 调用在 C# 中直接对应 `await Xxx()` 或 `Xxx()` 实例方法，无需前缀
- 生成代码要符合C#语法规范，使用帕斯卡命名法，提高可读性
- **提取公共方法**：当多个方法共享相同模式，只有参数不同时，提取私有辅助方法，用 expression-bodied 简化。差异较大的方法不强行合并
- **文件对应一定要准确**

---

## NPC 脚本

### 范围

JS: `/src/Application.Resources/scripts/npc/*.js`
C#: `/src/Application.Plugin.Script/Npc/*.cs` (partial class `NpcScript`)

### 映射关系

`NpcScript` 是一个 `partial class`，分散在多个文件中，每个文件按功能领域拆分（职业、PQ、Boss等）。

**查找方式**：方法上的注释 `// Npc: {npc_id}` 标明了对应的NPC ID。

```
npc_id.js → C# 中拥有 `// Npc: {npc_id}` 注释的方法
```

多个NPC ID可以映射到同一个C#方法：
```csharp
// Npc: 1002002, 2010005, 2040048 
public async Task florina2() { ... }
```

### JS → C# 转换模式

#### context对象

| JS | C# |
|----|----|
| `cm.xxx()` | `await Xxx()` 或 `Xxx()` (直接调用基类方法) |
| `cm.getPlayer()` | `getPlayer()` |
| `cm.getJobId()` | `getJobId()` |
| `cm.getMeso()` | `getMeso()` |
| `cm.getLevel()` | `getLevel()` |
| `cm.getMapId()` | `getMapId()` |
| `cm.haveItem(id)` | `haveItem(id)` |
| `cm.canHold(id)` | `canHold(id)` |
| `cm.gainItem(id, n)` | `await gainItem(id, n)` |
| `cm.gainMeso(n)` | `await gainMeso(n)` |
| `cm.gainExp(n)` | `await gainExp(n)` |
| `cm.warp(map, portal)` | `await warp(map, portal)` |
| `cm.isQuestStarted(id)` | `isQuestStarted(id)` |
| `cm.isQuestCompleted(id)` | `isQuestCompleted(id)` |
| `cm.startQuest(id)` | `await startQuest(id)` |
| `cm.completeQuest(id)` | `await completeQuest(id)` |
| `cm.forceStartQuest()` | `await forceStartQuest(id)` |
| `cm.forceCompleteQuest()` | `await forceCompleteQuest(id)` |
| `cm.dispose()` | 不需要（方法结束自动dispose） |

#### 对话方法对照

| JS (cm) | C# |
|---------|----|
| `cm.sendOk(text)` | `await SayOK(text)` |
| `cm.sendNext(text)` | `await SayNext(text)` |
| `cm.sendYesNo(text)` | `await AskYesNo(text)` → 返回 `bool` |
| `cm.sendAcceptDecline(text)` | `await SayAcceptDecline(text)` → 返回 `bool` |
| `cm.sendSimple(text)` | `await AskMenu(text, options?)` → 返回 `int` |
| `cm.sendStyle(text)` | `await AskAvatar(text)` → 返回 `int` |
| `cm.sendGetNumber(text, def, min, max)` | `await AskNumber(text, def, min, max)` → 返回 `int` |
| `cm.sendGetText(text)` | `await AskText(text)` → 返回 `string` |
| `cm.sendNext(text) + cm.sendPrev(text)` | `await SaySpeech(texts)` → 传入 `string[]` |
| `cm.sendOk(text)` + `cm.sendOk(text)` | `await SayOK(text)` (连续调用) |

### JS 案例

```js
// npc/10200.js
var status = -1;

function start() {
    cm.sendNext("弓箭手具有灵巧和力量的天赋...");
}

function action(mode, type, selection) {
    status++;
    if (mode != 1) {
        if (mode == 0) {
            cm.sendNext("如果你想体验成为一个弓箭手的感觉，再来找我吧。");
        }
        cm.dispose();
        return;
    }
    if (status == 0) {
        cm.sendYesNo("你想体验一下成为一个弓箭手是什么感觉吗？");
    } else if (status == 1) {
        cm.lockUI();
        cm.warp(1020300, 0);
        cm.dispose();
    }
}
```

### C# 重写结果

```csharp
// Npc: 10200 
public async Task job_10200()
{
    await SayNext("弓箭手具有灵巧和力量的天赋...");
    if (await AskYesNo("你想体验一下成为一个弓箭手是什么感觉吗？"))
    {
        await lockUI();
        await warp(1020300, 0);
    }
    else
    {
        await SayNext("如果你想体验成为一个弓箭手的感觉，再来找我吧。");
    }
}
```

### 状态机简化

JS 使用 `status` 变量+ `action(mode,type,selection)` 回调实现多步对话。C# 通过 `await` 直接按顺序书写：

```js
// JS: 三步对话
var status = -1;
function start() { cm.sendNext("第一步"); }
function action(mode, type, selection) {
    if (mode == 1) status++;
    else { cm.dispose(); return; }
    if (status == 0) cm.sendNext("第二步");
    else if (status == 1) cm.sendOk("第三步");
    else cm.dispose();
}
```

```csharp
// C#: 直接顺序书写
public async Task example()
{
    await SayNext("第一步");
    await SayNext("第二步");
    await SayOK("第三步");
}
```

### 特殊情况处理

- 未注册的NPC：在 `NpcScript.cs` 中用 `n{npc_id}` 作为方法名创建新方法，并添加 `// Npc: {npc_id}` 注释
- 同一个方法映射多个NPC：注释行写 `// Npc: id1, id2, id3`，如果同时存在这多个NPC脚本，观察他们是否相同，如果相同，则取其中任意一份；如果不同，则不修改并注释说明；如果只有其中一个存在脚本，则正好使用这个NPC脚本重写
- 如果找不到匹配的JS文件，方法体内写 `// TODO` + 简单注释（记录NPC ID和原始功能描述）

---

## Quest 脚本

### 范围

JS: `/src/Application.Resources/scripts/quest/*.js`
C#: `/src/Application.Plugin.Script/Quest/*.cs` (partial class `QuestScript`)

### 映射关系

`QuestScript` 也是 `partial class`，按任务区域拆分。

```
quest_id.js → C# `q{quest_id}s` (start) / `q{quest_id}e` (end) 方法
```

方法注释标记：`// Quest: {quest_id}`

### 转换规则

JS 中任务脚本使用 `qm` 上下文，方法签名通常为 `start(mode, type, selection)`（可能同时包含开始和完成的逻辑，也可能没有独立的 `end` 函数）。

C# 分为 `q{id}s`（开始）/ `q{id}e`（完成）两个方法：

| JS `qm` | C# |
|---------|-----|
| `qm.forceStartQuest()` | `await forceStartQuest()` |
| `qm.forceCompleteQuest()` | `await forceCompleteQuest()` |
| `qm.gainItem(id, n)` | `await gainItem(id, n)` |
| `qm.gainExp(n)` | `await gainExp(n)` |
| `qm.getQuestStatus(id)` | `getQuestStatus(id)` |
| `qm.getQuestProgress(id)` | `getQuestProgress(id)` |

对话方法与NPC脚本一致（继承自同一基类）。

### 案例

```js
// quest/20000.js
function start(mode, type, selection) {
    if (mode == -1) { qm.dispose(); }
    else {
        if (mode > 0) status++;
        else status--;
        if (status == 0)
            qm.sendNext("啊，你来了。。。");
        else if (status == 3) {
            qm.gainItem(1142065, 1);
            qm.gainExp(20);
            qm.forceStartQuest();
            qm.forceCompleteQuest();
            qm.dispose();
        }
    }
}
```

```csharp
// Quest: 20000
public async Task q20000s()
{
    await SayNext("啊，你来了。。。");
    await SayNext("对抗想吞没整个枫叶世界的黑魔法师的邪恶本性...");
    await SayOK("但我不担心这些。我相信你一定能战胜这一切...");
    await gainItem(1142065, 1);
    await gainExp(20);
    await forceStartQuest();
    await forceCompleteQuest();
}
```

### 注意事项

- 如果没有对应的JS文件，跳过
- 如果JS中没有 `end` 方法，则不需要创建 `q{id}e` 方法
- 方法名中 `{quest_id}` 是任务ID数字，直接拼接（如 `q20000s`）

---

## Portal 脚本

### 范围

JS: `/src/Application.Resources/scripts/portal/*.js`
C#: `/src/Application.Plugin.Script/PortalScript.cs`（单文件，非partial）

### 映射关系

portal 脚本通过文件名（不含`.js`）作为键查找：

```
portal_name.js → C# 中同名方法 或 [ScriptName("portal_name")] 特性标记的方法
```

### 转换规则

JS 入口函数 `enter(pi)`，返回 `true`（允许通过）或 `false`（阻止通过）。

C# 方法签名：`public async Task<bool> portalName()`，返回 `true/false`。

| JS `pi` | C# |
|---------|-----|
| `pi.warp(map, portal)` | `await warp(map, portal)` |
| `pi.playPortalSound()` | `await playPortalSound()` |
| `pi.blockPortal()` | `await blockPortal()` |
| `pi.showInstruction(msg, w, h)` | `await showInstruction(msg, w, h)` |
| `pi.showInfo(effect)` | `await showInfo(effect)` |
| `pi.containsAreaInfo(id, info)` | `containsAreaInfo(id, info)` |
| `pi.updateAreaInfo(id, info)` | `await updateAreaInfo(id, info)` |
| `pi.getMapId()` | `getMapId()` |

### 案例

```js
// portal/advice00.js
function enter(pi) {
    pi.showInstruction("您可以使用箭头键移动。", 250, 5);
    return true;
}
```

```csharp
public async Task<bool> advice00()
{
    await showInstruction("您可以使用箭头键移动。", 250, 5);
    return true;
}
```

### 特殊情况

- 某些 portal 名称为纯数字或特殊字符，可使用 `[ScriptName("08_xmas_out")]` 特性映射
- portal 名称不能以数字开头时，方法名可以任意命名，通过 `[ScriptName]` 特性指定实际portal名称
- 没有找到对应C#方法时，用portal名称作为方法名创建新方法

---

## Reactor 脚本

### 范围

JS: `/src/Application.Resources/scripts/reactor/*.js`
C#: 4个类，分别对应不同的触发阶段

| 触发阶段 | C#类 | JS函数 |
|---------|------|--------|
| Hit (被击打) | `ReactorHitScript` | `hit()` |
| Act (激活) | `ReactorActScript` | `act()` |
| Touch (玩家触碰) | `ReactorTouchScript` | `touch()` |
| Untouch (玩家离开) | `ReactorUntouchScript` | `untouch()` |

### 映射关系

```
reactor_id.js → C# 方法，通过 `// Reactor: {id}` 注释匹配
```

多个reactor ID可以映射到同一个C#方法：
```csharp
// Reactor: 1022002, 1032000, 1202000, 1202004 
public async Task EpisodeQuest0()
{
    await dropItems();
}
```

### 转换规则

JS `rm` 上下文 → C# `this`（`ReactorActionManager` 基类）：

| JS `rm` | C# |
|---------|-----|
| `rm.dropItems()` | `await dropItems()` |
| `rm.dropItems(bool f, int r, int m, int M)` | `await dropItems(f, r, m, M)` |
| `rm.spawnMonster(id)` | `await spawnMonster(id)` |
| `rm.spawnMonster(id, n)` | `await spawnMonster(id, n)` |
| `rm.warp(map, portal)` | `await warp(map, portal)` |
| `rm.getMap()` | `getMap()` |
| `rm.getReactor()` | `getReactor()` |
| `rm.getEventInstance()` | `getEventInstance()` → 返回 `AbstractEventInstanceManager?` |
| `rm.GetEventInstanceTrust()` | `GetEventInstanceTrust()` → 非null或抛异常 |
| `eim.getIntProperty(key)` | `eim.getIntProperty(key)` |
| `eim.setIntProperty(key, value)` | `eim.setIntProperty(key, value)` |
| `rm.mapMessage(c, msg)` | `await mapMessage(c, msg)` |
| `rm.getMap().killAllMonsters(bool)` | `await getMap().killAllMonsters()` (C#无参数) |
| `rm.getMap().toggleEnvironment(name)` | `await getMap().toggleEnvironment(name)` |

### ⚠️ 关键：Touch/Untouch 的计数器模式（勿简化！）

Touch 和 Untouch 脚本**必须配对**，通过事件实例属性计数器跟踪玩家触碰数量：

**火焰/机关开关模式**：仅在第一个玩家触碰时触发（ON），最后一个玩家离开时恢复（OFF）：

```javascript
// JS: 火焰切换（完整模式）
var fid = "glpq_f0";

function touch() {
    var eim = rm.getEventInstance();
    if (eim.getIntProperty(fid) == 0) {  // 没人碰 → 开火
        action();
    }
    eim.setIntProperty(fid, eim.getIntProperty(fid) + 1);  // 计数器+1
}

function untouch() {
    var eim = rm.getEventInstance();
    if (eim.getIntProperty(fid) == 1) {  // 最后一人离开 → 关火
        action();
    }
    eim.setIntProperty(fid, eim.getIntProperty(fid) - 1);  // 计数器-1
}

function action() {
    var flames = Array("a1", "a2", "b1", "b2", "c1", "c2");
    for (var i = 0; i < flames.length; i++) {
        rm.getMap().toggleEnvironment(flames[i]);
    }
}
```

```csharp
// Touch: 在 ReactorTouchScript 中
public async Task glpqflame0()
{
    var eim = getEventInstance();
    if (eim == null) return;

    var fid = "glpq_f0";
    if (eim.getIntProperty(fid) == 0)
    {
        string[] flames = ["a1", "a2", "b1", "b2", "c1", "c2"];
        for (var i = 0; i < flames.Length; i++)
        {
            await getMap().toggleEnvironment(flames[i]);
        }
    }
    eim.setIntProperty(fid, eim.getIntProperty(fid) + 1);
}

// Untouch: 在 ReactorUntouchScript 中
public async Task glpqflame0()
{
    var eim = getEventInstance();
    if (eim == null) return;

    var fid = "glpq_f0";
    if (eim.getIntProperty(fid) == 1)
    {
        string[] flames = ["a1", "a2", "b1", "b2", "c1", "c2"];
        for (var i = 0; i < flames.Length; i++)
        {
            await getMap().toggleEnvironment(flames[i]);
        }
    }
    eim.setIntProperty(fid, eim.getIntProperty(fid) - 1);
}
```

**阈值模式**：计数器达到指定值时触发：

```javascript
var fid = "glpq_s";

function touch() {
    var eim = rm.getEventInstance();
    if (eim.getIntProperty(fid) == 5) {
        action();  // 5人触碰 → 激活
    }
    eim.setIntProperty(fid, eim.getIntProperty(fid) + 1);
}

function action() {
    rm.mapMessage(6, "All stirges have disappeared.");
    rm.getMap().killAllMonsters(true);
    eim.setIntProperty(fid, 777);
}
```

```csharp
public async Task glpqstrge()
{
    var eim = getEventInstance();
    if (eim == null) return;

    var fid = "glpq_s";
    if (eim.getIntProperty(fid) == 5)
    {
        await mapMessage(6, "All stirges have disappeared.");
        await getMap().killAllMonsters();
        eim.setIntProperty(fid, 777);
    }
    eim.setIntProperty(fid, eim.getIntProperty(fid) + 1);
}
```

### ❗ 常见错误

1. **遗漏事件实例属性**：JS 中 `rm.getEventInstance()` 获取的事件实例用于存储跨触碰的计数器状态，C# 必须对应调用 `getEventInstance()`，不能省略。
2. **简化计数器逻辑**：touch 和 untouch 的 `getIntProperty` / `setIntProperty` 计数器是核心行为，不能去掉。简单的 `await toggleEnvironment()` 会导致多人同时触碰时火焰来回闪烁。
3. **Touch 和 Untouch 不配对**：JS 中 touch/untouch 共用同一个 `fid` 属性名和计数器，C# 中需要在 `ReactorTouchScript` 和 `ReactorUntouchScript` 各自实现对应的加减逻辑。
4. **事件实例可能为 null**：`getEventInstance()` 返回 nullable，必须判空。部分现有代码使用 `GetEventInstanceTrust()`（直接抛异常），但 touch/untouch 推荐使用 `getEventInstance()` + null check。

### 案例

```js
// reactor/9202000.js
function act() {
    rm.dropItems();
}
```

```csharp
// Reactor: 9202000 
public async Task boxItem0()
{
    await dropItems();
}
```

### 特殊情况

- 批量reactor映射同一方法：注释写 `// Reactor: id1, id2, id3`
- 有些reactor可能同时有多个阶段的JS脚本，需要分别映射到不同阶段的C#类
- 如果没有找到对应C#方法，用reactor ID作为方法名创建新方法，放在对应的C#类中
- **如果一个 JS 文件同时有 `touch()` 和 `untouch()` 但没有 `act()` / `hit()`**：说明该 reactor 只响应触碰事件，`ReactorActScript.cs` 和 `ReactorHitScript.cs` 中对应的空方法应保留（防止 "不支持的脚本" 异常），但要将 `// TODO` 改为说明注释

---

## Item 脚本

### 范围

JS: `/src/Application.Resources/scripts/item/*.js`
C#: `/src/Application.Plugin.Script/ItemScript.cs`（单文件）

### 映射关系

```
item_script_name.js → C# 中间名方法
```

### 转换规则

Item脚本的JS使用 `im` 上下文，入口函数 `start()` + `action(mode,type,selection)`，与NPC类似。但C#中 `ItemScript` 继承自 `NPCConversationManager`，使用同样的对话方法。

| JS `im` | C# |
|---------|-----|
| `im.getMapId()` | `getMapId()` |
| `im.getMap()` | `getMap()` |
| `im.getPlayer()` | `getPlayer()` |
| `im.isQuestStarted(id)` | `isQuestStarted(id)` |
| `im.isQuestCompleted(id)` | `isQuestCompleted(id)` |
| `im.startQuest(id)` | `await startQuest(id)` |
| `im.completeQuest(id)` | `await completeQuest(id)` |
| `im.removeAll(id)` | `await removeAll(id)` |
| `im.showInfo(eff)` | `await showInfo(eff)` |
| `im.dropMessage(c, msg)` | `await Pink(msg)` / `await LightBlue(msg)` |
| `im.message(msg)` | `await playerMessage(5, msg)` |
| `im.dispose()` | 不需要 |

### 案例

```js
// item/killarmush.js
function start(){
    if (im.getMapId() == 106020300) {
        var portal = im.getMap().getPortal("obstacle");
        if (portal != null && portal.getPosition().distance(im.getPlayer().getPosition()) < 240) {
            if (!(im.isQuestStarted(100202) || im.isQuestCompleted(100202))) {
                im.startQuest(100202);
            }
            im.removeAll(2430014);
            im.showInfo("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon2");
            im.dropMessage(6,'好像有什么动静...嗯？是结界被消除了');
        } else {
            im.message('尽可能的接近魔法结界才能将其消除');
        }
    }
    im.dispose();
}
```

```csharp
public async Task killarmush()
{
    if (getMapId() == 106020300)
    {
        var portal = getMap().getPortal("obstacle");
        if (portal != null && portal.getPosition().distance(getPlayer().getPosition()) < 240)
        {
            if (!(isQuestStarted(100202) || isQuestCompleted(100202)))
            {
                await startQuest(100202);
            }
            await removeAll(2430014);
            await showInfo("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon2");
            await Pink("好像有什么动静...嗯？是结界被消除了");
        }
        else
        {
            await playerMessage(5, "尽可能的接近魔法结界才能将其消除");
        }
    }
    else
    {
        await playerMessage(5, "这里似乎没有需要消除的魔法结界");
    }
}
```

---

## 地图脚本

C# 中有两种地图脚本，没有对应的JS文件：

### MapEnterScript

当玩家进入地图时触发，对应WZ中的 `OnUserEnter` 属性。

映射方式：方法名以 `go{mapId}` 命名，或用 `[ScriptName("name")]` 特性。

```csharp
// Map: 10000 
public async Task go10000()
{
    await unlockUI();
    await mapEffect("maplemap/enter/10000");
}
```

### MapFirstEnterScript

当第一个玩家进入地图时触发，对应WZ中的 `OnFirstUserEnter` 属性。

```csharp
// Map: 103000800, 103000801, 103000802, 103000803, 103000804 
public async Task StageMsg_together()
{
    // TODO
}
```

---

## 通用API对照表

### 对话/消息方法

| JS | C# | 说明 |
|----|----|------|
| `cm.sendOk` | `await SayOK` | 纯文本对话框 |
| `cm.sendNext` | `await SayNext` | "下一步"按钮对话框 |
| `cm.sendYesNo` | `await AskYesNo` | 是/否选择，返回 `bool` |
| `cm.sendAcceptDecline` | `await SayAcceptDecline` | 接受/拒绝，返回 `bool` |
| `cm.sendSimple` | `await AskMenu` | 列表选择，返回 `int` |
| `cm.sendStyle` | `await AskAvatar` | 发型/外观选择，返回 `int` |
| `cm.sendGetNumber` | `await AskNumber` | 数字输入，返回 `int` |
| `cm.sendGetText` | `await AskText` | 文本输入，返回 `string` |
| `cm.sendNext + sendPrev` | `await SaySpeech` | 多页对话 |
| `cm.sendNext + sendOk` | `await SayNext` + `await SayOK` | 连续对话 |

### 物品/货币操作

| JS | C# | 说明 |
|----|----|------|
| `cm.gainItem(id, n)` | `await gainItem(id, n)` | 获取/移除物品 |
| `cm.haveItem(id)` | `haveItem(id)` | 检查是否有物品 |
| `cm.canHold(id)` | `canHold(id)` | 检查背包空间 |
| `cm.gainMeso(n)` | `await gainMeso(n)` | 增减金币 |
| `cm.getMeso()` | `getMeso()` | 获取金币数 |
| `cm.removeAll(id)` | `await removeAll(id)` | 移除所有指定物品 |

### 任务操作

| JS | C# | 说明 |
|----|----|------|
| `cm.isQuestStarted(id)` | `isQuestStarted(id)` | 任务是否进行中 |
| `cm.isQuestCompleted(id)` | `isQuestCompleted(id)` | 任务是否已完成 |
| `cm.getQuestStatus(id)` | `getQuestStatus(id)` | 获取任务状态（0/1/2） |
| `cm.forceStartQuest()` | `await forceStartQuest()` | 强制启动任务 |
| `cm.forceCompleteQuest()` | `await forceCompleteQuest()` | 强制完成任务 |
| `cm.startQuest(id)` | `await startQuest(id)` | 启动任务（需判断） |
| `cm.completeQuest(id)` | `await completeQuest(id)` | 完成任务（需判断） |

### 地图/移动

| JS | C# | 说明 |
|----|----|------|
| `cm.warp(map, portal)` | `await warp(map, portal)` | 传送 |
| `cm.getMapId()` | `getMapId()` | 当前地图ID |
| `cm.getMap()` | `getMap()` | 当前地图对象 |

### 玩家信息

| JS | C# | 说明 |
|----|----|------|
| `cm.getPlayer()` | `getPlayer()` | 玩家对象 |
| `cm.getLevel()` | `getLevel()` | 等级 |
| `cm.getJobId()` | `getJobId()` | 职业ID |
| `cm.getGender()` | `getGender()` | 性别 |

### 特效/UI

| JS | C# | 说明 |
|----|----|------|
| `cm.lockUI()` | `await lockUI()` | 锁定UI |
| `cm.unlockUI()` | `await unlockUI()` | 解锁UI |
| `cm.mapMessage(c, msg)` | `await mapMessage(c, msg)` | 地图广播消息 |
| `cm.showInfo(eff)` | `await showInfo(eff)` | 播放特效 |
| `cm.changeMusic(bgm)` | `await changeMusic(bgm)` | 更换背景音乐 |

---

## 执行流程

1. 遍历 `/src/Application.Resources/scripts/npc/*.js` 和 `/src/Application.Resources/scripts/quest/*.js`
2. 对每个JS文件：
   - 在对应的C#类中查找匹配的方法
   - 如果找到的方法体中有 `// TODO`，则根据JS逻辑重写
   - 如果没有找到方法，创建新方法并实现
   - 如果找到的方法没有 `// TODO`，跳过（已处理）
3. 遍历 `/src/Application.Resources/scripts/portal/*.js`
4. 遍历 `/src/Application.Resources/scripts/reactor/*.js`
5. 遍历 `/src/Application.Resources/scripts/item/*.js`

### 创建新方法的命名规则

| 脚本类型 | 方法命名规则 | 注释标记 |
|---------|-------------|---------|
| NPC | `n{npc_id}` | `// Npc: {npc_id}` |
| Quest (start) | `q{quest_id}s` | `// Quest: {quest_id}` |
| Quest (end) | `q{quest_id}e` | `// Quest: {quest_id}` |
| Portal | portal名称（或 `[ScriptName]`） | 无 |
| Reactor (act) | reactor ID | `// Reactor: {id}` |
| Reactor (hit) | reactor ID | `// Reactor: {id}` |
| Item | JS文件名（不含扩展名） | 无 |
