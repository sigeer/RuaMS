# 事件脚本迁移行为偏差审计

对照 `BeiDou-Server-Dev/gms-server/scripts-zh-CN/event/` 的原始 JS 脚本，逐个核对
`src/Application.Plugin.Script/Features/` 下对应的 C# 事件类，找出**迁移过程中行为发生改变**的地方。

- 范围：名字带 `PQ` / `Battle` / `Quest` 的事件（共 36 个目标）。`AreaBoss*` 及杂项事件不在范围内。
- 比对维度：仅 Event 主体（构造函数常量与奖励表、`OnSetup` / `AfterSeup` / `respawnStages` /
  `changedMap` / `OnMobKilled` / `OnFriendlyMobKilled` / `ClearPQ` / `OnTimeOut` 等覆写）。
  Npc / Portal / Reactor / Map 脚本已确认完整迁移，不在此次比对范围。
- 本文件用于人工逐条确认，未加入 `RuaMS.slnx`。

## 图例

| 标记 | 含义 |
|---|---|
| **HIGH** | 破坏流程：玩不通、通关不判定、无奖励、重复进入被拒 |
| **MEDIUM** | 数值/副作用不对，但流程能走完 |
| **LOW** | 观感或等价行为，影响很小 |
| ✔︎ | 已核对具体代码确认 |
| ？ | 存疑，需运行时验证或进一步定位 |

---

## 零、框架级问题

### F1 地图重置：**不是缺陷**（原判 HIGH，已更正）

最初把「JS 每个 `setup()` 都逐张 `resetPQ`，而 C# 一处都没调用」列为 HIGH 缺陷。经与维护者确认，
C# 的三条机制已覆盖这些场景，**不需要**手动重置：

1. **怪物补充**：`MapleMap.respawn()`（`MapleMap.cs:2053`）遍历 `getMonsterSpawn()`，
   而 WZ 中 `type=m` 且 `mobTime >= 0` 的 life 节点会被注册为 `SpawnPoint`
   （`MapleMap.addMonsterSpawn` `:1752`）。这些点由地图自己的 `OnTick` 周期补充，
   不需要事件侧调用 `resetMapObjects`。
2. **反应堆洗牌**：`MapFactory.cs:253-255` 在读图时就会按 Map.wz 的 `reactorShuffle` 节点自动洗牌。
   已核 WZ：`926100201/202`、`926110201/202`、`930000500`、`610030550` 均**自带** `reactorShuffle`；
   反之 `670010200`（AmoriaPQ）、`674030000`（TreasurePQ）、`922020100`（ElementalBattle）
   **没有**该节点，这些事件里的手动 `shuffleReactors` 才是必要的。
3. **`resetPQ` 已被移除**，无需调用。
4. **`instanceMapRespawn` 同样由 `MapleMap.respawn()` 处理**，事件侧无需调用。

因此下表原先归到 F1 的「缺 resetPQ」条目，**全部不作缺陷处理**，仅在个别事件里保留
「WZ 未声明 shuffle、而 JS 有事件侧洗牌」的真实差异（见各事件条目）。

### F2 阶段奖励经验被乘 0.5　**有意设计，不修**　✔︎

- C#：`AbstractEventTemplate.cs:360-363` 的 `GetExpRate()` 在 `Type == PartyQuest` 时返回
  `PQ_BONUS_EXP_RATE`；`config.yaml:109` 该值为 `0.5`
- Java 原版 `EventInstanceManager.java:1351-1355` 不乘任何倍率
- **结论**：维护者确认 0.5 是有意的全局折扣，**保持现状**

### F3 等级上限 255 vs JS 999　**不需要对齐**　✔︎

- C#：`AbstractEventTemplate.cs:67-72`，`USE_ENABLE_SOLO_EXPEDITIONS` 为真时设 `MinLevel=1`、`MaxLevel=255`、`MinCount=1`
- **结论**：维护者确认不需要对齐，**保持 255**

### F4 `getIntProperty` 缺键返回 0（不抛异常）　事实说明

- C#：`AbstractEventInstanceManager.cs:615-620`，`Convert.ToInt32(null)` 返回 `0`
- 因此「读了未初始化的属性」不会崩服，而是**静默取 0**。下文涉及此类问题的严重度均按此修正

### F5 `damage_ranking` 伤害统计缺失　待补

- JS 各远征在 `playerEntry` / `scheduledTimeout` / `monsterKilled` 里维护并通报伤害排行；
  C# 完全没有实现。Java 侧默认关闭（`damage_ranking=false`），但维护者要求**补上**

### F6 两条映射约定（维护者确认）

**F6.1 远征类事件的「开战时刻」代码放 `OnBattleStarted`，不是 `AfterSeup`。**

- `AfterSeup` 在 `startEvent()`（报名阶段，`AbstractEventInstanceManager.cs:343-349`）触发，
  对远征而言这时玩家还在集合图、战斗尚未开始。
- 远征的 `OnBattleStarted`（`BehindPartyQuestEventInstanceManager.cs:41`，由 NPC 的
  `StartBattle()` 走到）才是 JS 里 `afterSetup` 的对应点。
- 已按此调整：`Zakum.Expedition`、`Exped_Balrog`、`Exped_Balrog.EasyEvent`（原写在 `OnSetup`）、
  `Exped_PinkBean`。剩余 `AfterSeup` 覆写（AmoriaPQ / GuildQuest / MagatiaPQ / OrbisPQ /
  PapulatusBattle / PuppeteerFeature）都是 PQ，不涉及本约定。
- 附带修正：`AbstractExpeditionEventTemplate.AfterSeup` 保留「队长提示」职责，不再承载刷怪。

**F6.2 `SetExclusiveItems` 只在 WZ 没有 `pquest=1` 时才需要。**

- `AbstractEventInstanceManager.dropExclusiveItems`（`:718-726`）第一步就是
  `chr.Bag.ClearPartyQuestItems()`，它按 WZ 的 `info/pquest` 清理 USE/ETC
  （`PlayerBag.cs:207-212`）。因此凡 `pquest=1` 的道具，即使不写 `SetExclusiveItems`
  也会在离场时被回收，重复登记纯属冗余。
- 已逐件核对 `Item.wz`（嵌套感知解析，避免把相邻条目的 `pquest` 误算进来）：

  | 事件 | 道具 | `pquest` |
  |---|---|---|
  | AmoriaPQ | 4031594-4031597 | 全部 `=1` |
  | CWKPQ | 4001256-4001260 | 全部 `=1` |
  | CafePQ ×6 | 4001007 | `=1` |
  | EllinPQ | 4001162/4001163/4001169/2270004 | 全部 `=1` |
  | HenesysPQ | 4001095-4001100 | 全部 `=1` |
  | HorntailPQ | 4001087-4001093 | 全部 `=1` |
  | KerningPQ | 4001007/4001008 | 全部 `=1` |
  | LudiPQ | 4001022/4001023 | 全部 `=1` |
  | LudiMazePQ | 4001106 | `=1` |
  | MagatiaPQ | 4001130-4001135 | 全部 `=1` |
  | OrbisPQ | 4001044-4001063（20 件） | 全部 `=1` |
  | PiratePQ | 4001117/4001120-4001122 | 全部 `=1` |
  | TreasurePQ | 4032118 | `=1` |
  | ZakumPQ | 4001015/4001016/4001018 | 全部 `=1` |
  | **GuildQuest** | 4001024-4001037 | 全部 `=1` |
  | **GuildQuest** | **1032033** | **无 `pquest`** → 必须保留 |
  | **Q3230** | **4031094** 玩偶钥匙 | **无 `pquest`** → 必须保留 |
  | **AriantPQ** | **4031868 / 2270002 / 2100067** | **无 `pquest`** → 必须保留 |

- 结论：`SetExclusiveItems` 只需保留三处——`GuildQuest`（仅 1032033）、
  `Q3230.Event`（4031094）、`AriantEventInstanceManager`（构造里的三件）；
  其余（MagatiaPQ / AmoriaPQ / CWKPQ / CafePQ / EllinPQ / HenesysPQ / HorntailPQ /
  OrbisPQ / PiratePQ / TreasurePQ / ZakumPQ / LudiPQ / LudiMazePQ / KerningPQ）
  的显式登记已移除，交由 WZ 兜底。
- `AriantEventInstanceManager` 的三件是构造器里写死的（没有对应 JS 的 `setEventExclusives`），
  核对结果同为无 `pquest`，行为正确，未动。

---

## 一、PQ 组

### 1. AmoriaPQ　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `AmoriaPQ.Event.cs:16-33`（构造函数无 `StageClearRewards`） | `AmoriaPQ.js:89-98` | JS 有 `expStages [2000,4000,6000,8000,9000,11000]` + `setEventClearStageExp` | 6 关全程 0 经验 |
| MEDIUM ✔︎ | `AmoriaPQ.Event.cs:35-75`（`OnSetup` 无重置） | `AmoriaPQ.js:149-158` | 10 张图逐张 `resetPQ(level)` | 属 F1 |
| MEDIUM ✔︎ | 未覆写 `GetEligibleParty` | `AmoriaPQ.js:101-133` | JS 要求 `onlyMarriedPlayers`、`mask==3`、`hasLeader`、6 人 | 未婚 / 同性别队伍也能开局，夫妻副本的准入校验丢失 |
| LOW ？ | `AmoriaPQ.Event.cs:118-127` | `AmoriaPQ.js:215-221` | JS 在 `statusStg6==1` 时只 warp，C# 无条件 `base.OnTimeOut` → `End` | 结局接近；`statusStg6` 可达性未确认 |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `AmoriaPQ.js:84-87` | JS `[4031594,4031595,4031596,4031597]`，四件 WZ 均 `pquest=1` | 见 F6.2：`ClearPartyQuestItems` 已兜底，显式登记冗余 |
| LOW ✔︎ | `AmoriaPQ.Event.cs:10-14` | — | `EventMaps` 数组声明后全类未使用 | 死代码 |

`MATCH: AmoriaPQ` — 常量（27-40 vs C#:18-31，含 eventTime=75min、maxLobbies=1）；setup 属性（139-147）；
`toggleDrops` 4 图（160-166）；`forceRespawn` 200/500（168-169）；shuffle 750/800（171-172）；
9400536@(942,478)（174-179）；afterSetup `isTeamAllCouple`（187-206）；playerEntry（210-213）；
changedMap / changedLeader / playerDead / playerRevive / playerDisconnected / leftParty / disbandParty 走基类一致。

### 2. BossRushPQ　差异 2 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `BossRushPQ.Event.cs:11-28`（无 `AllClearRewards`） | `BossRushPQ.js:80-112` | JS 6 档奖励池，完成档 `evLevel=6` 由 `npc/9000037.js:134` 领取 | 池为空时 `AbstractEventTemplate.cs:384-393` 直接返回 Success，通关/休息点领取全部「无奖励」 |
| MEDIUM ✔︎ | `BossRushPQ.Event.cs:11-28`（未设 `MaxLobbys`） | `BossRushPQ.js:39` | JS `maxLobbies=7`，C# 默认 1（`AbstractEventTemplate.cs:22`） | `lobbyId` 恒为 0，所有队伍都进 970030100，并发副本数不足 |

`MATCH: BossRushPQ` — 常量（27-37）；setup 的 `level`/`lobby` 属性（139-149）；playerEntry 的 entryMap+lobby（152-155）；
`setEventExclusives` 空（78）等价；changedMap 等走基类一致。

### 3. CWKPQ　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `CWKPQ.Event.cs:66-85`（无 `StageClearRewards`） | `CWKPQ.js:83-96` | JS exp `[2500,8000,18000,25000,30000,40000]`、meso `[500,1000,2000,5000,8000,20000]` | 23 处 `GiveStageClearRewardAll` 全部落空，整套远征 0 关卡经验/金币 |
| MEDIUM ✔︎ | `CWKPQ.Event.cs:87-127` | `CWKPQ.js:216-223` | 缺 610030540 的 5 只 9400594，坐标 x`[944,401,28,-332,-855]` / y`[-204,-384,-504,-384,-204]` | 弓手房守卫者群不生成 |
| MEDIUM ✔︎ | `CWKPQ.Event.cs:118-123` | `CWKPQ.js:158-231` | C# 在 610030100 额外生成 1 只 9400594；JS setup 内无此图刷怪（只有 60 秒后 `spawnGuardians` 刷 20 只，239-250） | 开局多一只守卫（新增行为） |
| MEDIUM ✔︎ | `CWKPQ.Event.cs:87-127` | `CWKPQ.js:182-196` | 15 张图无重置 | 属 F1 |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `CWKPQ.js:78-81` | JS `[4001256…4001260]`，五件均 `pquest=1` | 见 F6.2，有兜底 |

`MATCH: CWKPQ` — 常量与构造参数（26-39）；setup 属性 `glpq*`（158-180）；`generateMapReactors`（104-156）；
`moveEnvironment`（202-214）；610030550 shuffle（199）；60 秒 `spawnGuardians`（229-250）；
playerEntry `current_instance*100`（233-237）；changedMap 六段定时器（256-304）；
scheduledTimeout / clearPQ / changedLeader / playerDead / leftParty / disbandParty（252-254, 301-315, 340-344）；
playerRevive / playerDisconnected 与基类 `IsEventTeamLackingNow` 等价。

### 4. CafePQ（6 个难度）　差异 5 处（HIGH 2）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `CafePQ.CafePQ.cs:24-46`（无 `StageClearRewards`） | `CafePQ_1..6.js:93-94` | 各难度 expStages：20000/15000/12000/21000/25000/10000 | `CafePQ.CafePQ.cs:90` 仍调用 `GiveStageClearRewardAll(1)`，表为空 → 通关 0 经验 |
| **HIGH** ✔︎ | `CafePQ.CafePQ.cs:24-46`（无 `AllClearRewards`） | `CafePQ_1..6.js:89-91` | 各难度通关道具：4001014/4001009/4001013/4001011/4001012/4001010 | `NpcHandler.cs:72` 的 `GiveClearReward` 无池可发，通关道具全丢 |
| MEDIUM ✔︎ | `CafePQ.CafePQ.cs:48-63`（仅 `toggleDrops`） | `CafePQ_1.js:130-135` | JS 还有 `resetPQ` + `instanceMapForceRespawn` | 开局不清理、不立即补怪 |
| LOW ✔︎ | 无 `respawnStages` | `CafePQ_1.js:146-153` | JS 每 15 秒 | 由 `MapleMap.OnTick` 通用 10 秒兜底（`MapleMap.cs:2102-2117`） |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `CafePQ_1.js:80-83` | JS `[4001007]`，`pquest=1` | 见 F6.2，有兜底 |

`MATCH: CafePQ` — 六子类常量全部一致（`CafePQ_1..6.cs:7` vs JS 29-39：couponsNeeded 400/350/350/450/500/300，
highLevelDrop 5/5/3/5/5/5）；minLevel 21 / maxLevel 120；maxLobbies=1；
monsterKilled 阈值（267-282 vs 68-81）；clearPQ 除奖励表外一致（245-255 vs 86-101）；其余走基类一致。

### 5. DavyJohn / PiratePQ　差异 7 处（HIGH 4）　**流程完全不通**

`DavyJohn.Event.cs` 全文仅 27 行，**完全没有 `OnSetup` 覆写**（JS `PiratePQ.js:113-211`）。

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `DavyJohn.Event.cs:7-25` | `PiratePQ.js:117` | `stage2` 未初始化 | `DavyJohn.NpcHandler.cs:115-163` 读到 0（非异常）落入「下一阶段已开放」（:162），勋章上缴链无法开始 |
| **HIGH** ✔︎ | `DavyJohn.Event.cs:7-25` | `PiratePQ.js:249-278` | `curStage` 未初始化、无 `OnPlayerMapChanged` | 6/6/6/6/8 分钟分段计时器全部不存在，只剩 4 分钟（`DavyJohn.Event.cs:23` vs `PiratePQ.js:39`）→ 正常流程必然超时 |
| **HIGH** ✔︎ | `DavyJohn.Event.cs:7-25` | `PiratePQ.js:127-128` | `openedChests` / `openedBoxes` 未初始化 | `PortalHandler.cs:154` 读到 0，判为「未开箱」→ Boss 不刷出、传送门静默失败（**不会抛异常**） |
| **HIGH** ✔︎ | 无 setup 刷怪 | `PiratePQ.js:136-205` | 925100200/300 各 5 组 9300124/9300125；925100201/301 各 10 组 9300112/9300113 | 已核 WZ：925100200/300 无任何 mob，风花房在 C# 是空的；925100201/301 有 `mobTime=-1` 自动生成 |
| **HIGH** ✔︎ | 无 `ClearPQ` 覆写 | `PiratePQ.js:346-355` | 按 `openedChests` 发 28000/35000/42000 经验 + `warpEventTeam(925100600)` | 通关无经验、无收尾传送 |
| MEDIUM ✔︎ | 无 `monsterKilled` 覆写 | `PiratePQ.js:369-394` | 领主死亡广播 + `spawnNpc 2094001@(777,140)` + `showClearEffect` | 缺通关演出 |
| MEDIUM ✔︎ | 无重置/shuffle | `PiratePQ.js:130-203` | 10 张图 | 属 F1 |
| LOW ✔︎ | 无 `respawnStages` | `PiratePQ.js:215-225` | JS 10 秒 | 925100100 的 `mobTime=0` 由通用 tick 覆盖 |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `PiratePQ.js:81-84` | JS `[4001117,4001120,4001121,4001122]`，四件均 `pquest=1` | 见 F6.2，有兜底 |
| LOW ✔︎ | `stage2a`/`stage3a` 未初始化 | `PiratePQ.js:118-119` | `PortalHandler.cs:25/46` 判空为 false | `spawnAllMonstersFromMapSpawnList` 不执行，但 925100201/301 的 `mobTime=-1` 自动生成，无实际影响 |

`MATCH: DavyJohn` — 常量全部一致（`PiratePQ.js:29-41` vs `DavyJohn.Event.cs:9-23`）；eventTime=4min；
`setEventRewards` 空（86）等价；playerEntry；scheduledTimeout=end；changedMap 越界部分（280-291）与基类等价；
changedLeader / playerDead / playerRevive / leftParty / disbandParty 与基类一致。

### 6. HenesysPQ　差异 8 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `PQ_Henesys.Event.cs:15-39`（无 `ClearPQ` 覆写） | `HenesysPQ.js:250-255` | JS `stop + setCleared + warpEventTeam(910010100)` | 走基类（`AbstractEventTemplate.cs:341-345`），通关后不传送，玩家留在蛋糕图 |
| MEDIUM ✔︎ | 无 `OnTimeOut` 覆写 | `HenesysPQ.js:155-162` | JS 有 `"1stageclear"` 时 warp 910010200→910010400，否则 end | 第一阶段已过却超时也直接结束 |
| MEDIUM ✔︎ | 无 `OnPlayerMapChanging` 覆写 | `HenesysPQ.js:182-191` | JS 越界**或 `mapid==910010300`** 时 unregister | 基类 `InInstanceMap`（`AbstractEventTemplate.cs:209-223`）把流放图判为副本内，不退队不结束 |
| MEDIUM ✔︎ | `PQ_Henesys.Event.cs:104-112` | `HenesysPQ.js:259-263` | JS 延迟 5 秒 `bunnyDefeated`；C# 立即 Pink+End | 少 5 秒缓冲与提示 |
| MEDIUM ✔︎ | `PQ_Henesys.Event.cs:81-92` | `HenesysPQ.js:273-283` | C# `% 5 == 0`、永不归零、LightBlue 类型 6；JS `> 5` 时 `serverNotice` 类型 5 并清零 | 触发点、消息通道、计数器状态都不同 |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `HenesysPQ.js:80-83` | JS `[4001095…4001100]`，六件均 `pquest=1` | 见 F6.2，有兜底 |
| LOW ✔︎ | `PQ_Henesys.Event.cs:49-59` | `HenesysPQ.js:129-132` | JS `resetPQ` 含立即首刷；C# `clearMapObjects` + RespawnInterval=15s | 怪物最多晚 15 秒出现 |
| LOW ✔︎ | `PQ_Henesys.Event.cs:62-78` | `HenesysPQ.js:143-153` | C# 每 5 秒强制同步 fullmoon 反应堆（JS 无此行为）；JS 每 15 秒 `instanceMapRespawn` | C# 多出 5 秒反应堆同步（新增行为） |
| LOW ✔︎ | `PQ_Henesys.Event.cs:28` `ClearMap=100000200` | `HenesysPQ.js:32` `clearMap=910010100` | C# 的 `ClearMap` 仅 Dojo 使用（全仓 grep） | 运行时无差异 |

`MATCH: HenesysPQ` — 常量（27-39 vs 15-33）；`StageClearRewards {910010000:(1600,0)}` 对应 expStages `[1600]`（85-95 vs 35-38）；
setup 的 stage/bunnyCake/bunnyDamaged（124-127 vs 41-47）；`friendlyItemDrop` 类型 6 + 年糕计数（265-271 vs 94-102）；
playerEntry；changedLeader / playerRevive / playerDisconnected / leftParty / disbandParty 与基类一致。

### 7. KerningPQ　**完全一致**

`MATCH: KerningPQ` — 常量 12-58 vs JS 27-35；42 项奖励表 vs JS 87-88；
expStages `[100,200,400,800,1500]`（JS:91）与 `StageClearRewards`（C#:51-58）一致；clearPQ 与全部玩家钩子一致。

### 8. LudiPQ　差异 2 处（HIGH 2）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `LudiPQ.Event.cs:122-132` | `LudiPQ.js:92` | 阶段经验错位：C# `922010800=5950` 且**无 `922010900` 键**；JS 第 8 阶段 5040→922010800、第 9 阶段 5950→922010900 | 第 8 阶段多发 910，第 9 阶段（NPC 2040044）0 经验 |
| **HIGH** ✔︎ | `LudiPQ.Event.cs` `OnTimeOut` | `LudiPQ.js:179` | 检查 `ClearedMaps[922011000]`，但该键没人写入 | 通关后不传送到 922011100 领奖 NPC |

> 注：C# `OnTimeOut` 的 `9stageclear` 分支（159-169）在 JS 里也是死代码（全目录仅 `LudiPQ.js:179` 读取、无写入），**不算 drift**。

### 9. LudiMazePQ　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `LudiMazePQ.Event.cs:13-29`（无 `AllClearRewards`） | `LudiMazePQ.js:87-88` | JS 47 项奖励 | 通关 NPC 走「没有奖励」分支，返回 Success 但不发奖 |
| MEDIUM ✔︎ | 无 `OnPlayerEntry` | `LudiMazePQ.js:140-146` | JS 按 entryMap + `random*15` | 所有人固定从 809050000 进 |
| MEDIUM ✔︎ | `LudiMazePQ.Event.cs:31-44` | `LudiMazePQ.js:125` | JS 先 `resetPQ` 再 shuffle；C# 只 shuffle | 属 F1 |
| LOW ✔︎ | `LudiMazePQ.Event.cs:46-51` | `LudiMazePQ.js:232-237` | C# 先 warp 后 `base`；JS 先 `setCleared` 后 warp | 顺序差异 |

`MATCH: LudiMazePQ` — 常量（13-29 vs 27-39）。

### 10. OrbisPQ　差异 2 处（HIGH 0）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| MEDIUM ✔︎ | `OrbisPQ.Event.cs:40-59` | `OrbisPQ.js:138-164` | 27 张图无重置 | 属 F1 |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `OrbisPQ.js:79-81` | JS 20 件（4001044-4001063） | 均 `pquest=1`，见 F6.2，退出仍被清 |

`MATCH: OrbisPQ` — 94 项奖励表逐项一致（vs JS 27-37）；`statusStg` 初值；音乐反应堆按 `DayOfWeek`；
afterSetup 全职业 buff；playerEntry 对话；超时 `statusStg8==1` → 920011300；playerUnregistered 取消 buff。

### 11. EllinPQ　差异 3 处（HIGH 0）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| MEDIUM ✔︎ | `EllinPQ.Event.cs`（全文 48 行，**无 `OnSetup`**） | `EllinPQ.js:112-134` | JS：`statusStg4=0`（:116）、8 张图 `resetPQ`（:118-127）、930000500 `shuffleReactors`（:125）、`respawnStg2`（:129） | 930000500 反应堆不洗牌；第二阶段刷怪由 4 秒变通用 10 秒且需有玩家。`statusStg4` 在 C# 无消费者，不影响 |
| LOW ✔︎ | 继承 `AbstractPartyQuestEventTemplate.cs:23-40` | `EllinPQ.js:97` | JS 限「仅冒险家」职业 | v83 全职业 `/1000==0`，当前无影响 |
| LOW ✔︎ | `EllinPQ.Event.cs:40-46` | `EllinPQ.js:248-252` | JS 第 4 阶段清怪且 `!hasKiller` 也放特效 | 少一种特效触发 |

`MATCH: EllinPQ` — 常量（11-27 vs 29-39）；`setEventRewards` 两端均空；boss 9300182 击杀逻辑。

### 12. ElnathPQ　差异 1 处（HIGH 0）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| LOW/MEDIUM ✔︎ | `ElnathPQ.Event.cs:12-27`（无 `respawnStages`） | `ElnathPQ.js:113-116` | JS 921100300 每 15 秒 `instanceMapRespawn` | 改为通用 10 秒且按玩家人数刷新 |

`MATCH: ElnathPQ` — 常量一致；clearPQ / monsterKilled / allMonstersDead。

### 13. TreasurePQ　差异 5 处（HIGH 2）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | 无 `ClearPQ` 覆写（`TreasurePQ.Event.cs:12-41`） | `TreasurePQ.js:239-244` → `warpBonus` 234-237 | JS `startEventTimer(10*60000)` + warp 674030300 | 杀 MV 后永远不进宝藏房，也没有 10 分钟奖励时间 |
| **HIGH** ✔︎ | `TreasurePQ.Event.cs:12-29` 构造函数无 `StageClearRewards` | `TreasurePQ.js:93` | JS expStages `[60000,100000]` | `:37` 调用 `GiveStageClearRewardAll(mob map)` 但表为空 → 静默返回，两阶段经验全 0 |
| MEDIUM ✔︎ | 无 `OnSetup` | `TreasurePQ.js:126,128` | JS `shuffleReactors` + `statusStg1` | 674030000 无 WZ `ReactorShuffle` 节点，事件侧洗牌是唯一来源 → 箱子布局固定 |
| MEDIUM ✔︎ | 继承 `InInstanceMap`（`AbstractEventTemplate.cs:347-350`） | `TreasurePQ.js:166-182` | JS 额外把 `mapid == 674030100` 视为场外 | 队长在该图时事件不收尾 |
| **等价、非缺陷** ✔︎ | 无 `SetExclusiveItems` | `TreasurePQ.js:80-83` | JS 4032118，`pquest=1` | 见 F6.2，有兜底 |

`MATCH: TreasurePQ` — 常量（12-29 vs 27-37）。

### 14. YaoSengPQ　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `YaoSengPQ.Event.cs:46` | `YaoSengPQ.js:380-399` + `:400-402` | C# 无条件 `clearPQ`（boss 判定只包住掉宝）；JS 非 `BossID` 直接 return，清零走 `allMonstersDead` | **打任何小怪即通关** |
| MEDIUM ✔︎ | 无 `OnSetup` | `YaoSengPQ.js:132-183` | JS boss 生成 / 难度缩放 / 预掷掉落 | WZ 自带 9600025(351,580) 仍在，但无缩放 |
| MEDIUM ✔︎ | 继承 base `ClearPQ`（`AbstractEventTemplate.cs:341-345`） | `YaoSengPQ.js:369-374` | JS 有 2 分钟收尾计时 | 清关后 10 秒即被请出 |
| MEDIUM ✔︎ | `YaoSengPQ.Event.cs:39-44` | `YaoSengPQ.js:21-23,172-181` | C# 固定掉 1 个 2000005；JS 0-5 个、各 40% | 掉宝不符 |
| LOW ✔︎ | `YaoSengPQ.Event.cs:28` `Type=PartyQuest` | `YaoSengPQ.js:2` `isPq=false` | 类型不同 | 影响 `GetExpRate()` 是否乘 0.5 |

`MATCH: YaoSengPQ` — 常量（12-28 vs 3-17）。

### 15. WuGongPQ　差异 4 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `WuGongPQ.Event.cs` 仅构造函数 9-22 + `HandleCreateInstanceResult` | `WuGongPQ.js:381-399`（BossID 9600009 击杀→`clearPQ`）、`:370-375`（2 分钟计时） | 无 `OnMobKilled` / `ClearPQ` / 收尾计时 | **Boss 击杀后事件永不判通关** |
| MEDIUM ✔︎ | `WuGongPQ.Event.cs:21` `EventTime` 30 分钟 | `WuGongPQ.js:13` 10 分钟 | 时限不符 | 时间限制差 3 倍 |
| MEDIUM ？ | 无 `AllClearRewards` | `WuGongPQ.js:84-94`（2000005×5） | 未找到 JS 里 `giveEventReward` 的调用者 | JS 表可能本就没被调用，需确认 |
| MEDIUM ✔︎ | 无 `OnSetup` | `WuGongPQ.js:132-183` | boss 生成 / 缩放 | 无难度缩放 |
| LOW ✔︎ | `Type` 未设（Regular） | `WuGongPQ.js:2` `isPq=true` | 类型不同 | 影响经验倍率 |

`MATCH: WuGongPQ` — 其余常量（9-21 vs 3-11）。

### 16. GuildQuest　差异 7 处（HIGH 2）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `GuildQuest.Event.cs:60` `entryTimestamp=now` | `GuildQuest.js:174-177` | JS `now + 60000*waitTime` | `OnTimeOut`（C#:96-127）的 `canJoin` 判定窗口被跳过 |
| **HIGH** ✔︎ | 无 `dispose` → `reopenGuildQuest` | `GuildQuest.js:329-331` → `:333-335` | JS 结束实例后调 `attemptStartGuildInstance` | 已核 `GuildQuestEventManager.cs:110-134` 唯一调用点是 `:90` 的 `addGuildToQueue` → **队列中后续公会永不自动开局** |
| MEDIUM ✔︎ | `QuestMaps`（`GuildQuest.Event.cs:16-23`）未被使用 | `GuildQuest.js:143-171` | 29 张图无重置 | 属 F1 |
| MEDIUM ✔︎ | 继承 `changedLeader`/`leftParty`/`disbandParty` 会结束事件 | `GuildQuest.js:261,295,297` | JS 三者均为空 | 队长掉线/退队/解散会误结束公会任务 |
| MEDIUM ✔︎ | 缺 `canJoin==0` 前置 | `GuildQuest.js:243-252,286-295` | JS 有 `canJoin` 判定 | 缺少入场拦截 |
| MEDIUM ✔︎ | `GetEligibleParty` 缺同公会过滤且强加 6..30 | `GuildQuest.js:100-140` | JS 要求同公会、仅要求 `hasLeader` | 非本公会成员也能进 |
| LOW ✔︎ | 无 `SetExclusiveItems` | `GuildQuest.js:83-85` | 1032033 **无 `pquest`**，其余 4001024-4001037 均 `=1` | 见 F6.2：只登记 1032033 一件，已在修复记录中处理 |

`MATCH: GuildQuest` — 常量（10-49 vs 27-41）；`OnTimeOut` 主体、`ClearPQ`、`playerDied`、`playerRevive`、
`playerUnregister`、`playerEntry`、`afterSetup`、990000100 对话；`respawnStages` 两端均空。

---

## 二、Battle / Expedition 组

### 17. ZakumBattle　差异 6 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Zakum.Expedition.cs`（无开战/通关/释放钩子） | `ZakumBattle.js:99-101, 218-222, 244-252` | JS 三处调 `updateGateState(1)` / `(0)` / `(0)`，操作 211042300 上的反应堆 2118002 | 入口闸门不随开战打开、失败后也不复位 |
| MEDIUM ✔︎ | `Zakum.Expedition.cs:13-31`（无 `OnSetup`） | `ZakumBattle.js:103-116`（`canJoin=1` 在 105） | C# 未写 `canJoin`，读到 0 | 战斗中断线重连的队员被 NPC 判为「不能再加入」 |
| MEDIUM ？ | `Zakum.Expedition.cs:13-31`（无 `OnSetup`） | `ZakumBattle.js:109` | 缺 `resetPQ(280030000)` | 属 F1 |
| MEDIUM ✔︎ | `Zakum.Expedition.cs:33-47`（无 `OnPlayerUnregister`） | `ZakumBattle.js:195-199` | JS 在已通关时 `em.completeQuest(player, 100200, 2030010)` | 通关后任务 100200 不再自动完成 |
| LOW ✔︎ | `Zakum.Expedition.cs:41-45` | `ZakumBattle.js:236` | JS 调 `broadcastZakumVictory()`（世界中文公告），C# 改为频道英文 `SendDropMessage` | 仅公告文案/语言 |
| LOW ✔︎ | 无对应实现 | `ZakumBattle.js:136-138, 143, 233` | JS 有 `damage_ranking` 伤害统计与通报 | 该配置 Java 默认 false，默认下无差异 |

`MATCH: ZakumBattle` — 构造常量（人数/等级属配置等价）；`setEventRewards`/`setEventExclusives`（JS 均为空）；
`clearPQ` 基础行为；`monsterKilled` 的清怪 + 停表。

### 18. ZakumPQ　差异 2 处（HIGH 0）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **配置等价、非缺陷** ✔︎ | `ZakumPQ.PartyQuest.cs:9` | `ZakumPQ.js:29 + 45` | JS 自身有 `use_enable_solo_expeditions ? 1`，C# `OnMounted` 做同一件事（`config.yaml:83=true`） | 无差异 |
| **配置等价、非缺陷** ✔︎ | `ZakumPQ.PartyQuest.cs:11-13` | `ZakumPQ.js:31 + 46-47` | 等级门槛同属 `use_enable_party_level_limit_lift` 放宽项 | 无差异（C# 缺该开关，见 F3） |
| MEDIUM ？ | `ZakumPQ.PartyQuest.cs:7-24`（无 `OnSetup`） | `ZakumPQ.js:142-183`（`resetPQ` 30 张图 146-175） | 未逐图重置 | 属 F1 |
| **等价、非缺陷** ✔︎ | `ZakumPQ.PartyQuest.cs:7-24`（未设 `SetExclusiveItems`） | `ZakumPQ.js:88-90` | JS 设 4001015/4001016/4001018 | 三件均 `pquest=1`，见 F6.2，有兜底 |

`MATCH: ZakumPQ` — `gotDocuments` 初始化（缺省 0 等价）；`clearPQ`（JS 只 `stopTimer+setEventCleared`）；
`afterSetup`/`respawnStages`/`monsterKilled` 均为空。

### 19. HorntailBattle　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Horntail.Expedition.cs:12-30`（无 `OnSetup`） | `HorntailBattle.js:100-127`（114-120 刷 8810000@(960,120)、8810001@(-420,120)） | JS 在 setup 里直接刷出左右头，C# 完全不刷 | 240060000/240060100 的 WZ life 里没有这两个 id → 头不出现 → `defeatedHead` 恒 0 → `hontale_BR` 封印门（>=1、>=2）永不开启，**副本无法推进** |
| MEDIUM ✔︎ | `Horntail.Expedition.cs:12-30`（无 `OnSetup`） | `HorntailBattle.js:102` | 缺 `canJoin=1` | 断线重连被拒绝重入 |
| MEDIUM ？ | `Horntail.Expedition.cs:12-30`（无 `OnSetup`） | `HorntailBattle.js:107-109` | 缺 3 张图重置 | 属 F1 |
| LOW ✔︎ | `Horntail.Expedition.cs:41-45` | `HorntailBattle.js:213` | JS 调 `broadcastHorntailVictory()`，C# 用频道英文消息代替（`MapleMap.broadcastHorntailVictory` 存在但未用） | 仅公告文案 |
| LOW ✔︎ | 无对应实现 | `HorntailBattle.js:134-138, 141, 209` | `damage_ranking` 缺失 | 默认关闭，无差异 |

`MATCH: HorntailBattle` — 构造常量；`clearPQ`（`stopTimer+setEventCleared`）；`playerUnregistered` 空；
`monsterKilled` 的 8810018 判定、`defeatedHead++`、`dispatchRaiseQuestMobCount(8810018, 240060200)`。

### 20. HorntailPQ　差异 3 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Horntail.PartyQuest.cs:8-25, 27-35`（无 `ClearPQ` 覆写） | `HorntailPQ.js:244-249` | JS 通关后 `warpEventTeam(240050400)`；C# 只走基类 `stopTimer+setEventCleared` | 通关后队伍不传送，需自己走出或等超时 |
| MEDIUM ？ | `Horntail.PartyQuest.cs:27-35`（`OnSetup` 只设 `passKey1-4`） | `HorntailPQ.js:125-134` | 缺 10 张图重置 | 属 F1 |
| **等价、非缺陷** ✔︎ | `Horntail.PartyQuest.cs:8-25`（未设 `SetExclusiveItems`） | `HorntailPQ.js:79-80` | 缺 4001087-4001093 | 均 `pquest=1`，见 F6.2，有兜底 |

`MATCH: HorntailPQ` — 构造常量（6/6、120/255、地图组、25 分钟）；`passKey1-4` `setEventState(0..3)`；
`afterSetup`/`respawnStages`/`monsterKilled` 空。

### 21. PinkBeanBattle　差异 7 处（HIGH 4）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Exped_PinkBean.Event.cs`（无开战钩子） | `PinkBeanBattle.js:98-101` | JS `afterSetup`（开战）提示并 `schedule("startWave", 15s)` 启动首波 | 开战后场上只有 8820000，无小 Boss 可打，**阶段卡死** |
| **HIGH** ✔︎ | `Exped_PinkBean.Event.cs:99-112` | `PinkBeanBattle.js:238-250, 287-294` | JS `startWave` 先 `killMonster` 占位体、再在占位点刷 `id-17`（8820002-8820006）；C# 直接把这些占位怪当真实怪又刷一份，未做 kill/替换 | 出来的不是可计数的小 Boss，占位体残留，「无小 Boss 剩余」判定失真 |
| **HIGH** ✔︎ | `Exped_PinkBean.Event.cs:82-87` | `PinkBeanBattle.js:265-274` | JS 在 stage==5 时于 270050100 反应堆 2708000 位置刷道具 4001193；C# 只发一条提示消息 | **通关所需道具 4001193 永不出现** |
| **HIGH** ✔︎/？ | `Exped_PinkBean.Event.cs:62-94`（无 `OnMobRevive`） | `PinkBeanBattle.js:175-179` | JS `monsterRevive` 对 8820001 调 `enableDrops`；C# 无该钩子 | setup 里 8820000 被 `disableDrops`，`Monster.Revive` 继承 `dropsDisabled`（`Monster.cs:2337-2340`）→ 真品克缤永久不掉落（取决于 8820000→8820001 的 WZ revive 链） |
| MEDIUM ？ | `Exped_PinkBean.Event.cs:43-60` | `PinkBeanBattle.js:113-115` | 缺 3 张图重置 | 属 F1 |
| MEDIUM ✔︎ | `Exped_PinkBean.Event.cs:62-94`（无 `OnPlayerDied`） | `PinkBeanBattle.js:155-169` | JS 累计 `fallenPlayers`，3/4/5 次分别提示、第 5 次 `end(eim)`；C# 只初始化 0，从不累加 | **团灭保护失效**，全员阵亡也不结束远征 |
| LOW ✔︎ | `Exped_PinkBean.Event.cs:66-72` | `PinkBeanBattle.js:260` | JS 调 `broadcastPinkBeanVictory(ch)`，C# 无公告 | 击败无世界公告 |

`MATCH: PinkBeanBattle` — 构造常量；`OnSetup` 的 `canJoin`/`defeatedBoss`/`stage`/`channel` 与 8820000 带 `disableDrops` 刷出；
`clearPQ`（`stopTimer+setEventCleared`）。

### 22. ScargaBattle　差异 2 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Exped_Scarga.Event.cs:17-36`（无 `AllClearRewards`） | `ScargaBattle.js:83-96`（物品表 87-89，29 项） | JS 有完整 29 项通关奖励，C# 未配置 | 奖励 NPC 调 `GiveClearReward` 返回 Success 但不发任何道具/经验 |
| MEDIUM ？ | `Exped_Scarga.Event.cs:38-44` | `ScargaBattle.js:106` | 缺 `resetPQ(551030200)` | 属 F1 |

`MATCH: ScargaBattle` — 构造常量；`canJoin`/`defeatedBoss` 初始化；`monsterKilled`（第二次击杀即通关）；`clearPQ` 基础行为。

### 23. ShowaBattle　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Exped_Showa.Event.cs:115` | `ShowaBattle.js:194-196, 198-203` | C# 判定 `mob.getId() == 8830000`，JS 判定 `9400300` | **击杀真正的 Showa Boss 不触发通关**（8830000 是巴洛古本体 id，本图也不存在），副本无法正常结算 |
| MEDIUM ✔︎ | `Exped_Showa.Event.cs:82-101` | `ShowaBattle.js:181-192`（182 无条件 `killAllMonsters`） | JS 先无条件清光 801040100 再判 `playerDied`；C# 把 `killAllMonsters` 放进 `playerDied==0` 分支，且额外调 `allowSummonState(false)` | 有人阵亡时本图残留怪不被清；无阵亡时本图刷怪被永久关闭 |
| MEDIUM ？ | `Exped_Showa.Event.cs:74-80`（无 `respawnStages`） | `ShowaBattle.js:107, 109, 117-121` | JS setup 里 `resetPQ` 并每 15 秒 `instanceMapRespawn` + 重排 | 801040100 杂兵不再周期补充，长局后半段空场 |
| LOW ？ | `Exped_Showa.Event.cs:12` | — | 事件名 `"Exped_Showa"` 被当作 `ExpeditionEntryType`，枚举里没有 `SHOWA`（`ExpeditionEntryType.cs:10-18`） | 仅在开启每日远征限制时，Showa 不受每日次数约束（默认关闭，`config.yaml:84`） |
| LOW ✔︎ | `Exped_Showa.Event.cs:95-99` | `ShowaBattle.js:190` | 提示文案语言/措辞不同 | 仅文案 |

`MATCH: ShowaBattle` — 构造常量与 29 项奖励表（与 JS 88-89 一致，含 JS 自身重复的 2022306）；
`canJoin`/`playerDied` 初始化；`playerDead` 置 1；`clearPQ` 收尾顺序。

### 24. BalrogBattle　差异 5 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Exped_Balrog.Event.cs:13-30`（未设 `RecruitMap`） | `BalrogBattle.js:31`（`recruitMap=105100100`） | C# 未设 `RecruitMap`（默认 0），而基类 `OnPlayerRegister` 会 `changeMap(RecruitMap)`（`AbstractBehindPartyQuestEventTemplate.cs:62`） | **报名队员被传送到地图 0**，而不是 105100100 大厅。注意同组的 `EasyEvent` 有设 |
| MEDIUM ？ | `Exped_Balrog.Event.cs:13-30`（无 `OnSetup`） | `BalrogBattle.js:130-131` | 缺 2 张图重置 | 属 F1 |
| MEDIUM ✔︎ | `Exped_Balrog.Event.cs:77-91` | `BalrogBattle.js:257-281`（248-255 判定） | JS 只在「未封印巴洛古计数==2」时通关，并给 8830000 安排 10 秒后 `spawnSealedBalrog`；C# 只要 8830000 被杀就立刻 `clearPQ` 并当场刷 8830003 | 击杀时序与封印体出现时机不同（C# 提前且无 10 秒延迟） |
| LOW ✔︎ | `Exped_Balrog.Event.cs:36` | `BalrogBattle.js:132` | JS 在 setup（报名时）排 `releaseLeftClaw` 60 秒，C# 在 `OnBattleStarted`（开战时）排 | 实际触发比线上晚一个报名期（≤5 分钟） |
| LOW ✔︎ | `Exped_Balrog.Event.cs:85-88` | `BalrogBattle.js:267-271` | C# 用世界英文消息代替 `broadcastBalrogVictory` | 仅公告文案 |

`MATCH: BalrogBattle` — 构造常量；`OnBattleStarted` 刷 fake 8830000 + real 8830002 + real 8830006 @(412,258)；
`releaseLeftClaw` 杀 8830006；`dispatchRaiseQuestMobCount(8830003/9101003)`；`"boss"` 缺省 0 等价。

### 25. BalrogBattle_Easy　差异 4 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `Exped_BalrogFeature.cs:38-43`（由 `MapHandler:54` 调用） | `BalrogBattle_Easy.js:151-159` | JS 刷 fake 8830007 + real 8830009 + real 8830013；C# 刷 real 8830010 + fake 8830007 + real 8830013 | 8830010（封印本体）在开战瞬间就以真实怪出现，且少刷一只爪 8830009，前排逻辑被打乱 |
| MEDIUM ✔︎ | `Exped_Balrog.EasyEvent.cs:12-28`（无 `OnSetup`） | `BalrogBattle_Easy.js:130-132, 147-149` | 既无重置，也无 `releaseLeftClaw` 定时（JS 60 秒后杀 8830013） | 第三只爪 8830013 永不退场；其复活链会产生额外可计数击杀，影响通关节奏 |
| **等价** ✔︎ | `Exped_Balrog.EasyEvent.cs`（召唤在 `MapHandler.easy_balog_summon`） | `BalrogBattle_Easy.js:141-143` | JS 由 `afterSetup` → `spawnBalrog`；C# 由 `onFirstUserEnter = easy_balog_summon` 触发（`Map.wz/Map1/105100400.img.xml:17`） | 首次进入必触发（`MapleMap.cs:2642`），召唤一定会发生 |
| LOW ✔︎ | `Exped_Balrog.EasyEvent.cs:38-44` | `BalrogBattle_Easy.js:266` | C# 通关时无 `broadcastBalrogVictory` | 仅公告缺失 |

`MATCH: BalrogBattle_Easy` — 构造常量（含 `RecruitMap=105100100`）；`monsterKilled` 的偏移判定、`"boss"` 计数、
`makeMonsterReal(8830007)`、本体死亡后 10 秒 `SpawnSealedBalrog` 于 (412,258) 刷 8830010；`"boss"` 缺省 0 等价。

### 26. PapulatusBattle　差异 2 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `PapulatusBattle.Event.cs`（无开战/通关/释放钩子） | `PapulatusBattle.js:134-136, 216-220, 238-247` | JS 用 `updateGateState(1)`/`(0)`/`(0)` 操作 220080000 上的反应堆 2208001-2208003 | 时空裂缝闸门不随开战/通关/异常结束切换 |
| MEDIUM ？ | `PapulatusBattle.Event.cs:10-27`（无 `OnSetup`） | `PapulatusBattle.js:125` | 缺 `resetPQ(220080001)` | 属 F1 |

`MATCH: PapulatusBattle` — 构造常量；`"boss"` 缺省 0 等价；`monsterKilled`（8500002→`showClearEffect`+`clearPQ`）；
`clearPQ` 基础行为；`afterSetup`/`respawnStages` 在 JS 侧亦为空。

### 27. LatanicaBattle　差异 1 处（HIGH 0）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| MEDIUM ？ | `LatanicaBattle.Event.cs:10-27`（无 `OnSetup`） | `LatanicaBattle.js:125` | 缺 `resetPQ(541010100)` | 属 F1 |

`MATCH: LatanicaBattle` — 除上述一项外全部一致：构造常量、`"boss"` 缺省 0 等价、
`afterSetup`/`respawnStages` 空、`clearPQ` 基础、`monsterKilled(9420513)`。

### 28. ElementalBattle　差异 3 处（HIGH 1）

| 级别 | C# | JS | 说明 | 后果 |
|---|---|---|---|---|
| **HIGH** ✔︎ | `ElementalBattle.Event.cs:10-34`（未设 `MaxLobbys`，默认 1） | `ElementalBattle.js:39`（`maxLobbies=7`） | C# 每频道只能同时开 1 个大厅 | 同频道第 2 队起无法创建副本 |
| MEDIUM ✔︎ | `ElementalBattle.Event.cs:10-34`（无 `GetEligibleParty` 覆写） | `ElementalBattle.js:97-120`（107 要求 `getJobNiche()==2`） | JS 只允许法师系（niche 2）入队，C# 用基类的「同地图 + 等级」过滤 | 任意职业都能进入，职业限制失效 |
| MEDIUM ？ | `ElementalBattle.Event.cs:36-41` | `ElementalBattle.js:127` | 缺 `resetPQ(922020100)` | 属 F1 |

`MATCH: ElementalBattle` — 构造常量（2/2、100/255、地图组、20 分钟）；`"boss"` 初始化；
`afterSetup`/`respawnStages` 空；`clearPQ` 基础；`monsterKilled`（9300086/9300100 第二次击杀通关）。

### 跨条目补充

- **配置缺口（非缺陷）**：C# 只有 `USE_ENABLE_SOLO_EXPEDITIONS` 的等价实现（`AbstractEventTemplate.cs:69-71`），
  没有 `use_enable_party_level_limit_lift`，等级上限固定 255。若线上开启等级放宽，C# 无法复现（见 F3）。
- **全局缺失**：`damage_ranking` 在 C# 无任何实现；Java 侧默认 false，默认配置下无差异。
- **全局缺失**：`resetPQ` 在 C# 无实现，只有 `MapleMap.resetMapObjects()`（`MapleMap.cs:2292`）；
  `IOpenMap.resetPQ` 仅声明（`IOpenMap.cs:25`）。故所有 resetPQ 条目统一为 MEDIUM + 存疑，
  实际影响受 EIM 地图缓存 1 分钟后销毁（`AbstractEventInstanceManager.cs:542-548`）制约。
---

## 三、存疑项（需运行时验证或进一步定位）

1. **缺失重置是否可见**：`MapManager.cs:42-79` 按 eim 缓存实例地图，EIM 释放后 1 分钟销毁
   （`AbstractEventInstanceManager.cs:542-548`）。若在该窗口内复用大厅，则 F1 及各「缺 resetPQ」条目会变成可见的状态残留；否则加载时的初始生成会掩盖部分表现。
2. **通用 10 秒重生能否替代各事件 `respawnStages`**：`MapleMap.cs:2098-2145` 只对「图内有玩家」的图生效，且不区分事件预期节奏。
3. **`resetMapObjects` 不传 level**：JS 的 `resetPQ(level)` 带难度参数，C# 无对应入参，战斗数值是否有实际影响未验证。
4. **AmoriaPQ `statusStg6` 可达性**：取决于 9201044/9201046 的 stage 计算（按范围未追 NPC 脚本）。
5. ~~**DavyJohn 独占道具**：4001117/4001120/4001121/4001122 是否带 WZ `pquest` 标记~~ → **已核实：四件均 `pquest=1`**，见 F6.2。
6. **WuGongPQ 奖励表**：JS 侧 `giveEventReward` 的调用者未找到，该表可能本就未生效。
7. **PinkBeanBattle 掉落**：`OnMobRevive` 缺失导致「真品克缤永久不掉落」的结论，依赖 8820000→8820001 的 WZ revive 链是否真的走 `Monster.Revive`（已确认 `Monster.cs:2337-2340` 的继承逻辑）。
8. ~~**BalrogBattle_Easy 召唤来源**~~ → **已核实**：`Map.wz/Map1/105100400.img.xml:17` 的 `onFirstUserEnter = easy_balog_summon`，而 `MapleMap.cs:2642-2646` 在首次有玩家进入时必然触发，故召唤一定会发生；且 `EasyEvent.OnSetup` 同时排了 60 秒清 8830013，两条路径都在。
9. **EIM 地图缓存与 resetPQ**：`resetPQ` 在 C# 既无实现也无调用，需先决定补 `resetMapObjects` 调用还是补 `resetPQ` 实现。

---

## 四、汇总

> **状态：本轮已按维护者确认的口径完成全部修复。** 详见文末「修复记录」。

### 原 HIGH 清单（共 21 条，均已处理）

**流程完全不通**

| # | 事件 | 问题 |
|---|---|---|
| 1 | DavyJohn / PiratePQ | 无 `OnSetup`：勋章链断、分段计时器全缺、Boss 不刷、无 `ClearPQ` |
| 2 | WuGongPQ | 无 `OnMobKilled`/`ClearPQ`：杀 Boss 永不判通关 |
| 3 | TreasurePQ | 无 `ClearPQ`：杀 MV 后不进宝库，无 10 分钟奖励时间 |
| 4 | HenesysPQ | 无 `ClearPQ`：通关后留在蛋糕图 |
| 5 | HorntailBattle | 未 spawn 8810000/8810001：封印门永远打不开 |
| 6 | HorntailPQ | `ClearPQ` 未 `warpEventTeam(240050400)`：通关滞留 |
| 7 | PinkBeanBattle | 无 `AfterSeup` → 首波不发；`startWave` 未 kill 占位体；道具 4001193 永不出现；无 `OnPlayerDied` 团灭保护失效 |
| 8 | BalrogBattle | 未设 `RecruitMap`：报名队员被传到地图 0 |
| 9 | BalrogBattle_Easy | 召唤阵容错；无 `OnSetup` 导致 8830013 永不退场 |
| 10 | ShowaBattle | Boss 判定写成 8830000（应 9400300） |
| 11 | ZakumBattle | 无 gate 状态切换；`canJoin` 恒 0 |
| 12 | PapulatusBattle | 无 gate 状态切换（2208001-3） |
| 13 | GuildQuest | 缺 `dispose→reopenGuildQuest`；入场时间戳少等待窗口 |
| 14 | YaoSengPQ | 任意怪死即 `clearPQ`（应只认 9600025） |
| 15 | LudiPQ | 阶段经验错位，无 922010900 键 |
| 16 | LudiPQ | `OnTimeOut` 检查从未写入的 `ClearedMaps[922011000]` |
| 17 | ElementalBattle | `MaxLobbys` 未设，缺法师系职业过滤 |
| 18 | AmoriaPQ / CWKPQ / CafePQ×6 / TreasurePQ | 无 `StageClearRewards` |
| 19 | BossRushPQ / LudiMazePQ / ScargaBattle | 无 `AllClearRewards` |
| 20 | CafePQ×6 | 无 `AllClearRewards` |
| 21 | AmoriaPQ | 无 `GetEligibleParty` 覆写：未婚/同性别队伍也能开 |

### 数量统计

| 分类 | 数量 |
|---|---|
| 框架级 | F1 已更正为「非缺陷」、F2/F3 保持现状、F4 事实说明、F5 待补 |
| HIGH | 21（已修） |
| MEDIUM | 约 30（已修大部分，详见修复记录） |
| LOW | 约 20（已修大部分） |
| 完全一致 | KerningPQ |

### 待你决定

- **F5 `damage_ranking`**：按你的要求**单列后续**，本轮未动
- **F2**：已确认为有意设计（0.5），保持现状
- **F3**：已确认不需要对齐（保持 255）

---

## 五、修复记录（2026-11-09）

### 框架层

| 项 | 改动 |
|---|---|
| F1 | 确认 `MapleMap.respawn()` 已覆盖 `mobTime >= 0` 的 life 节点与 `instanceMapRespawn`，`resetPQ` 已移除 → **不需要手动重置**，相关条目全部撤销 |
| 洗牌 | `MapFactory.cs:253-255` 已按 WZ `reactorShuffle` 自动洗牌；删掉 CWKPQ 对 610030550 的多余调用，保留 Amoria(670010750/800)、Treasure(674030000)、Ellin(930000500)、LudiMaze、Q3230、Pirate4th 等 WZ 未声明的手动洗牌 |
| 新钩子 | `AbstractEventTemplate.OnLobbyDisposed`（原命名 `OnDisposed`），在 `AbstractEventManager.ProcessDisposeInstanceInternal` 里调用，对应 JS 的 `dispose(eim)` |
| F6.1 | 远征的「开战时刻」逻辑统一放 `OnBattleStarted`；`AbstractExpeditionEventTemplate.AfterSeup` 只保留队长提示 |
| F6.2 | `SetExclusiveItems` 只保留 WZ 无 `pquest` 的三处（GuildQuest 的 1032033、Q3230 的 4031094、Ariant 三件），其余交 `ClearPartyQuestItems` |
| 快照 | `script-registration-counts.txt` 移除 6 条已随重构搬家的 Reactor 脚本（`PinkBeenScript0`/`PinkBeenBack0`/`guyfawkes*` 由 Hit 归位到 Act） |

### 已修复事件

| 事件 | 修复内容 |
|---|---|
| **DavyJohn** | 补 `OnSetup`（7 个阶段属性 + 6 处刷怪）、`respawnStages`（甲板按轮次刷）、`OnPlayerMapChanged`（6/6/6/6/8 分钟分段计时）、`ClearPQ`（开箱数决定经验 + 传送）、`OnMobKilled`（领主击杀播报 + 无恙 NPC + 阶段清怪特效 + 苦战模式） |
| **WuGongPQ** | `EventTime` 30→10 分钟；补 `OnSetup`（清怪 + 缩放召唤 9600009）、`OnMobKilled`（仅 Boss 判定 + 0~5 个掉落）、`ClearPQ`（+2 分钟清场） |
| **TreasurePQ** | 补 `StageClearRewards{2:100000}`、`OnSetup`（洗牌 + statusStg1）、`ClearPQ`（10 秒后传送宝库 + 10 分钟计时）、`OnPlayerMapChanging`/`OnLeaderChanged`（674030100 视为场外） |
| **HenesysPQ** | 补 `ClearPQ`（warp 910010100）、`OnTimeOut`（1stageclear 分支）、`OnPlayerMapChanging`（910010300 流放图）、月妙延迟 5 秒失败、伤害计数改 `>5` 清零、年糕提示改 `serverNotice` |
| **ShowaBattle** | Boss 判定 8830000 → 9400300；`ClearPQ` 改回无条件清怪 |
| **BalrogBattle** | 补 `RecruitMap=105100100`；重写 `OnMobKilled`（封印体计数到 2 才通关、本体死后 10 秒重刷） |
| **BalrogBattle_Easy** | `SummonEasy` 改为 fake 8830007 + real 8830009 + real 8830013；补 `OnSetup`（boss=0 + 60 秒清 8830013） |
| **HorntailBattle** | 补 `OnSetup`（canJoin/defeatedBoss/defeatedHead + 刷左右头 8810000/8810001） |
| **HorntailPQ** | 补 `ClearPQ`（warpEventTeam 240050400） |
| **ZakumBattle** | 补闸门 `updateGateState`（`OnBattleStarted` 开 / `ClearPQ` 关 / `OnLobbyDisposed` 关） |
| **PapulatusBattle** | 同上，操作 2208001~2208003 |
| **PinkBeanBattle** | 补 `OnBattleStarted`（15 秒后首波）、`StartWave` 改 kill 占位体再刷 `id-17`、stage5 掉落 4001193、`OnPlayerDied` 团灭保护、`OnMobRevive` 恢复掉落 |
| **GuildQuest** | `entryTimestamp` 改为 now + 等待时长；补 `OnLobbyDisposed`（延迟重开队列）；`SetExclusiveItems` 收窄为 1032033 一件（其余 4001024-4001037 WZ 已带 `pquest`） |
| **YaoSengPQ** | `OnMobKilled` 只在 Boss 时通关；掉落改 0~5 个 × 40%；补 `OnMobClear`、`ClearPQ`(+2 分钟) |
| **LudiPQ** | 阶段经验修正（922010800=5040，补 922010900=5950） |
| **ElementalBattle** | 补 `MaxLobbys=7`、`GetEligibleParty`（仅法师系 niche 2） |
| **AmoriaPQ** | 补 `StageClearRewards`(6 档)、`GetEligibleParty`（全员已婚 + 双性别） |
| **CWKPQ** | 补 `StageClearRewards`(6 档 exp+meso)、补 610030540 的 5 只 9400594、移除 610030100 的多余守卫 |
| **CafePQ×6** | 基类增加 `clearExp`/`clearItem` 参数，补齐 `StageClearRewards` 与 `AllClearRewards` |
| **BossRushPQ** | 补 6 档 `AllClearRewards`、`MaxLobbys=7` |
| **LudiMazePQ** | 补 47 项 `AllClearRewards`、`OnPlayerEntry`（随机起点） |
| **ScargaBattle** | 补 29 项 `AllClearRewards` |
| **EllinPQ** | 补 `OnSetup`（statusStg4 + 930000500 洗牌） |

### 验证

```
dotnet build Application.Core          → 0 错误
dotnet build Application.Plugin.Script → 0 错误
dotnet build Application.Host          → 0 错误
ServiceTest 全量                        → 271 通过 / 187 跳过 / 0 失败
```
