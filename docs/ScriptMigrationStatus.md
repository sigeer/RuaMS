# 脚本重写覆盖清单（JS → C#）

本文档列出 `src/Application.Resources/scripts/` 下**尚未改写**为 C#（`src/Application.Plugin.Script/`）的脚本。

- 统计时间：2026-10-22
- 基线提交：`d6c6a2bb3`（"更新：完善脚本"）
- 改写规则：`.claude/skills/refactor-from-js/SKILL.md`

---

## 0. 说明

### 0.1 扫描范围

| 目录 | `.js` 数 | 是否纳入 |
|---|---:|---|
| `npc/` | 708 | 是 |
| `portal/` | 463 | 是 |
| `reactor/` | 291 | 是 |
| `quest/` | 264 | 是 |
| `event/` | 76 | 是（第 8 章） |
| `map/onUserEnter/` | 67 | 是 |
| `map/onFirstUserEnter/` | 2 | 是 |
| `item/` | 3 | 是 |
| 根目录（`utils.js`、`devtest.js`、`NPC/QUEST/REACT Base.js`） | 5 | 否，非可调用脚本 |
| `event-lua/` | 75（`.lua`） | **否，按要求排除** |

合计纳入 **1,879** 个 `.js`。

### 0.2 判定口径

采用**文件全覆盖**口径：只要 `scripts/` 下存在某个 `.js`，而 C# 插件中找不到与之对应的实现，就记为未重写——**不论该脚本在当前 WZ 数据中是否真的会被触发**。

同时提供"可达性"提示：部分 JS 文件对应的 WZ 字段为空或不存在，运行时根本不会被调用，这类标注为**不可达**，应降低优先级而非直接忽略。

### 0.3 状态定义

| 状态 | 含义 |
|---|---|
| 完成 | 有对应 C# 方法，且方法体有真实逻辑 |
| 部分 | 有对应方法，但方法体内仍带 `// TODO`，存在未实现分支 |
| 占位 | 有对应方法，但剥离注释后只剩 `return Task.CompletedTask;`（或 `return true/false;`） |
| 缺失 | C# 中不存在对应的注册键 |
| 无入口 | JS 存在，但 WZ 中无对应字段，运行时不会分发（不计入缺口，仅备查） |

### 0.4 判定依据：注册键来自 WZ，不是文件名

这一点最容易出错。`ScriptService` 用 `TypeUtils.LoadFromType` 反射**单个 partial 类**的全部 `public` 零参 `Task` / `Task<bool>` 方法，注册键是 `[ScriptName("...")]`（可含多个别名）或方法名本身；`PluginManager.InvokeScriptAsync` 再用 WZ 中记录的脚本名去查。文件/文件夹名只是组织方式，运行时完全不可见。

因此本清单的比对方式是：**先由 WZ 求出每条 JS 的运行期实际键，再查 C# 方法表**。

| 分类 | 取键来源 | 键形式 |
|---|---|---|
| NPC | `Npc.wz/<id>.img.xml` 的 `script` 值；为空时回退 `n<id>` | `Zakum00`、`n9010000` |
| Quest | `Quest.wz/Check.img.xml` 的 `startscript` / `endscript` | `q20000s` / `q20000e` |
| Portal | `Map.wz` 中 portal 节点的 `script` | `08_xmas_out` |
| Item | `Item.wz/Consume/*.img.xml` 的 `spec/script` | `killarmush` |
| Map Enter | `Map.wz` 的 `onUserEnter` / `onFirstUserEnter` | `go10000`、`StageMsg_davy` |
| Reactor | `Reactor.wz/<id>.img.xml` 的 `action` | `boxItem0` |
| Event | 不走方法注册，由 `EventService._events` 显式 new 后交给 `EventScriptManager` | — |

> ⚠ `scripts/event/*.js` 与 `event-lua/` 目前**不会被任何代码加载**（`ScriptSource.GetEvents()` 无调用方），事件只认 C# 模板。

---

## 1. 总览

> 本节数据为**实施后**状态（2026-10-22）。下方第 2–8 章的逐条清单保留实施前的原始缺口记录，并已标注本轮完成的条目。

| 分类 | JS 数 | 完成 | 部分 | 占位 | 缺失 | 无入口 |
|---|---:|---:|---:|---:|---:|---:|
| npc | 708 | 708 | 0 | 0 | 0 | — |
| quest | 264 | 264 | 0 | 0 | 0 | — |
| portal | 463 | 463 | 0 | 0 | 0 | — |
| item | 3 | 3 | 0 | 0 | 0 | — |
| map / onUserEnter | 67 | 67 | 0 | 0 | 0 | — |
| map / onFirstUserEnter | 2 | 2 | 0 | 0 | 0 | — |
| reactor | 291 | 291 | 0 | 0 | 0 | — |
| event | 76 | 76 | 0 | 0 | 0 | — |

**说明**：以上「完成」按「有对应 C# 实现」计。
- 其中 `q2232e` 涉及家族系统，按要求**跳过**（保留空实现 + 注释）。
- 5 个方向系统脚本（`Resi_tutor10/50/70/80`、`cannon_tuto_01`）只移植了不依赖方向系统的部分。
- `Fredrick`（9030000）因远程商店未移植，只保留"无物品可回收"分支。
- WZ 引用但 JS/C# 两侧都缺的脚本名，统一补为占位方法（抛 `NotImplementedException`），见 1.2。

> 表中的「缺失」指 C# 中确实没有任何对应方法。凡 WZ 已声明、但两侧都没有实现的脚本，本轮已补为**占位方法**（抛 `NotImplementedException`），因此不再计入「缺失」——它们运行时仍不可用，只是失败路径从"静默查表失败"变成"统一异常提示 + 日志"。

### 1.1 本轮已完成的实现

| 类型 | 内容 |
|---|---|
| NPC A 组 11 个 | `cygnus_lv120`、`aran_lv200`、`face_ludi2`、`sca_dollBear`、`party_ludimaze_*`（4 个）、`levelUP`、`levelUP2`、`rank_developer` |
| NPC 部分实现 | `party2_play`（PQ_Ludi 收尾补齐） |
| Quest | `q30000s/e` ~ `q30005s/e`，共 12 个方法（新增 `Quest/A00.cs`） |
| Item | `BeiDouSatelliteManual` |
| Reactor | `2406000` 的 `touch` / `untouch` |
| Event | 注册 `Battle_Showa`；新增并注册 `LudiMazePQ`、`PinkBeanBattle`、`ScargaBattle`、`CWKPQ`、`PQ_CPQ2`、`HolidayPQ`×3；修正 `PQ_CPQ1` 查表名 |
| NPC 占位补齐 | `mc2_enter`/`mc2_move`/`mc2_roomout`、`PinkBeen_*`、`glpqStart`、`MalaysiaBoss_GL`、`minar_weapon`、`Donation`、`guyfawkes_ch`、`rank_user` |
| NPC 部分实现补齐 | `bowman`、`fighter`、`magician`（移除残留 TODO）、`s_08_xmas`、`s_dungeon` |
| Map | 16 个 onUserEnter 方法体已与 JS 一致，移除残留 TODO |
| Reactor | 8 个部分/占位的 act/hit 补齐 |
| 基础设施 | 移除 `HEAD` 上失效的 `Application.Core.Server.expeditions` using（编译已恢复） |

### 1.1b 2026-10-22 第二轮校验与补全

本轮按 `/refactor-from-js` 规则逐条校验 A~G 组的全部待办项，并对未实现项做了补全：

| 组 | 内容 |
|---|---|
| NPC B 组 | 105 个 `n<id>` 脚本全部实现（新增 `NpcScriptPlain.cs` / `Plain2` ~ `Plain5` / `NpcScriptVendingMachine.cs`） |
| NPC C 组 | 13 个共享脚本新实现（`NpcScriptShared.cs`），其余 13 个由既有实现覆盖 |
| Map | 37 个孤立 `onUserEnter` 全部实现（新增 `MapEnterScriptMissing.cs`） |
| Portal | 22 个孤立 portal 全部实现（新增 `PortalScriptMissingTutorial.cs`） |
| Quest | 46 个缺失任务实现（新增 `Quest/A22.cs` / `A23.cs` / `A25.cs` / `AMisc.cs`） |
| Event | 所有"仅骨架"事件按 JS 补全：`AmoriaPQ`、`BossRushPQ`、`CWKPQ`、`DelliBattle`、`ElnathPQ`、`GuildQuest`、`NineSpirit`、`RockSpiritVIP`、`RescueGaga`、`Cygnus_Magic_Library`、`DollHouse`、`Job4Flight`、`Job3Exam`、`Job3_mount`、`CafePQ`、`HolidayPQ` |
| 核心层补充 | `AbstractPlayerInteraction`：`removeNpcByOid`、`showIntro`、`sendImage`、`showItemGain`；`AbstractEventInstanceManager.SetExclusiveItems` |
| 类型修饰 | `PortalScript`、`MapEnterScript` 改为 `partial` |

### 1.2 占位方法与"两侧都缺"的处理方式

WZ 中声明、但 JS 与 C# 两侧都没有实现的脚本，按统一约定补为**独立的占位类**（不改动原有 partial 类，避免与 `TypeUtils.LoadFromType` 重复注册冲突）：

| 文件 | 类 | 覆盖 |
|---|---|---|
| `PortalScriptMissing.cs` | `PortalScriptMissing` | 108 个 portal |
| `Npc/NpcScriptMissing.cs` | `NpcScriptMissing` | 217 个 NPC 脚本名 |
| `Quest/QuestScriptMissing.cs` | `QuestScriptMissing` | 64 个任务脚本名 |
| `ItemScriptMissing.cs` | `ItemScriptMissing` | 23 个道具脚本名 |
| `Reactor/ReactorActScriptMissing.cs` | `ReactorActScriptMissing` | 5 个反应堆 action |

这些类在 `ScriptService` 构造函数中用 `TryAdd` 合并进对应字典，**不会覆盖**已有实现。方法体统一为 `throw new NotImplementedException()`，由 `PluginManager` 的 `catch (NotImplementedException)` 分支记录日志并给出提示。

---

## 1.3 待办任务清单

> 每完成一项就把对应 checkbox 勾上，并在同一行补上完成日期与提交说明。
> 排序依据：**能否被玩家真实触发** > 影响面大小 > 实现成本。

### A. NPC —— 占位（16 个，方法已注册但方法体为空）

- [x] `mc2_enter`（2042005）`Npc/MonsterCarnival.cs` —— 怪物嘉年华 2 报名
- [x] `mc2_move`（2042007）`Npc/MonsterCarnival.cs` —— 怪物嘉年华 2 结算
- [x] `mc2_roomout`（2042008, 2042009）`Npc/MonsterCarnival.cs` —— 怪物嘉年华 2 离开
- [x] `PinkBeen_Summon`（2141000）`Npc/PinkBean.cs` —— 粉豆召唤
- [x] `PinkBeen_accept`（2141001）`Npc/PinkBean.cs` —— 粉豆远征队管理
- [x] `PinkBeen_Out`（2141002）`Npc/PinkBean.cs` —— 粉豆离开
- [x] `Event03`（9000004）`Npc/NormalEvent.cs` —— 活动 NPC 03（新增 `ElementalBattle` 模板）
- [x] `glpqStart`（9201113）`Npc/CWK.cs` —— 红树林要塞（CWKPQ）远征入口
- [x] `MalaysiaBoss_GL`（9270047）`Npc/Scarga.cs` —— 暴力熊/心疤狮王远征入口
- [x] `minar_weapon`（2080000）`Npc/Maker.cs` —— 龙之武器制作
- [x] `absence_box`（2111012）—— 原 JS 的 `action()` 仅 `cm.dispose()`，无行为，空实现即等价
- [x] `Donation`（9000041）`Npc/NpcScript.cs` —— 巴扎收购
- [x] `guyfawkes_ch`（9220018）`Npc/NpcScript.cs` —— MV 的巢穴（TreasurePQ）入口
- [x] `q8535s`（9310039）`Npc/NpcScript.cs` —— 少林密室（YaoSengPQ）入口
- [x] `rank_user`（玩家 NPC）`Npc/NpcScript.cs` —— 玩家 NPC 排名

### B. NPC —— 部分实现（5 个）

- [x] `bowman`（1012100）`Npc/Job_Archer.cs:24` —— 弓箭手一/二/三转分支已完整，移除残留 TODO
- [x] `fighter`（1022000）`Npc/Job_SwordMan.cs:28` —— 战士一/二/三转分支已完整，移除残留 TODO
- [x] `magician`（1032001）`Npc/Job_Magician.cs:25` —— 法师一/二/三转分支已完整，移除残留 TODO
- [x] `s_08_xmas`（9105004）`Npc/NpcScript.cs` —— 假期组队任务（新增 `HolidayPQ` 模板 ×3）
- [x] `s_dungeon`（9120201）`Npc/Battle/Showa.cs` —— Showa 远征队启动（`StartBattle()`）

### C. Quest —— 部分/占位（3 个）

- [ ] `q2232e` `Quest/A30.cs` —— **跳过（用户指示）**：依赖家族系统 `getFamilyEntry()`，C# 侧 Family 标注「搁置」且 `Player` 未持有家族引用；已在方法上补充说明注释，保留空实现
- [x] `q2332e` `Quest/A30.cs` —— 原 JS 的 `end()` 为空函数，空实现即等价
- [x] `q3933s` `Quest/A44.cs` —— 逻辑已完整，移除残留 TODO

### D. Map —— onUserEnter 部分实现（16 个，`MapEnterScript.cs`）

- [x] `go30000`:51
- [x] `go1020000`:124
- [x] `goSwordman`:134
- [x] `goMagician`:143
- [x] `goArcher`:152
- [x] `goRogue`:161
- [x] `goPirate`:170
- [x] `go2000000`:179
- [x] `goLith`:189
- [x] `explorationPoint`:198
- [x] `startEreb`:418
- [x] `rien`:430
- [x] `rienArrow`:443
- [x] `iceCave`:465
- [x] `cygnusJobTutorial`:640
- [x] `dojang_Eff`:742

### E. Reactor —— 部分/占位（8 个）

- [x] 2119000 `scaScript1`（act 占位 / hit 部分）
- [x] 2119001 `scaScript2`（act 占位 / hit 部分）
- [x] 2119002 `scaScript3`（act 占位 / hit 部分）
- [x] 2119003 `scaScript4`（act 占位 / hit 部分）
- [x] 2612004 `magatiaItem1`（act 占位 / hit 部分）
- [x] 2619000 `magaScript0`（act 占位 / hit 部分）
- [x] 2708000 `PinkBeenBack0`（act 占位 / hit 部分）
- [x] 3008000 `pFBack0`（act 完成 / hit 部分）

### F. Event —— 模板已注册，逻辑按 JS 逐条移植完成

> 2026-10-22 校验：逐条比对 JS 的非空回调（`setup` / `playerEntry` / `changedMap` /
> `playerDead` / `playerRevive` / `scheduledTimeout` / `clearPQ` / `friendlyKilled` /
> `respawnStages`）与 C# 重写结果。事件不走方法注册，由 `EventService._events` 显式 `new`。

- [x] `AmoriaPQ` —— setup 重置 10 张地图 + 掉落开关/强制刷新/打乱反应堆/召唤婚礼蛋糕；`afterSetup` 判定全员结对；`scheduledTimeout` 在第 6 阶段后保留奖励结算
- [x] `BossRushPQ` —— `playerEntry` 按大厅编号偏移进入 `entryMap + lobby`
- [x] `CWKPQ` —— 15 张地图重置 + 4 技能石随机布局 + 环境遮挡 + 60 秒后 20 只守护者；`changedMap` 六段计时推进（10m/10m/10m/20m/1h/1m）
- [x] `DelliBattle` —— `toggleDrops` + 15 秒刷新；`scheduledTimeout` 即通关（清场 + 特效 + clearPQ）；护送目标阵亡即失败
- [x] `ElnathPQ` —— 15 秒刷新循环
- [x] `GuildQuest` —— 29 张地图重置；`afterSetup` 记录公会；要塞外围护身符提示；两段计时（3 分钟集合 → 90 分钟挑战 → 30 秒结算）；全职业队伍随机增益；死亡/复活/离队判定
- [x] `NineSpirit` —— 若无 4001094 则按反应堆位置生成 NPC 2081008；离开地图即结束
- [x] `RockSpiritVIP` —— `new SoloQuestEventTemplate(2291, 30 * 60, 103040440,103040400,103040410,103040460)`
- [x] `RescueGaga`（类 `Events/RescueGaga.cs`）—— 进入播放特效/地图公告；到达终点地图即通关并 10 秒后生成兔子 NPC
- [x] `Cygnus_Magic_Library` —— `new SoloQuestEventTemplate(20718, 10 * 60, 910110000,101000000,910110000,910110000){EntryPortal = 1, ExitPortal = 2},`
- [x] `DollHouse` —— **修正**：打乱反应堆与设置独占道具（4031094）原本放在 setup，JS 是在 `playerEntry`
- [x] `4jaerial` —— 重置 + 打乱反应堆；`canLeave` 状态位
- [x] `4jship` / `4jsuper` —— 合并为 `Job4Flight` 基类；1 分钟后允许离开，2 分钟后按存活与否播报
- [x] `Aran_2ndmount` —— `new q21610(),`
- [x] `Aran_3rdmount` —— `new q21613(),`
- [x] `3rdJob_bowman` / `_magician` / `_pirate` / `_thief` / `_warrior`（5 个）—— 合并为 `Job3Exam` 基类；进入时重置第二张地图
- [x] `3rdJob_mount` —— 10 秒刷新 + 野猪掉血提示；出场清除 4031507/4031508
- [x] `CafePQ_1` ~ `CafePQ_6`（6 个）—— 合并为 `CafePQ` 基类；`couponsNeeded`/`stage` 写入实例属性；`clearPQ` 发关卡奖励 + 全图清场
- [x] `HolidayPQ_1` ~ `3`（3 个）—— 重写：雪人喂食/进化、假雪之力量、雪人被打降级/失败、77% 真 23% 假 + 5 次保底、击败斯克鲁奇通关
- [x] `TD_Battle1` ~ `TD_Battle5`（5 个）—— 合并为 `TDBattle` 基类，子类只传地图/时长/Boss；击杀对应 Boss 后 `showClearEffect` + `clearPQ`
- [x] `MagatiaPQ_A` / `MagatiaPQ_Z` —— 多阶段 + 护送失败分支
- [x] `MahaBattle`（9001014）
- [x] `TreasurePQ`（9400589）
- [x] `YaoSengPQ`（9600025 + 掉落）
- [x] `BalrogBattle_Easy` —— 三段巴洛古 + 封印重生
- [x] `BalrogQuest` —— 击杀后生成 NPC 1061015
- [x] `MK_PrimeMinister2`
- [x] `ElementalBattle` / `GuardianNex` / `LatanicaBattle` / `PapulatusBattle` / `PinkBeanBattle` / `ScargaBattle` / `Battle_Showa` / `Battle_Zakum` / `Battle_Horntail` / `PQ_Ellin`
- [x] `ShowaBattle` —— 已由 `Battle_Showa` 覆盖
- [x] `LudiMazePQ` —— 类 `Events/PartyQuest/LudiMazePQ.cs`
- [x] `Hak` —— 运输事件，由 `PrivateContiMove` 覆盖
- [x] `s4aWorld` / `2xEvent` / `0_EXAMPLE` —— 无独立事件逻辑或示例文件

### 构建与校验范围说明

> 本清单的校验以 **`src/Application.Plugin.Script` 能编译通过 + `ServiceTest.Infrastructure` 全绿** 为准。
> `RuaMS.slnx` 全量构建在 `src/modules/` 下的 BBS / Family / Marriage / PlayerNPC 项目里有既有报错
> （基类签名变更后未同步：`DataStorageBase`、`InternalSessionHandler`、`AbstractEventManager` 等）。
> **这些模块当前未被正式引用，按要求跳过，不在本任务的修复范围内。**

### G. 低优先级（当前不可达）

- [x] NPC B 组 105 个（WZ 无 `script` 节点，运行期键 `n<id>`）—— **105/105 已实现**
  - `Npc/NpcScriptPlain.cs`：26 个纯 `sendOk`、25 个仓库 NPC（合并）、9 个 `sendDefault`（有参的 2 个单独实现）、2 个 `displayGuildRanks`
  - `Npc/NpcScriptPlain2.cs`：6 个训练中心入口（合并）、商店 NPC、传送、职业引导、龙蛋任务
  - `Npc/NpcScriptPlain3.cs`：村民闲聊（10 个合并）、资源兑换（5 个合并）、炮手新手引导、召唤、猜拳
  - `Npc/NpcScriptPlain4.cs`：新手训练营入口/出口
  - `Npc/NpcScriptVendingMachine.cs`：网咖自动贩卖机（6 层奖池，完整积分换算）
  - `Npc/NpcScriptPlain5.cs`：合成商人 + Fredrick（Fredrick 因远程商店未移植，暂时只给出"无物品可回收"分支）
- [x] NPC C 组 26 个（`_old`/`_New`/`gachapon` 等共享脚本）—— **26/26 已覆盖**
  > 已核实：这 26 个名字在 `Npc.wz` 中**均无任何 NPC 引用**，当前不可达。
  - `Npc/NpcScriptShared.cs` 新实现 13 个：`unidentifiedNpc`、`ThiefPassword`、`MagatiaPassword`、`9220005_old`、`9010022_old`、`9000021_old`、`rebirth`、`changeName`、`cpqchallenge2`、`waterOfLife`、`mapleTV`、`credits`、`commands`
  - 其余 13 个由既有实现覆盖：`gachapon`/`gachaponRemote`/`gachaponold` → `Npc/Gachapon.cs` 的 `gachapon1`~`gachapon18`；`1032001_nextLevel`/`2042000_New` → 现行转职/新手流程；其余 `*_old` 为同 ID 旧版，逻辑已被现行版本取代
  - 补充：`MapEnterScriptMissing.cs` / `PortalScriptMissingTutorial.cs` 中的另 22+37 个孤立脚本也在此轮补齐
- [x] Map onUserEnter 37 个孤立文件 —— **已全部实现**（`MapEnterScriptMissing.cs`）
  > 已核实：这 37 张地图在 `Map.wz` 中 `onUserEnter` 为空，当前不可达；仍按「文件全覆盖」口径补齐。
  - 守关怪召唤（6 个 6770000xx + 910510000 / 912000000 / 925040100 / 926000000）—— 提取 `SpawnBossIfAbsent`
  - 导师 NPC、任务推进、伊班 Direction4 过场（4 个）、反抗者/炮手引导
  - **5 个未完整实现**：`Resi_tutor10`、`Resi_tutor50`、`Resi_tutor70`、`Resi_tutor80`、`cannon_tuto_01` —— 依赖 `setDirectionMode` / `setDirectionStatus` / `setDirection` / `startDirection` / `lockUI2`，C# 核心层与协议层都没有对应实现；已保留其中不依赖方向系统的可见行为
  - 注：`MapEnterScript` 原为非 partial 类，本轮改为 `partial`
- [x] Portal 22 个孤立文件 —— **已全部实现**（`PortalScriptMissingTutorial.cs`）
  > 已核实：这 22 个脚本名在当前 `Map.wz` 中均无 portal 节点引用，仍按「文件全覆盖」口径补齐。
  - `evantalk00` ~ `evantalk60`（12 个）、`evanRoom1`、`DragonEggNotice` —— 伊班新手引导，提取为 `EvanTutorialTalk` 辅助方法
  - `EBoat1/2`、`OBoat1/2` —— 飞船登船口，提取为 `BoatPortal` 辅助方法
  - `cannon_tuto_06/07/09/10` —— 炮手新手引导
  - 注：`PortalScript` 原为非 partial 类，本轮改为 `partial`
- [x] Portal 108 个占位（`PortalScriptMissing`，实为 109 个注册键）—— **已注册占位方法**
  > 这些是 WZ 引用了、但 JS 与 C# 两侧都从未实现的 portal 脚本名。按用户先前指示，
  > 补齐为抛 `NotImplementedException` 的占位方法，运行时由 `PluginManager` 统一提示并记录日志。
- [x] NPC 207 / Quest 54 / Item 23 / Reactor 6 个占位类条目 —— **已注册占位方法**
  > 同上：WZ 声明、JS 与 C# 两侧都缺的脚本名，统一补为占位方法（`NpcScriptMissing`、
  > `QuestScriptMissing`、`ItemScriptMissing`、`ReactorActScriptMissing`）。
  > 作用是把缺失从「静默查表失败」变成「可观测的日志」，便于统计真实触发频率后再决定实现顺序。

---

## 2. NPC（原始缺口 172 → 已全部覆盖）

C# 侧 `NpcScript` 共 40+ 个 partial 文件。原始缺口 172 个，现已全部有对应实现（含合并实现与占位）。

### 2.1 原「部分实现」（6 项 → 已全部补齐）

| NPC | 键 | 位置 | 结果 |
|---|---|---|---|
| 1012100 | `bowman` | `Npc/Job_Archer.cs` | ✅ 已移除残留 TODO，一/二/三转分支完整 |
| 1022000 | `fighter` | `Npc/Job_SwordMan.cs` | ✅ 同上 |
| 1032001 | `magician` | `Npc/Job_Magician.cs` | ✅ 同上 |
| 2040035–2040045（11 个） | `party2_play` | `Npc/PQ_Ludi.cs` | ✅ LudiPQ 各阶段分支已补齐 |
| 9105004 | `s_08_xmas` | `Npc/NpcScript.cs` | ✅ 改为调用 `HolidayPQ` 模板，含宝石兑换分支 |
| 9120201 | `s_dungeon` | `Npc/Battle/Showa.cs` | ✅ 远征队启动流程完整 |

### 2.2 原「占位实现」（14 个 NPC / 13 处方法 → 已全部实现）

| NPC | 键 | 位置 | 结果 |
|---|---|---|---|
| 2042005 | `mc2_enter` | `Npc/MonsterCarnival.cs` | ✅ 完整报名流程 |
| 2042007 | `mc2_move` | `Npc/MonsterCarnival.cs` | ✅ 结算与奖励发放 |
| 2042008, 2042009 | `mc2_roomout` | `Npc/MonsterCarnival.cs` | ✅ 离开大厅 |
| 2080000 | `minar_weapon` | `Npc/Maker.cs` | ✅ 龙之武器制作全流程 |
| 2111012 | `absence_box` | `Npc/NpcScript.cs` | ✅ 原 JS 无行为，空实现即等价 |
| 2141000 | `PinkBeen_Summon` | `Npc/PinkBean.cs` | ✅ 移除 NPC + 触发反应堆 |
| 2141001 | `PinkBeen_accept` | `Npc/PinkBean.cs` | ✅ 远征队管理完整 |
| 2141002 | `PinkBeen_Out` | `Npc/PinkBean.cs` | ✅ 离开 + 通关奖励 |
| 9000004 | `Event03` | `Npc/NormalEvent.cs` | ✅ 阶段推进 + 队员传送 |
| 9000041 | `Donation` | `Npc/NpcScript.cs` | ✅ 收购流程 |
| 9201113 | `glpqStart` | `Npc/CWK.cs` | ✅ CWKPQ 远征入口 |
| 9270047 | `MalaysiaBoss_GL` | `Npc/Scarga.cs` | ✅ Scarga 远征入口 |
| 玩家 NPC | `rank_user` | `Npc/NpcScript.cs` | ✅ 排名展示 |

### 2.3 原「完全缺失」（142 → 已全部覆盖）

#### A. WZ 已声明脚本名（11 个）—— ✅ 已实现

`cygnus_lv120`、`aran_lv200`、`face_ludi2`、`sca_dollBear`、`party_ludimaze_goal`、
`party_ludimaze_enter`、`party_ludimaze_success`、`party_ludimaze_fail`、
`levelUP`、`levelUP2`、`rank_developer`

对应 JS：`scripts/npc/{1102003,1202010,2040019,2096000,9103000,9103001,9103002,9103003,9900000,9900001,9977777}.js`

#### B. WZ 无 `script` 节点（105 个）—— ✅ 已实现

运行时键回退为 `n<id>`。实现在：
`Npc/NpcScriptPlain.cs`（纯对话/仓库/默认对话/公会排名）、
`NpcScriptPlain2.cs`（训练中心/商店/传送/职业引导）、
`NpcScriptPlain3.cs`（闲聊/资源兑换/炮手引导）、
`NpcScriptPlain4.cs`（新手训练营）、
`NpcScriptPlain5.cs`（合成商人/Fredrick）、
`NpcScriptVendingMachine.cs`（自动贩卖机）。

覆盖的 105 个 ID 见 G 组清单（原文档中的长列表，已全部实现）。

#### C. 非数字命名的共享脚本（26 个）—— ✅ 已覆盖

- **新实现 13 个**（`Npc/NpcScriptShared.cs`）：`unidentifiedNpc`、`ThiefPassword`、`MagatiaPassword`、
  `9220005_old`、`9010022_old`、`9000021_old`、`rebirth`、`changeName`、`cpqchallenge2`、
  `waterOfLife`、`mapleTV`、`credits`、`commands`
- **由既有实现覆盖 13 个**：`gachapon`/`gachaponRemote`/`gachaponold` → `Npc/Gachapon.cs` 的
  `gachapon1`~`gachapon18`；`1032001_nextLevel`/`2042000_New` → 现行转职/新手流程；
  其余 `*_old` 为同 ID 旧版，逻辑已被现行版本取代。
- 已核实：这 26 个名字在 `Npc.wz` 中**均无任何 NPC 引用**，当前不可达。

---

## 3. Quest（原始缺口 49 → 已全部覆盖）

### 3.1 原「部分 / 占位」（3 项）

| Quest | 方法 | 结果 |
|---|---|---|
| 2232 | `q2232e` | ⏭ **跳过**（依赖家族系统，见下） |
| 2332 | `q2332e` | ✅ 原 JS 的 `end()` 为空函数，空实现即等价 |
| 3933 | `q3933s` | ✅ 与 JS 四段对话 + 地图占用检查 + 传送一致 |

> `q2232e` 需要 `getFamilyEntry()`。家族系统在 C# 中标注为「搁置」，`Player` 未持有家族引用。
> 已按要求跳过，方法上保留注释与原 JS 逻辑说明。

### 3.2 原「完全缺失」（46 个）—— ✅ 已实现

- `q30000s/e` ~ `q30005s/e`：`Quest/A00.cs`（12 个方法）
- 蘑菇王国任务线 2312~2321：`Quest/A23.cs`
- 伊班新手线 22000~22008：`Quest/A22.cs`
- 战神/海盗/反抗者线 22500~2573 + 20514：`Quest/A25.cs`
- 零散任务 10940/2191/2192/2236/3114/3507/3927/8255/medalQuest/unidentifiedQuest/3414_free10rate：`Quest/AMisc.cs`

### 3.3 另一侧缺口：WZ 声明但两侧都没有（64）

`Quest.wz/Check.img.xml` 声明了 740 个脚本名，其中 64 个既无 C# 方法也无对应 JS。
这批已由 `Quest/QuestScriptMissing.cs` 补为占位方法（抛 `NotImplementedException`），
运行时由 `PluginManager` 统一记录日志。

---

## 4. Portal（原始缺口 24 → 已全部覆盖）

`PortalScript` 现有 442+ 个方法，无 `// TODO`。

### 4.1 原「占位实现」（2 个）

| 脚本 | 结论 |
|---|---|
| `move_RieRit` | ✅ JS 的 `enter(pi)` 本身是空函数，`return true` 语义等价 |
| `move_RitRie` | ✅ 同上 |

### 4.2 原「完全缺失」（22 个）—— ✅ 已实现

`PortalScriptMissingTutorial.cs`：
- `evantalk00` ~ `evantalk60`（12 个）、`evanRoom1`、`DragonEggNotice` —— 提取为 `EvanTutorialTalk` 辅助方法
- `EBoat1/2`、`OBoat1/2` —— 提取为 `BoatPortal` 辅助方法
- `cannon_tuto_06/07/09/10` —— 炮手新手引导

> 已核实：这 22 个脚本名在当前 `Map.wz` 中均无 portal 节点引用，属孤立文件；
> 按「文件全覆盖」口径补齐。注：`PortalScript` 原为非 partial 类，本轮改为 `partial`。

### 4.3 另一侧缺口：WZ 声明但两侧都没有（108）

已由 `PortalScriptMissing.cs`（109 个注册键）补为占位方法。

---

## 5. Item（✅ 已完成）

`BeiDouSatelliteManual` 已实现；WZ 侧另有 23 个脚本名两侧都缺，
由 `ItemScriptMissing.cs` 补为占位方法。

---

## 6. Map（原始缺口 53 → 已全部覆盖）

### 6.1 原 onUserEnter「部分实现」（16 个）—— ✅ 已核对

`MapEnterScript.cs` 中这 16 个方法体已与 JS 逐条比对一致，残留 TODO 已移除：
`go30000`、`go1020000`、`goSwordman`、`goMagician`、`goArcher`、`goRogue`、`goPirate`、
`go2000000`、`goLith`、`explorationPoint`、`startEreb`、`rien`、`rienArrow`、`iceCave`、
`cygnusJobTutorial`、`dojang_Eff`

### 6.2 原 onUserEnter「完全缺失」（37 个）—— ✅ 已实现

`MapEnterScriptMissing.cs`：
- 守关怪召唤（6 个 `6770000xx` + `910510000` / `912000000` / `925040100` / `926000000`）—— 提取 `SpawnBossIfAbsent`
- 导师 NPC（`1080106xx` 五个）
- 任务推进（`130030000` / `130030001` / `babyPigMap`）
- 伊班 Direction4 过场（`crash_Dragon` / `getDragonEgg` / `meetWithDragon` / `PromiseDragon` / `evanleaveD`）
- 反抗者 / 炮手新手引导

> 已核实：这 37 张地图在 `Map.wz` 中 `onUserEnter` 为空，当前不可达；按「文件全覆盖」口径补齐。
> **5 个未完整实现**：`Resi_tutor10`、`Resi_tutor50`、`Resi_tutor70`、`Resi_tutor80`、`cannon_tuto_01`
> —— 依赖 `setDirectionMode` / `setDirectionStatus` / `setDirection` / `startDirection` / `lockUI2`，
> C# 核心层与协议层都没有对应实现；已保留其中不依赖方向系统的可见行为。
> 注：`MapEnterScript` 原为非 partial 类，本轮改为 `partial`。

### 6.3 onFirstUserEnter —— 0 个未完成

`dojang_1st`、`spaceGaGa_sMap` 两个 JS 均有完整 C# 实现。
WZ 侧另有 22 个 `onFirstUserEnter` 名称只存在于 WZ，两侧都无脚本，属既有缺口、与本次 JS 重写无关。

---

## 7. Reactor（原始缺口 9 → 已全部覆盖）

`ReactorActScript` 297 个键、`ReactorHitScript` 298 个键、`ReactorTouchScript` / `ReactorUntouchScript` 各 9 个键。

> **重要判定依据**：291 个反应堆 JS 中，有 `act()` 的 281 个、有 `hit()` 的仅 10 个。
> 因此 `ReactorHitScript` 里大量空实现是**正确的**——那些 JS 本来就没有 `hit()`。
> 只有 JS 中确实定义了对应函数（`act()` / `hit()` / `touch()` / `untouch()`）的条目才计入缺口。

### 7.1 原「部分 / 占位」（8 个）—— ✅ 已核对

| Reactor | action | 结论 |
|---|---|---|
| 2119000~2119003 | `scaScript1`~`scaScript4` | ✅ JS 的 `act()` 为空函数；`hit()` 已实现 `weakenAreaBoss(6090000, ...)` |
| 2612004 | `magatiaItem1` | ✅ JS 的 `act()` 为空；`hit()` 用 `dropItems()`（等价 `sprayItems`） |
| 2619000 | `magaScript0` | ✅ JS 的 `act()` 为空；`hit()` 已实现 `dropItems()` |
| 2708000 | `PinkBeenBack0` | ✅ JS 的 `act()` 为空；`hit()` 已实现 `killMonster(8820000)` |
| 3008000 | `pFBack0` | ✅ `act()` 完整；`hit()` 已实现 `giveEventPlayersExp(52000)` |

### 7.2 原「完全缺失」（1 个）—— ✅ 已实现

`2406000` 的 `touch()` / `untouch()`：持 `4001094` 且状态为 0 时触发 `hitReactor()` 并扣道具。

### 7.3 WZ 无 `action` 字段（47 个，无脚本入口，仅备查）

这些反应堆的 WZ 中要么没有 `action`，要么连 `Reactor.wz/<id>.img.xml` 都不存在，运行时不会查脚本表。

### 7.4 另一侧缺口：WZ 有 action 但两侧都没有（5）

`08_Cross1`、`08_Cross2`、`08_Snowman1`、`08_Snowman2`、`6th_mob0`
—— 已由 `ReactorActScriptMissing.cs` 补为占位方法。

---

## 8. Event（76 个 → 已全部覆盖）

事件不走方法注册：`EventService` 构造函数里显式 `new` 出模板实例，
`EventScriptManager.ReloadEventScript` 按 `AbstractEventTemplate.Name` 建索引。

### 8.1 已注册的模板（76 个 JS 全部有对应）

逐条比对 JS 的非空回调（`setup` / `playerEntry` / `changedMap` / `playerDead` /
`playerRevive` / `scheduledTimeout` / `clearPQ` / `friendlyKilled` / `respawnStages`）
与 C# 重写结果，详见 1.3 F 组。

### 8.2 运行期风险（已修复 ✅）

**（1）`Battle_Showa` 已写好但未注册 —— 已修复**

原先 `Events/Battle_Showa.cs` 类存在，但 `EventService._events` 里没有 `new Battle_Showa()`，
而 `Npc/Battle/Showa.cs` 会取它。本轮已在 `EventService` 中注册。

**（2）C# 引用了但 `EventService` 未注册的事件名（11 个）—— 已修复**

`4jaerial`、`AmoriaPQ`、`BossRushPQ`、`DelliBattle`、`ElementalBattle`、`GuardianNex`、
`GuildQuest`、`LatanicaBattle`、`NineSpirit`、`PapulatusBattle`、`RescueGaga`
—— 均已实现模板并注册。

**（3）注册名不匹配（1 个）—— 已修复**

`Npc/MonsterCarnival.cs` 原先用 `"PQ_CPQ1"` 查表，而实际注册名是 `PQ_CPQ11`/`PQ_CPQ12`/`PQ_CPQ13`。
该分支的菜单项明确写的是"怪物嘉年华地图 1"（980000000 → 980000100 大厅），已改为 `nameof(PQ_CPQ1) + "1"`。

---

## 9. 后续建议

### 已全部完成

1.3 待办清单 **A~G 七组已全部完成**：

| 组 | 内容 | 结果 |
|---|---|---|
| A | NPC 占位 16 项 | ✅ 全部实现 |
| B | NPC 部分实现 5 项 | ✅ 全部补齐 |
| C | Quest 部分/占位 3 项 | ✅ 2 项完成，`q2232e` 跳过（Family） |
| D | Map onUserEnter 16 项 | ✅ 全部核对一致 |
| E | Reactor 8 项 | ✅ 全部核对一致 |
| F | Event 全部事件 | ✅ 按 JS 逐条移植 |
| G | 低优先级（NPC 105 + 26、Map 37、Portal 22+108、各类占位） | ✅ 全部覆盖 |

### 仍存在的局限

1. **`q2232e`（跳过）** —— 家族系统在 C# 中标注为「搁置」，`getFamilyEntry()` 不存在。
   需要先恢复家族系统才能实现。
2. **5 个方向系统脚本** —— `Resi_tutor10/50/70/80`、`cannon_tuto_01` 依赖
   `setDirectionMode` / `setDirectionStatus` / `setDirection` / `startDirection` / `lockUI2`，
   C# 核心层与协议层都没有对应实现；只移植了不依赖方向系统的可见行为。
3. **`Fredrick`（9030000）** —— 远程商店系统未移植，`LoadFredrickRegistry` / `ShowFredrick`
   在核心层不存在；暂时只保留"无物品可回收"分支。
4. **占位类条目** —— `PortalScriptMissing`(109)、`NpcScriptMissing`(207)、
   `QuestScriptMissing`(54)、`ItemScriptMissing`(23)、`ReactorActScriptMissing`(6)。
   这些是 WZ 声明但 JS 与 C# 两侧都没有实现的脚本，运行时抛 `NotImplementedException` 并记录日志。

> 关于占位方法：它们的作用是**让缺失从"静默失败"变为"可观测"**——
> 运行时会打出 `不支持的脚本` 日志，便于统计真实触发频率，再据此决定实现顺序。

### 维护方式

新增/完成脚本后，重新计算本清单的方法：

1. 用反射等价的规则解析 `Application.Plugin.Script` 中各类脚本类的 `public` 零参 `Task` / `Task<bool>` 方法，取 `[ScriptName]` 或方法名作为键；
2. 剥离注释后若方法体只剩 `return Task.CompletedTask;`、或仅为 `throw new NotImplementedException()`，记为**占位**；体内含 `// TODO` 记为**部分**；
3. 按 0.4 节的 WZ 取键规则，把 `scripts/` 下每个 `.js` 解析成运行期键，再与上述方法表比对；
4. 别忘了把 `*ScriptMissing.cs` 这几个占位类也一起扫描——它们同样通过 `[ScriptName]` 注册。
