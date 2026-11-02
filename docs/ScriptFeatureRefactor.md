# 脚本模块化重构：按功能聚合多类型脚本

> 本文档是本次重构的**进度记录**。上下文超出窗口或会话中断后，以本文档为准继续。
> 最后更新：2026-10-23（会话暂停点）

---

## 0. 状态速览

| 事项 | 状态 |
|---|---|
| 现状调研（脚本注册/分发/事件） | ✅ 完成 |
| 设计方案定稿 | ✅ 完成 |
| 基础设施实现（特性 + 发现器 + 注册表） | ✅ 完成 |
| 巴洛古简单模式迁移（参考实现） | ✅ 完成 |
| 大文件迁移（Portal / MapEnter / Npc 事件耦合部分） | ✅ 完成（事件耦合脚本已清零） |
| 诊断工具（GM 命令 / 启动期校验） | ✅ 完成（`!scripts` + WZ 基线校验） |
| 构建验证 | ✅ Core / Plugin.Script / Host 均 0 错误 |
| 测试 | ✅ `ServiceTest.Infrastructure` 252+ 通过；脚本相关 47 通过；WZ 校验 5 通过 |

基线：分支 `dev/script`，提交 `383070b4a`（"更新：完善脚本"）。
基线构建：`dotnet build src/Application.Host/Application.Host.csproj` → **0 错误 / 228 警告**（2026-10-23 实测）。

### 0.1 会话暂停时的待提交改动

主体改动已由用户提交为 `505812006`（"重构：脚本关联，不再按种类分散到不同文件"）。
暂停时工作区仍有 2 个文件未提交：

```text
 M src/Application.Plugin.Script/Features/Horntail/HorntailFeature.cs   # 新增 hontale_BtoB1
 M test/ServiceTest/Infrastructure/WZ/ScriptRegistrationCheck.cs        # 增加 Portal 种类校验
```

两者均已通过编译与 `ScriptRegistrationCheck`（5 通过）。

### 0.2 恢复会话后的第一步

1. `dotnet build src/Application.Host/Application.Host.csproj` 确认 0 错误；
2. `dotnet test --filter "FullyQualifiedName~ServiceTest.Infrastructure.Scripts" ./test/ServiceTest/ServiceTest.csproj`
   与 `--filter "FullyQualifiedName~ScriptRegistrationCheck"` 确认通过；
3. 从 §4「剩余」继续。

---

## 1. 问题

脚本按**类型**分注册表：NPC、Quest、Item、Portal、MapEnter、MapFirstEnter、ReactorHit/Act/Touch/Untouch，再加事件模板（`AbstractEventTemplate`）。同一功能的代码因此被拆散到互不相关的文件，彼此之间只有**字符串**联系，编译期无法发现断链。

### 1.1 典型案例：巴洛古简单模式

| 位置 | 内容 |
|---|---|
| `src/Application.Plugin.Script/Events/Expedition/Exped_Balrog_Easy.cs` | `AbstractExpeditionEventTemplate`，BossId 8830007、本体 8830010、EntryMap 105100400 |
| `src/Application.Plugin.Script/MapFirstEnterScript.cs:50` | `easy_balog_summon()`，通过 `mapObj.EventInstanceManager.GetTemplate<Exped_Balrog_Easy>()` 反查事件，再召唤 8830010 / 8830009 / 8830013 |
| `src/Application.Resources/wz/Map.wz/Map/Map1/105100400.img.xml:17` | `<string name="onFirstUserEnter" value="easy_balog_summon"/>` |

问题点：

1. **两处硬编码同一批数据**：mobId 8830010/8830009/8830013、坐标 `(412, 258)` 在事件与地图脚本里各写一份。
2. **运行期才校验的强类型假设**：地图脚本 `GetTemplate<Exped_Balrog_Easy>()` 找不到就抛 `ConversationDiffInstanceException`。
3. **迁移产物**：原 JS `scripts/event/BalrogBattle_Easy.js` **没有**对应地图脚本，召唤写在事件的 `afterSetup` 里。普通难度 `Exped_Balrog.cs` 至今仍在 `AfterSeup` 中召唤。`easy_balog_summon` 是移植时凭空拆出来的，逻辑与 `Exped_Balrog_Easy` 完全重叠。

---

## 2. 现有机制（调研结论）

### 2.1 加载与热更新

- `PluginManager`（`src/Application.Core/Gameplay/Plugins/PluginManager.cs`，每个 `WorldChannelServer` 一个）把 `Plugins/Application.Plugin.*.dll` 载入可回收 `AssemblyLoadContext`。
- `WorldChannelServer` 用 `FileSystemWatcher` 监听插件目录 → `LoadPlugin` / `UnloadPlugin`（`WorldChannelServer.cs:341` 起）。
- `RebuildRegistrations()` 重建 `服务接口 → 按优先级排序的实现[]`；`_scriptOwners` 缓存随之清空。

### 2.2 优先级裁决

- `PluginPriority { High=0, Normal=1, Low=2 }`，数字小者优先；同优先级按插件名排序（`RebuildRegistrations()`）。
- `InvokeScriptAsync<TService>` 取 `owners[0]` 执行，**只调用一次，不回退**；被压制的插件每个 `(插件, 脚本名)` 只告警一次。
- 内置插件 `ScriptService.Priority => PluginPriority.Low`，第三方插件默认 `Normal` 即可覆盖。

### 2.3 脚本名 → 实现

- `TypeUtils.LoadFromType(Type)`（`Gameplay/Plugins/ScriptNameAttribute.cs`）扫描 `DeclaredOnly|Public|Instance` 方法，键为 `[ScriptName(...)]` 的每个值，否则为方法名；重复键 `dict.Add` 抛异常。
- 键来自 WZ，不是文件名：NPC `script`（空则 `n{id}`）、Quest `startscript/endscript`、Item `spec/script`（空则 `item{id}`）、Portal 节点 `script`、Map `onUserEnter`/`onFirstUserEnter`、Reactor `action`。

### 2.4 事件侧

- `EventScriptManager`（每频道）持有 `ConcurrentDictionary<string, AbstractEventManager>`，按 `AbstractEventTemplate.Name` 索引；`ReloadEventScript(HashSet<AbstractEventTemplate>)` 重名直接抛 `BusinessFatalException`。
- 模板**唯一实例**被所有频道共享（`EventService._events` 在构造时 `new` 一次，`OnMounted()` 逐频道 `ReloadEventScript`，同一实例赋值给各频道的 `AbstractEventManager.Template`）。
- 目前模板清单硬编码在 `Events/EventService.cs` 构造函数中（约 120 项）。

### 2.5 脚本上下文对象

脚本方法无参数；`PluginManager` 用 `DynamicObjectFactory.Create(type, args...)` 构造上下文，再 `Method.Invoke(script, null)`。

| 类型 | 基类 | 构造参数 |
|---|---|---|
| `NpcScriptBase` | `NPCConversationManager` | `(IChannelClient, int npc, NPC?)` |
| `QuestScriptBase` | `QuestActionManager` | `(IChannelClient, Quest, int npc)` |
| `ItemScriptBase` | `NPCConversationManager` | `(IChannelClient, Item, int npcId)` |
| `PortalPlayerInteraction` | `AbstractPlayerInteraction` | `(IChannelClient, IPortal)` |
| `MapScriptMethods` | `AbstractPlayerInteraction` | `(IChannelClient, IMap)` |
| `ReactorActionManager` | `AbstractPlayerInteraction` | `(IChannelClient, IReactor)` |

`AbstractPlayerInteraction` 提供 `GetEventManager<T>(name)`、`GetEventInstanceTrust<T>()`、`openNpc(...)`。

---

## 3. 已确认的设计决策（用户拍板）

| # | 决策 | 结论 |
|---|---|---|
| 1 | 同一功能的多种脚本类型如何组织 | **单文件 + 嵌套类**：一个功能一个 `.cs`，仅一个顶层类型（符合「一文件一类型」），各脚本类型作为嵌套类，共享外层常量与私有方法 |
| 2 | 事件模板是否并入功能文件并由插件发现 | **是**：新增事件插件服务，各插件上报模板，按优先级解决同名冲突，热重载自动增删；`EventService` 中硬编码清单逐步清空 |
| 3 | 落地范围 | **基础设施 + 巴洛古参考实现 + 同时迁移主要大文件**（PortalScript / MapEnterScript / NpcScript 中与事件强耦合的部分） |

### 3.1 目标形态（示意）

```csharp
// 目标形态（后续演进为扁平类，见「扁平类改造」一节）
internal sealed class BalrogEasy
{
    internal const int BodyMobId = 8830010;
    internal const int SealedMobId = 8830007;
    static readonly Point SpawnPoint = new(412, 258);

    internal sealed class Event : AbstractExpeditionEventTemplate { /* AfterSeup 负责召唤 */ }

    [NpcScript]
    internal sealed class Npc : NpcScriptBase { /* 入口，直接引用 Event 的常量 */ }
}
```

要点：

- 脚本类型由**嵌套类的基类**推导，歧义类型（MapEnter vs MapFirstEnter、四种 Reactor 触发）用显式特性标注。
- 发现的产物仍是 `Dictionary<string, (Type ObjType, MethodInfo Method)>`，`PluginManager.InvokeScriptAsync` 的优先级/热重载语义**不变**。
- 功能内部靠**编译期引用**（常量、嵌套类型、私有方法）连接，而非字符串。

---

## 4. 待办清单

### 阶段一：基础设施 ✅

- [x] `ScriptHandlerAttribute`、`ScriptKind`、`ScriptHandlerDescriptor`、`ScriptFeatureDescriptor`
- [x] `ScriptFeatureDiscovery.ScanByNamespace` / `BuildRegistries` / `CreateEventTemplates`
- [x] 重复键检测：报错同时指出种类、插件名与**双方完整方法名**
- [x] `IScriptEventService` + `PluginManager.ResolveEventTemplates` / `SyncEventTemplatesAsync`
- [x] `EventScriptManager.SyncEventTemplates`（新增/替换/移除模板）
- [x] `EventService` 改为 `IScriptEventService`，移除自带的事件注册循环
- [x] 卸载顺序调整：摘注册 → 同步事件 → 卸载 ALC

### 阶段二：巴洛古简单模式 ✅

- [x] 新增 `Features/BalrogEasy/BalrogEasyFeature.cs`：常量、`Event`、`MapFirstEnterHandler`、`ReactorHitHandler` 同文件
- [x] 召唤逻辑移入 `Event.AfterSeup`（与 `Exped_Balrog` 对齐）
- [x] 删除 `Events/Expedition/Exped_Balrog_Easy.cs`
- [x] 删除 `MapFirstEnterScript.easy_balog_summon`（跨类 `GetTemplate<T>()` 反查消除）
- [x] 删除 `ReactorHitScript` / `ReactorActScript` 中的 `Easy_balogReactor` 空桩（改到功能内并有真实行为）
- [x] 确认 `MapFirstEnterScript` 对「无持有者」安全：`InvokeScriptAsync` 在 `owners.Length == 0` 时返回 false，不抛异常

> 说明：地图钩子 `onFirstUserEnter=easy_balog_summon` 保留给功能内处理器，因此它不再是孤立名字；
> 该处理器用 `eim.EventName` 判断而非强转模板类型，地图在非该事件下进入时静默返回。

### 阶段三：大文件迁移 ✅（主要部分）

已完成的功能迁移（每个功能一个文件，事件 + 各脚本类型同处）：

| 功能 | 文件 | 迁入的脚本类型 | 消除的耦合 |
|---|---|---|---|
| BalrogEasy | `Features/BalrogEasy/` | Event、MapFirstEnter、ReactorHit | `MapFirstEnterScript` 中 `GetTemplate<Exped_Balrog_Easy>()` 强转；重复的 mobId/坐标 |
| PepeKing | `Features/MushroomCastle/`（原 `Features/PepeKing/`，后续并入） | Event、MapFirstEnter、MapEnter、Portal | `summon_pepeking` 与 `pepeking_effect` 靠 `eim["BossId"]` 跨文件通信 |
| DavyJohn | `Features/DavyJohn/` | Event、Portal(7)、Npc(3)、ReactorAct(9) | 四个文件通过 `eim` 属性键耦合 |
| KerningPQ | `Features/KerningPQ/` | Event、Portal(5)、Npc(3) | 事件、三个 NPC、五段传送门分散三处 |
| LudiPQ | `Features/LudiPQ/` | Event、Portal(8)、Npc(3) | 同 Kerning；暴露 `ClearLudiPQStage` 给 `PQ_MoonRabbit` |
| CWKPQ | `Features/CWKPQ/` | Event、Portal(12) | 事件与 12 个阶段/职业门集中 |
| GuildQuest | `Features/GuildQuest/` | Event、Portal(6) | 楼层传送门与集合计时口同处 |
| Horntail | `Features/Horntail/` | Portal(4) | `Exped_Horntail` 与 `PQ_Horntail` 共用封印门；补上缺失的 `hontale_BtoB1` |
| MagatiaPQ | `Features/MagatiaPQ/` | Portal(30) | Alcadno(`jnr*`) 与 Zenumist(`rnj*`) 共用一份抽象基类实现 |
| OrbisPQ | `Features/OrbisPQ/` | Event、Portal(7)、Npc(3) | 通天塔八段传送门 + 女神内侍官 |
| EllinPQ | `Features/EllinPQ/` | Event、Portal(2)、Npc(4) | 毒雾森林 |
| TreasurePQ | `Features/TreasurePQ/` | Event、Portal(1) | MV 的巢穴隧道门 |
| BossRushPQ | `Features/BossRushPQ/` | Event、Portal(1) | 休息室出口 |
| Puppeteer | `Features/Puppeteer/` | Event、Portal(1) | 傀儡师出口 |
| Job4Aerial | `Features/Job4Aerial/` | Event、Portal(3) | 四转飞行教学的三个出口 |
| Zakum | `Features/Zakum/` | Event(PQ+远征)、Portal(1) | 试炼与远征共用出口门 |

> 补记：`ScriptRegistrationCheck` 新增 Portal 种类校验后，发现 `hontale_BtoB1`
> 在 WZ 中被 240050100 引用、但 C# 侧从未实现，已按原 JS 补上。

### 阶段三补充：NPC 脚本归位

上一轮只迁了事件与传送门，NPC 仍留在 `NpcScript.cs` 里，只是把 `nameof(Xxx)` 换成了
`XxxFeature.EventName` —— 这等于换了个引用方式，功能代码依旧分散。本轮补齐：

| 功能 | 迁入的 NPC |
|---|---|
| Zakum | 2030008 / 2030010 / 2030011 / 2030013 / 2032002 / 2032003（6 个） |
| CWKPQ | glpqstatue0~4、glpqStory、glpqStart、glpqEnter、glpqStory2（9 个） |
| GuildQuest | guildquest1_enter/clear/comment/NPC1/out/baseball/will/statue/bonus/board/knight（11 个） |
| BossRushPQ | Raid_solo、Raid_party（2 个） |
| TreasurePQ | guyfawkes_ch（1 个） |
| Job4Aerial | s4strike（1 个） |
| MushroomCastle | TD_MC_bossEnter、TD_MC_violetaEnter（2 个）+ q2333s/e 任务脚本 |
| Puppeteer | DollMaster、Dollcave（2 个）+ q21733s/e + dangerInfo 地图脚本 |

`NpcScript.cs` 中保留的混合脚本（逻辑跨多个功能，暂不迁移）：

| NPC | 原因 |
|---|---|
| `MomCow` 辅助 + `mom_cow` | 牛奶任务线（任务 2180），与上述功能无关 |
| `secretNPC`（1063011） | 玛加提亚密码门（任务 3360），原文件里挂在同一 NPC 下但属另一条线 |

> `DollMaster` 与 `TD_MC_violetaEnter` 已随蘑菇城堡、傀儡师两条剧情线迁走。

### 阶段三结论

**所有事件耦合的脚本已迁移完毕**，核对结果：

| 文件 | 剩余事件耦合方法 |
|---|---|
| `PortalScript.cs` | 0 |
| `MapEnterScript.cs` | 0 |
| `MapFirstEnterScript.cs` | 0 |
| `NpcScript.cs` | 0（仅 11 处 `getEventInstance()` 常规用法） |

WZ 中声明的事件耦合传送门已 **100% 注册**（Portal 基线为空）。

### 阶段五：事件模板功能化

无参构造的 27 个事件模板已迁入各自功能文件（`Features/<Name>/`），引用一并改为
`XxxFeature.EventName`。`Features/` 目录现共 **44 个功能**。

仍在 `Events/` 集中注册的，全部是**带参构造**的模板（按决定保留现状）：

| 文件 | 模板 | 构造参数 |
|---|---|---|
| `SJob.cs` | `SoloQuestEventTemplate` | `(quest, eventTime, entryMap, exitMap, minMap, maxMap)` |
| `HolidayPQ.cs` | `HolidayPQ` | `(difficulty)` |
| `PrivateContiMove.cs` | `PrivateContiMove` | `(name, stations, transportings, rideTime)` |
| `PQ_Ariant.cs` | `PQ_Ariant` | `(name, recruitMap)` |
| `PQ_CPQ1.cs` / `PQ_CPQ2.cs` | `PQ_CPQ1/2` | `(name, recruitMap)` |
| `TD_Battle.cs` | `TDBattle` 子类 ×5 | 地图/时长/Boss |
| `CafePQ.cs` | `CafePQ` 子类 ×6 | 阶段参数 |
| `Exped_Balrog.cs` | `Exped_Balrog` / `PQ_Balrog` | 无参但同级两模板 |
| `Exped_Horntail.cs` | `Exped_Horntail` / `PQ_Horntail` | 无参但同级两模板 |
| `Job_Pirate.cs` | `q6330` / `q6370` | 无参但同级两模板 |

> 最后三行虽是无参构造，但一个文件含两个模板；拆成一功能一文件需要重新决定归属，
> 留待后续按需处理。

> 后续已全部处理：`Exped_Balrog` / `Exped_Horntail` / `Job_Pirate` 的两模板均已拆入各自功能目录，
> `Events/` 下不再有集中注册的事件模板。

### 四转技能任务按职业合并

`s4*` 系列传送门与任务事件按职业归入四个功能文件：

| 文件 | 任务事件 | 传送门 |
|---|---|---|
| `Warrior4thJob` | q6153（Sayram's Skill） | `s4berserk`、`s4berserk_move`、`s4rush`（q6110 Manji's Test，无独立事件） |
| `Archer4thJob` | q6240（Flame Bird）、q6241（Phoenix）、q6242（Frostprey） | `s4firehawk`、`s4nest`、`s4iceeagle` |
| `Magician4thJob` | q6132（Cold Flame）、q6230（Maple marble） | `s4resurrection`、`s4tornado_enter`、`s4resur_enter/out`（q6134，无事件）、`s4common1_exit` |
| `Thief4thJob` | q6201（Shawn's fear） | `s4hitman` |

> 职业归属依据 WZ 任务标题：Flame Bird/Phoenix/Frostprey 是弓箭手召唤兽、
> Cold Flame/Resurrection/Maple marble 属法师线、Sayram's Skill/Manji's Test 属战士、
> Shawn's fear 属飞侠。`q6110`/`q6134` 无事件模板（纯传送门判断），门归入对应职业文件。

同时：`Job4Aerial` 事件改名为 **q6400**（Jonathan 的考验，海盗四转），并入
`Features/Pirate4thJobFeature/`，与 q6330/q6370（耐力试炼）、q6410（心灵训练入口）同文件。

### 任务事件与关联脚本

`SoloQuestEventTemplate(...)` 的 33 个匿名实例已改为 `Features/Quest/` 下的无参具名类
（`Q<questId>`）。同时把与该任务关联的脚本一并归入同一文件：

| 类别 | 判定依据 | 已归入 |
|---|---|---|
| 任务脚本 | 方法名 `q{id}s` / `q{id}e` | 14 个（Q2191/2192/2245/2291/3239/3933/21301/21401/21613/21739/21746/21747） |
| 入口 NPC | 方法体内调用 `TryStartSoloQuest({id})` | 5 个（Q2175/3239/3335/6108/21747） |

`Features/Quest/` 现共 **39 个文件**：33 个任务事件 + 6 个早前迁移的 q 事件
（Q21401/21610/21613/21747/2245/6002，已从各自目录移入并改名）。

> `SoloQuestEventTemplate` 新增受保护的 `SoloQuestEventTemplate(string name)` 构造，
> 供具名子类使用；原 6 参构造保留给 `EventService` 中尚未迁移的实例。

### Events 目录清空

`Events/` 下剩余的全部参数化事件已迁入 `Features/`：

| 功能文件 | 内容 |
|---|---|
| `PrivateContiMove/` | 5 条航线**各自独立 Feature**（KerningTrain/ShipOribs/ShipEllin/Whale/Crane），基类 `ContiMoveEventBase`；每条航线携带自己的入口 NPC 与传送门 |
| `MonsterCarnival/`（PQ_CPQ1/PQ_CPQ2 两个功能） | 嘉年华 1/2，各 3 个房间（Room1~3） |
| `Exped_Balrog/` | `PQ_Balrog` + `Exped_Balrog` + 3 个入口 NPC（balog_accept 等） |
| `Exped_Horntail/` | `PQ_Horntail` + `Exped_Horntail`（**已在后续合并进 `Horntail/`**，见下文「远征相关文件按事件归并」） |
| `PQ_Ariant/` | `RoomBase` + Room1~3 |
| `TDBattle/`、`CafePQ/` | 抽象基类 + 子类整体迁入 |
| `SJob.cs` | 保留（`SoloQuestEventTemplate` 基类，供所有具名任务事件继承） |

同时把其他脚本里引用这些事件的方法迁入对应功能：NPC（s4mind/s4efreet/Depart_topFloorIn/q8535s/balog_accept 等）、传送门（TD_neo_inTree/captinsg00/dragonNest/Populatus00 等）。

### 发现器修正

- **抽象类不再实例化**：`ScriptFeatureDiscovery` 之前只跳过 `abstract sealed`，纯粹的抽象基类（如 CafePQ/TDBattle 的基类）会被当作模板 `Activator.CreateInstance` 而抛异常。
- ~~**继承的处理器方法**：处理器若为「抽象基类持有方法 + 具体子类提供配置」，沿基类链收集方法。~~
  该能力已移除：`MagatiaPQ` 后来改成单个 `sealed class`（内部按事件名切换地图基数），
  实测 `Application.Plugin.Script` 中已没有任何类型的方法来自插件自定义基类，
  因此 `CollectHandlers` 现在只扫 `DeclaredOnly`，配套的 `IsPluginOwnedHandler` 与
  8 项框架基类白名单一并删除。

### 事件模板守卫

新增 `EventTemplateCountsDoNotShrink`：记录 44 个事件模板名（`event-template-names.txt`），
任何名称消失即失败。批量迁移最容易出「删了旧文件没写进新文件」的静默丢失，
而事件名不在 WZ 中，解析校验抓不到，只能靠名称快照兜底。

> 迁移中发现 `DollHouse` 的实际注册名是 `q3230`（由 `base(3230, ...)` 派生），
> 并非类型名。已在功能内用注释说明，避免后续误用 `EventName`。

### 扁平类改造：功能目录内不再使用嵌套类

原先「一个功能 = 一个外层类 + 若干嵌套类」，迁移完成后外层类只剩常量，嵌套结构已无收益。
现统一改为**同目录下每类型一个文件**：

```
Features/HolidayPQ/                 namespace Application.Plugin.Script.Features.HolidayPQ
├── HolidayPQFeature.cs          常量（事件名等）
├── HolidayPQ.EventBase.cs       抽象基类（三个难度共用规则）
├── HolidayPQ.Event1.cs          HolidayPQ_1
├── HolidayPQ.Event2.cs          HolidayPQ_2
└── HolidayPQ.Event3.cs          HolidayPQ_3
```

约定：

| 项 | 说明 |
|---|---|
| 发现方式 | `ScriptFeatureDiscovery.ScanByNamespace`：同一 namespace 下的所有类型归为一个功能 |
| 功能名 | 取 namespace 末段，因此**目录名必须与 namespace 末段一致** |
| 类名 | 同一 namespace 会共享命名空间，通用名（`PortalHandler`/`Event`/`QuestHandler`）会撞名，故按文件前缀命名：`Q21747Event`、`MonsterCarnival.RoomTemplate.cs` 里的 `MonsterCarnivalRoomTemplate` |
| 跨文件引用 | 原嵌套作用域里的外层常量改为 `XxxFeature.常量名` |
| 抽象基类 | 单独成文件，扫描时被 `IsAbstract` 跳过 |

> 早期版本用 `[ScriptFeature("X")]` 标注功能类来取名。扁平化后该特性能提供的信息已由
> namespace 完全覆盖，且 `Features/Quest`（36 个）、`Zakum`（2 个）、`PrivateContiMove`（5 个）
> 一个 namespace 内有多个标注，最终取哪个取决于无保证的反射顺序——实测会得出
> `Crane`、`ZakumExped` 这类与目录无关的名字。**特性已删除**，功能名统一取 namespace 末段。
> 同时删除了只服务于旧嵌套类写法的 `ScriptFeatureDiscovery.Scan`（无调用点）。

### 事件名引用统一为常量

扁平化后「事件名 = 类型名」的隐含关系不再成立，`nameof(旧类名)`、`"PQ_Xxx" + n` 这类写法
要么指向已不存在的类，要么靠字符串拼接与注册处各自维护。现统一为**只在功能类上声明一次**：

| 位置 | 形式 |
|---|---|
| 单事件功能 | `XxxFeature.EventName` |
| 同功能多事件 | `XxxFeature.ExpeditionEventName` / `PartyQuestEventName`、`AlcadnoEventName` / `ZenumistEventName` |
| 序号派生（CafePQ / TDBattle / PQ_Ariant / MonsterCarnival / HolidayPQ） | 功能类上的 `EventNames[]` 数组或 `EventNameOf(index)` |

改动清单：

| 文件 | 原写法 | 现写法 |
|---|---|---|
| `AmoriaPQ` / `LudiMazePQ` / `PQ_WuGong` 的 NpcHandler | `GetEventManager(nameof(AmoriaPQ))` 等（类名已随扁平化消失） | `XxxFeature.EventName` |
| `NpcScript.cs`（假日入口） | `"HolidayPQ_" + pqType` | `HolidayPQFeature.EventNameOf(pqType)` |
| `NpcScript.cs`（咖啡厅入口） | `$"CafePQ_{pqArea}"` | `CafePQFeature.EventNames[pqArea - 1]` |
| `PortalScript.cs`（时空裂缝入口） | `"TD_Battle" + stage` | `TDBattleFeature.EventNames[stage - 1]` |
| `PQ_Ariant` / `MonsterCarnival` 的 NpcHandler | `"PQ_Ariant" + (x+1)`、`"PQ_CPQ1" + (x+1)` | `PQ_AriantFeature.EventNames[x]`、`MonsterCarnivalFeature.Cpq1EventNames[x]` / `Cpq2EventNames[x]` |
| `MagatiaPQ.NpcHandler` | 字面量 `"MagatiaPQ_A"` / `"MagatiaPQ_Z"` | `MagatiaPQFeature.AlcadnoEventName` / `ZenumistEventName` |
| `Pirate4thJobFeature` | `Super` / `Ship` / `Strike` / `Mind`（实为事件键） | `SuperEventName` / `ShipEventName` / `StrikeEventName` / `MindEventName` |
| `PrivateContiMove/*` | `Crane.EventName`（模板类名） | `CraneFeature.EventName`（事件名收敛到功能类） |
| `CafePQ_1~6`、`TD_Battle1~5` | `base(nameof(...))` | 各自声明 `EventName` 常量并由 `base(EventName)` 使用 |

配套新增的常量数组：

| 功能类 | 成员 |
|---|---|
| `CafePQFeature` | `EventNames[6]` |
| `TDBattleFeature` | `EventNames[5]` |
| `PQ_AriantFeature` | `EventNames[3]` |
| `MonsterCarnivalFeature` | `Cpq1EventNames[3]`、`Cpq2EventNames[3]` |
| `HolidayPQFeature` | `EventNameOf(difficulty)` |
| `Exped_BalrogFeature` / `Exped_Horntail`→`HorntailFeature` | `EventName` / `ExpeditionEventName` |

### 远征相关文件按事件归并

`Npc/Expedition/` 目录下的四个文件全部并入对应功能目录，该目录已删除：

| 原文件 | 归入 | 内容 |
|---|---|---|
| `Exped_Horntail.cs` | `Features/Horntail/Horntail.NpcHandler.cs` | `hontale_keroben` / `hontale_enterToE` / `hontale_enter1` / `hontale_out` / `hontale_accept` |
| `Exped_PinkBean.cs` | `Features/Exped_PinkBean/Exped_PinkBean.NpcHandler.cs` | 2141000~2141002 |
| `Exped_Scarga.cs` | `Features/Exped_Scarga/Exped_Scarga.NpcHandler.cs` | 9270047 / 9201134 |
| `Exped_Showa.cs` | `Features/Exped_Showa/Exped_Showa.NpcHandler.cs` | 9120201 |

`Exped_Horntail` 功能与原有的 `Horntail` 功能**合并为一个**——两者服务于同一张地图组
（240050xxx 试炼场、240060xxx 巢穴），此前拆成两个目录只导致传送门和事件模板分家：

```
Features/Horntail/
├── HorntailFeature.cs           ExpeditionEventName / PartyQuestEventName
├── Horntail.Expedition.cs       Expedition（原 Exped_Horntail 模板）
├── Horntail.PartyQuest.cs       PartyQuest（原 PQ_Horntail 模板）
├── Horntail.NpcHandler.cs       五个入口/出口 NPC
└── Horntail.PortalHandler.cs    hontale_BtoB1 / Bopen / BR / C / out1 / morph2
```

同时把 `PortalScript.cs` 中残留的 `hontale_morph2`、`hontale_out1` 两个传送门迁入。
该处原先还留有一句「horntail 传送门已在 Features/Horntail 中实现」的注释，说明搬迁时漏掉了这两个。

### 功能外的残留事件脚本全部归位

以「`GetEventManager` 在 `Features/` 之外还有哪些调用点」为线索，把仍留在旧式部分类里的
事件相关脚本全部归入对应功能。此后 `NpcScript.cs` / `PortalScript.cs` / `Reactor*Script.cs`
中**不再出现任何事件查表或事件名引用**：

| 原位置 | 脚本 | 归入 |
|---|---|---|
| `NpcScript.cs` | `go_pcmap`（NPC 1052013，高级道路报名 / 收取优惠券 / 发奖） | `Features/CafePQ/CafePQ.NpcHandler.cs` |
| `NpcScript.cs` | `s_08_xmas`（NPC 9105004，脚本名 `08_xmas`，按地图号算难度） | `Features/HolidayPQ/HolidayPQ.NpcHandler.cs` |
| `NpcScript.cs` | `out_08Xmas`（NPC 9105005，同属假日 PQ 地图组） | 同上 |
| `PortalScript.cs` | `TD_Boss_enter`（按地图号算关卡开 `TD_BattleN`） | `Features/TDBattle/TDBattle.PortalHandler.cs` |

`go_pcmap` 与 `s_08_xmas` 原先分别靠 `CafePQFeature.EventNames[...]`、
`HolidayPQFeature.EventNameOf(...)` 反向引用功能类，合并后引用留在同一 namespace 内。
`GiveXmasPresent` 是 `s_08_xmas` 的私有辅助方法，随其一起迁出。

> 迁移脚本时注意锚点：`TD_Boss_enter` 首次删除因锚点比实际多一行空行而静默失败，
> 直到注册表守卫报「脚本名冲突」才暴露。批量搬迁后务必跑 `ScriptRegistrationCheck`。

### 蛋塔王（PepeKing）并入蘑菇城堡

`Features/PepeKing/` 与 `Features/MushroomCastle/` 服务同一片区域（106021400~106021500、
Boss 3300005~3300008），入口 NPC 本来就同时开两个副本，此前拆成两个目录导致
`MushroomCastleFeature.PepeKingEventName` 反向引用另一功能。现合并为一个目录：

```
Features/MushroomCastle/
├── MushroomCastleFeature.cs                    EventName / Event2Name / PepeKingEventName + 常量
├── MushroomCastle.Event.cs / .Event2.cs        总理官邸两阶段
├── MushroomCastle.PepeKingEvent.cs             蛋塔王的诅咒（原 PepeKing.Event）
├── MushroomCastle.NpcHandler.cs                官邸入口 NPC
├── MushroomCastle.QuestHandler.cs              q2333
├── MushroomCastle.PepeKingMapFirstEnterHandler.cs   summon_pepeking（随机子 Boss）
├── MushroomCastle.PepeKingMapEnterHandler.cs        pepeking_effect（按 BossId 播特效）
└── MushroomCastle.PepeKingPortalHandler.cs          out_pepeking（清场 + 补发任务道具）
```

常量统一加 `PepeKing` 前缀挂在 `MushroomCastleFeature` 上
（`PepeKingEventName` / `PepeKingFirstSubBossId` / `PepeKingQuestRewardItem` /
`PepeKingExitMapId` / `PepeKingBossIdProperty`），事件类改名 `PepeKingEvent`，
三个处理器各带类名前缀以避免与目录内其它处理器撞名。

### 阶段五补充：清理 EventService 时丢失的事件模板

`EventService._events` 清空后（模板改由各功能上报），有以下模板**只在该清单里出现过**、
功能目录中没有对应实现，导致静默消失。已全部补齐：

| 模板 | 落位 |
|---|---|
| `HolidayPQ_1/2/3` | `Features/HolidayPQ/`：`HolidayPQEventBase` + `Event1/2/3` |
| `PQ_CPQ11/12/13`、`PQ_CPQ21/22/23` | `Features/MonsterCarnival/`：`MonsterCarnivalRoomTemplate` + `Cpq1Room1~3` / `Cpq2Room1~3` |
| `MagatiaPQ_A` / `MagatiaPQ_Z` | `Features/MagatiaPQ/`：`AlcadnoEvent` / `ZenumistEvent`（含 `OnMobKilled` 分阶段逻辑） |

`PQ_Balrog` 与 `DollHouse` **有意不复原**：

- `PQ_Balrog`（910520000 单人副本，击杀 9300326 后生成 NPC 1061015）与 `q2245` **规则完全相同**，
  且 `q2245` 已经承载了同一入口（NPC 侧走 `TryStartSoloQuest(2245)`）。
  两者并存会让同一张地图有两份等价模板，故只保留 `q2245`。
- `DollHouse` 的注册名就是 `q3230`（`base(3230, ...)` 派生），功能已落在 `Features/Quest/Q3230`。

`EventTemplateCountsDoNotShrink` 快照随之从 43 项扩到 **109 项**（补齐后重新生成，
覆盖此前未纳入快照的带参模板）。

### 脚本注册职责重划：插件声明，Manager 持有

原先 `ScriptService` 既做扫描又持有注册表（`NpcScripts` / `QuestScripts` … 十来个属性），
`PluginManager` 再反查这些属性做优先级裁决。职责混在一起，导致插件侧残留大量扫描代码，
且每个插件都要自己决定怎么合并、怎么处理重名。

现在按「插件声明、Manager 持有」重划：

**接口层**——属性改为方法，插件返回「我声明了哪些脚本」：

```csharp
[PluginInvocation(PluginInvocationMode.FirstMatch)]
public interface IScriptNpcService : IPluginServiceBase
{
    IReadOnlyCollection<ScriptHandlerDescriptor> LoadNpcScripts();
}
```

`ScriptHandlerDescriptor` 取代原来的 `(Type ObjType, MethodInfo Method)` 元组：
它多带 `Kind` 与 `ScriptName`，既是返回类型也是诊断信息。

**插件层**——`ScriptService` 不持有任何注册表，每个 `Load*` 内部做扫描：

```csharp
public IReadOnlyCollection<ScriptHandlerDescriptor> LoadNpcScripts()
    => Merge(ScriptKind.Npc, typeof(NpcScript), typeof(NpcScriptMissing));
```

`Merge` 合并三类来源：旧式 partial 类、`Features/` 功能目录、占位类。
占位类最后合并，与真实实现重名时让位——它只负责把「查不到」变成「未实现」的可观测日志。

**Manager 层**——`PluginManager` 持有合并结果：

```csharp
void RebuildScripts()   // 插件加载/卸载时调用
public IReadOnlyDictionary<string, ScriptHandlerDescriptor> GetScripts(ScriptKind kind)
```

归属信息直接写在描述符上，不再另建索引：

```csharp
public sealed class ScriptHandlerDescriptor
{
    public ScriptKind Kind { get; }
    public string ScriptName { get; }
    public Type HandlerType { get; }
    public MethodInfo Method { get; }

    /// 提供该脚本的插件容器；插件刚返回时为 null，合并时由 WithSource 补上
    public PluginContainer<PluginServiceBase>? Source { get; }

    public ScriptHandlerDescriptor WithSource(PluginContainer<PluginServiceBase> source);
}
```

这样 `_scripts` 只需一层 `Dictionary<ScriptKind, Dictionary<string, ScriptHandlerDescriptor>>`，
不必再维护平行的 `(Container, Handler)` 元组或 `_scriptOwnersByPlugin` 索引；
执行时 `script.Source.Tracker.EnterRequest()` 就能拿到正确的租约，
告警日志里的「被谁覆盖」也直接读 `existing.Source?.LoadContext.PluginName`。

`RebuildScripts` 按下标优先级顺序（数字小者优先，同优先级按插件名）合并，
被压制者用**自己插件的 logger** 输出一次告警，说明被哪个插件覆盖：

```
脚本 Npc:party_ludimaze_enter 已由更高优先级的插件 Script 提供，本插件的同名实现未生效
```

### 事件模板：重名直接抛异常

脚本允许跨插件覆盖（这正是优先级的意义），**事件模板不允许**——事件名是客户端与脚本约定好的全局键，
静默覆盖会让另一个功能悄悄失效，而从日志里极难发现。因此 `ResolveEventTemplates` 遇到重名直接抛
`BusinessFatalException`，指明两个插件。

### 热更新语义

`_scripts` 是整体替换的只读快照，执行时先取快照、再进入 `RequestTracker` 租约：

- **正在执行的脚本**：持有旧快照里的 `ScriptHandlerDescriptor` 与租约，卸载会等它执行完
- **新请求**：读到的已是重建后的新表

因此热更新不会把跑到一半的脚本换成新版本。NPC/Quest 对话的租约只包住 `Method.Invoke` 那一次调用，
不覆盖整个对话会话——否则一次长对话会一直阻塞插件卸载。

### 随之删除的复杂度

| 删除 | 原因 |
|---|---|
| `ScriptFeatureDescriptor` | 只服务于「按命名空间聚合功能名」，新设计不再需要功能概念 |
| `IScriptCatalog` | `Resolve` 的用途（回溯功能）随 `ScriptFeatureDescriptor` 一起消失 |
| `ScriptCatalogEntry` / `DumpScriptCatalog` / `ResolveScriptOwner` | 诊断由 `GetScripts` / `GetScriptOwner` 直接提供 |
| `ScriptFeatureDiscovery.BuildRegistries` | 合并逻辑移入 `PluginManager.RebuildScripts` |
| `ScriptService.BuildRegistries` 静态入口 | 插件不再产出注册表 |
| `PluginManager.InvokeScriptAsync` / `GetScriptOwners` | 脚本查找改为直接读合并表 |

`ScriptFeatureDiscovery` 现在只回答「这些类型提供了哪些脚本」：

```csharp
public static IReadOnlyCollection<ScriptHandlerDescriptor> CollectHandlers(Type handlerType)
public static IReadOnlyCollection<ScriptHandlerDescriptor> CollectHandlers(Type handlerType, ScriptKind kind)
public static IReadOnlyCollection<AbstractEventTemplate> CreateEventTemplates(IEnumerable<Type> types)
public static bool TryResolveKind(Type type, out ScriptKind kind)
```

> `CollectHandlers` 有两个重载：无 `kind` 的走类型推断（功能目录用），带 `kind` 的由调用方指定。
> 旧式 partial 类与占位类必须用后者——`NpcScript` 没有脚本上下文基类可推断，
> `MapEnterScript` 与 `MapFirstEnterScript` 又同为 `MapScriptMethods`，推断会把两者混成一种。

重构后逐个种类的脚本数与快照完全一致（合计 2967 条）。

### 阶段四：诊断与校验

- [x] ~~`PluginManager.DumpScriptCatalog` / `ResolveScriptOwner`~~（后由 `GetScripts` / `GetScriptOwner` 取代）
- [x] ~~GM 命令 `!scripts [种类] [脚本名]`~~
- [x] `ScriptFeatureDiscoveryTests`（事件唯一性 / 模板数量守卫 / 处理器可构造性）
- [x] WZ 校验：`ScriptRegistrationCheck` 比对 Map `onUserEnter`/`onFirstUserEnter`、Portal `script`、NPC `script`、Item `spec/script`
- [x] 基线快照 `test/ServiceTest/Infrastructure/WZ/script-registration-baseline.txt`：
      已知缺口固化（当前仅 645 条 NPC），新增缺失或已修复都会失败

> 基线说明：645 条 NPC 缺口经核对**均无对应 JS**，属 WZ 声明但两侧从未实现的脚本，
> 不是本次迁移造成的遗漏。如需清理，应单独评估。

### 阶段四附带修复：占位类从未注册

WZ 校验发现 `NpcScriptMissing` / `QuestScriptMissing` / `ItemScriptMissing` / `PortalScriptMissing` /
`ReactorActScriptMissing` 五类占位类**根本没有被注册**（`ScriptService` 只扫了主类）。
旧文档写的「用 `TryAdd` 合并」在代码里并不存在，导致 WZ 声明的缺失脚本一律静默查表失败。

现已在 `ScriptService.BuildRegistries` 中按「真实实现优先、占位补位」的规则合并，缺失重新变得可观测。

---

## 4.1 已落地文件

| 文件 | 作用 |
|---|---|
| `src/Application.Core/Gameplay/Plugins/ScriptKind.cs` | 脚本种类枚举 |
| `.../ScriptHandlerAttribute.cs` | 歧义种类显式标注 |
| `.../ScriptHandlerDescriptor.cs` / `ScriptFeatureDescriptor.cs` | 扫描结果模型 |
| `.../ScriptFeatureDiscovery.cs` | 扫描、分类、合并、重复检测 |
| `.../IScriptCatalog.cs` | 脚本名 → 功能 回溯 |
| `.../ScriptCatalogEntry.cs` | 诊断条目 |
| `.../Services/IScriptEventService.cs` | 事件模板上报接口 |
| `src/Application.Core/Game/Commands/Gm5/ScriptsCommand.cs` | `!scripts` |
| `src/Application.Plugin.Script/Features/*/` | 40 个功能目录（扁平类，每类型一个文件） |
| `test/ServiceTest/Games/Gameplay/ScriptFeatureDiscoveryTests.cs` | 发现器/重复键/事件唯一性/模板数量守卫 |
| `test/ServiceTest/Games/Gameplay/event-template-names.txt` | 事件模板名快照（109 项） |
| `test/ServiceTest/Infrastructure/WZ/ScriptRegistrationCheck.cs` | WZ 声明 → 注册表 一致性 |
| `test/ServiceTest/Infrastructure/WZ/script-registration-baseline.txt` | 已知缺口基线 |
| `test/ServiceTest/Infrastructure/WZ/script-registration-counts.txt` | 各脚本种类注册数快照 |

> `ScriptFeatureDiscoveryTests` 放在 `Games/` 而非 `Infrastructure/`：
> 它要实例化事件模板，而模板构造函数会触碰 `MapFactory` 等静态单例，
> 只有 `Games/` 下的 `GameTestGlobal` 会启动本地服务器完成依赖注册。详见 `AGENTS.md`。

删除的旧文件：`Events/Expedition/Exped_Balrog_Easy.cs`、`Events/MK_PepeKing.cs`、
`Events/PartyQuest/PQ_Pirate.cs`、`Events/PartyQuest/PQ_Kerning.cs`、`Events/PartyQuest/PQ_Ludi.cs`、
`Events/Expedition/Exped_CWKPQ.cs`、`Events/GuildQuest.cs`、`Npc/PartyQuest/PQ_Pirate.cs`、
`Npc/PartyQuest/PQ_Kerning.cs`、`Npc/PartyQuest/PQ_Ludi.cs`。

扁平化阶段删除的目录：`Npc/Expedition/`（4 个文件全部并入 `Features/`）、
`Features/Exped_Horntail/`（并入 `Features/Horntail/`）、`Features/PQ_CPQ1/` 与 `Features/PQ_CPQ2/`
（并入 `Features/MonsterCarnival/`）、`Events/` 下全部集中注册的模板文件。

修改：`PluginManager.cs`（事件合并/同步/卸载顺序/诊断）、`EventScriptManager.cs`（`SyncEventTemplates`）、
`ScriptService.cs`（旧式 + 功能式合并 + 占位类注册）、`EventService.cs`（上报模板）、
`PortalScript.cs` / `MapEnterScript.cs` / `MapFirstEnterScript.cs` / `Reactor*Script.cs`（删除已迁移实现）、
`Application.Plugin.Script.csproj`（`InternalsVisibleTo`）。

---

## 5. 风险记录

| 风险 | 说明 |
|---|---|
| 嵌套类反射 | `TypeUtils.LoadFromType` 只处理顶层类；嵌套类需按注册类型（`NestedType`）扫描。`ScanByNamespace` 已同时收集嵌套类型，旧写法可用 |
| 事件模板单实例跨频道共享 | 现有 `EventService` 就是共享实例；插件热重载时若实例被替换，正在运行的 `AbstractEventManager.Template` 引用需同步 |
| 插件卸载竞态 | `PluginContainer.RequestTracker` 负责排空在途请求；事件模板卸载必须等实例结束 |
| 部分类跨文件 | `NpcScript` 有 49 个 partial 文件；迁移期间新旧注册方式并存，需避免同名键冲突 |
| 模块工程失配 | `src/modules/*` 中 `MarriageEventManager : AbstractEventManager<T>` 等已在基线编译失败，不在本次修复范围 |
| 扁平类同名冲突 | 功能目录内所有类共享同一 namespace，通用名会撞名；新增类型须带功能前缀（如 `Q21747Event`） |

---

## 6. 变更日志

| 日期 | 内容 |
|---|---|
| 2026-10-23 | 建立文档；完成现状调研；确认三项设计决策 |
| 2026-10-23 | 基础设施完成；巴洛古简单模式迁移完成；新增 `!scripts`；Core / Plugin.Script / Host 编译通过；发现器测试 4/4 通过 |
| 2026-10-23 | 追加 PepeKing、DavyJohn 两个功能迁移（后者跨事件/传送门/NPC/反应堆四类）；脚本测试 47/47 通过；Host 编译 0 错误 |
| 2026-10-23 | 补 WZ 校验测试与基线；修复占位类从未注册的缺陷；追加 KerningPQ、LudiPQ、CWKPQ、GuildQuest、Horntail、MagatiaPQ 六个功能迁移；补上缺失的 `hontale_BtoB1`；Portal 纳入 WZ 校验 |
| 2026-10-23 | **会话暂停**：主体已提交为 `505812006`，工作区仅剩 Horntail 的 `hontale_BtoB1` 与 Portal 校验两项改动待提交 |
| 2026-10-26 | 四转技能任务按职业合并：新增 Warrior/Archer/Magician/Thief4thJob 四个功能文件；`Job4Aerial` 改名 q6400 并入 `Pirate4thJob`；`s4rush` 归战士 |
| 2026-10-26 | Q 任务事件：33 个 `SoloQuestEventTemplate(...)` 改为 `Features/Quest/Q<id>` 具名类；14 个任务脚本（`q{id}s/e`）与 5 个入口 NPC（`TryStartSoloQuest`）并入对应文件；`Features/Quest/` 共 39 个文件 |
| 2026-10-26 | 事件模板功能化：27 个无参事件迁入 `Features/`，共 44 个功能；新增 `EventTemplateCountsDoNotShrink` 守卫（44 个模板名快照）；带参模板按决定保留 |
| 2026-10-26 | 新增 MushroomCastle（MK_PrimeMinister 两阶段事件 + TD_MC NPC + Q2333）与 Puppeteer 全剧情线（事件 + DollMaster/Dollcave + Q21733 + dangerInfo）；`NpcScript.cs` 中已无跨功能的 `nameof(...)` 事件耦合 |
| 2026-10-26 | NPC 归位：Zakum/CWKPQ/GuildQuest/BossRushPQ/TreasurePQ/Job4Aerial 共 30 个 NPC 方法移入各功能文件；新增 `RegistryCountsDoNotShrink` 守卫测试 |
| 2026-10-26 | 恢复任务：迁移 OrbisPQ、EllinPQ、TreasurePQ、BossRushPQ、Puppeteer、Job4Aerial、Zakum 七个功能；事件耦合脚本清零；Portal 纳入 WZ 校验并通过；`DynamicObjectFactory` 缓存改为 `ConditionalWeakTable` 弱键，消除插件 ALC 卸载泄漏；`ServiceTest.Infrastructure` 253 通过 |
| 2026-10-27 | **扁平类改造**：`Features/` 下全部嵌套类展开为同目录独立文件（Quest 目录 97 个文件、PrivateContiMove 17 个、Zakum/MonsterCarnival/HolidayPQ 同步拆分）；发现器改用 `ScanByNamespace`，同时兼容两种写法 |
| 2026-10-27 | **补齐丢失模板**：`HolidayPQ_1/2/3`、`PQ_CPQ1x/2x` 六个房间、`MagatiaPQ_A/Z` 复原；`PQ_Balrog` 判定与 `q2245` 重复、`DollHouse` 即 `q3230`，均不复原（见阶段五补充）；事件快照 43 → 109 |
| 2026-10-27 | 校验：`Application.Host` 0 错误；`ServiceTest.Infrastructure` 249 通过 / 188 跳过；`ServiceTest.Games` 49/50（`Npc1012112Test` 在基线 `b57fa284` 上同样失败，属既有问题：该 NPC 的 JS 脚本查 `HenesysPQ`，C# 侧注册名为 `PQ_Henesys`） |
| 2026-10-27 | **事件名引用统一为常量**：清掉全部 `nameof(旧类名)`、字符串拼接查事件；新增 `CafePQFeature`/`TDBattleFeature`/`PQ_AriantFeature`/`MonsterCarnivalFeature` 的 `EventNames` 数组与 `HolidayPQFeature.EventNameOf`；`Pirate4thJobFeature` 四个事件键改名为 `...EventName`；`PrivateContiMove` 事件名收敛到功能类 |
| 2026-10-27 | **远征文件归并**：`Npc/Expedition/` 四个文件全部迁入对应功能目录并删除该目录；`Exped_Horntail` 功能并入 `Horntail`（事件模板 + NPC + 传送门同目录）；`PortalScript.cs` 中残留的 `hontale_morph2` / `hontale_out1` 一并迁入。顺带删除 `PortalScript.cs` 一处悬空的 `[ScriptTag(["PQ_Magatia"])]`（原宿主 `rnj1_out` 已移走，该特性会误挂到 `secretDoor()` 上） |
| 2026-10-27 | **PepeKing 并入 MushroomCastle**：`Features/PepeKing/` 删除，事件模板/地图脚本/出口传送门迁入 `Features/MushroomCastle/`；常量统一加 `PepeKing` 前缀，事件类改名 `PepeKingEvent`；功能目录数 41 → 40 |
| 2026-10-27 | **功能外事件脚本归位**：`go_pcmap` → `CafePQ`，`s_08_xmas` / `out_08Xmas` → `HolidayPQ`，`TD_Boss_enter` → `TDBattle`；至此 `GetEventManager` 与事件名引用在 `Features/` 之外清零 |
| 2026-10-27 | **删除 `ScriptFeatureAttribute`**：功能名改由 namespace 末段决定，删除只服务旧嵌套类写法且已无调用点的 `ScriptFeatureDiscovery.Scan`；`ScriptFeatureDescriptor` 去掉无消费方的 `FeatureType` 与 `Name` 兜底；`ExpedBalrogFeature` / `PrivateContiMoveFeature` 两个 namespace 归一为与目录同名的 `Exped_Balrog` / `PrivateContiMove` |
| 2026-10-28 | **脚本注册职责重划**：接口属性改 `Load*Scripts()` 方法，`ScriptHandlerDescriptor` 取代元组；`PluginManager` 持有合并表（`GetScripts` / `GetScriptOwner`），插件不再持有注册表；事件模板跨插件重名改为抛异常；执行时进入 `RequestTracker` 租约保证热更新语义；删除 `ScriptFeatureDescriptor` / `IScriptCatalog` / `ScriptCatalogEntry` / `InvokeScriptAsync` 等 |
| 2026-10-28 | **脚本来源内聚到描述符**：`ScriptHandlerDescriptor` 增加 `Source`（`PluginContainer`）与 `WithSource`，合并表退化为单层字典，删除 `_scriptOwnersByPlugin`、`FindScriptEntry` 元组与无人调用的 `GetScriptOwner` |
| 2026-10-28 | **扫描简化**：15 个旧式/占位类补 `[ScriptHandler]`，`*Missing` 改为主类 partial（去掉 `IsFallback`）；`CollectHandlers` 只在程序集级扫描、只取 `DeclaredOnly`；`SelectByKind` 提取到 `ScriptFeatureDiscovery` 供默认脚本插件与 TreasureMap 插件复用；删除 `FeaturesNamespace`、基类链收集与框架基类白名单 |
| 2026-10-30 | **四转任务各自成目录**：`Features/Quest/` 下的 `Archer4thJob`、`Magician4thJob`、`Pirate4thJobFeature` 三组文件与 `Warrior4thJob` 一样移入同名目录，namespace 同步改为 `Features.<Name>`；`Features/Quest/` 只保留 `Q<id>` 与 `FirstDH`、`PuppeteerFeature` |
| 2026-10-30 | **修复两处遗留问题**：① `Features.Aran` 内的 `Aran.POLEARM_BOOSTER` 等被外层 namespace 遮蔽（`Aran` 先绑定到 `Features.Aran` 而非技能常量类），改用 `using SkillAran = Application.Shared.Constants.Skill.Aran`；② 移除 csproj 里 `Features\Thief4th\**` 的 `<Compile Remove>`（该目录此前根本没参与编译，`s4hitman`/`s4tornado_enter` 两个 Portal 与 q6201/q6230 两个事件全部丢失），同时把快照中已不存在的 12 条 `ReactorHit|s4hitman*` 删除——这些真实实现只存在于 `Thief4th.ReactorActHandler`，旧 `ReactorHitScript` 里是空 TODO 桩，已随重构删除 |
| 2026-10-30 | **反应堆脚本按功能拆分后的快照对齐**：`ReactorHitScript`/`ReactorActScript` 里大量方法随拆分迁入 `Features/`，`script-registration-counts.txt` 按当前扫描结果重刷（2936 → 2778 条）。同时确认 `Easy_balogReactor` 的 ReactorHit 路径是重复召唤（简单难度的召唤由地图脚本 `easy_balog_summon` 完成），删除 `Exped_Balrog.ReactorHitHandler.cs`，`Exped_BalrogEasyEventWiring` 改为断言该脚本名**不**注册 |
