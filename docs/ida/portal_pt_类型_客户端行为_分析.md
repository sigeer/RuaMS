# Map.wz portal 的 `pt` 类型：客户端行为全量分析

> IDA 数据库: `` `Angel.exe ``（v83 客户端，映像基址 `0x400000`，代码段起始 `0x401000`）
> 客户端入口: `CPortalList::RestorePortal` (**0x71165A**) —— 唯一解析 `Map.wz/*/portal/*` 的地方
> 服务端对应: `src/Application.Shared/MapObjects/Portals/PortalConstants.cs`、`src/Application.Core/Server/Maps/PortalFactory.cs`、
> `src/Application.Core/Server/maps/GenericPortal.cs`、`src/Application.Templates/Map/MapPortalTemplate.cs`、
> `src/Application.Core/Channel/Net/Handlers/{ChangeMapHandler,ChangeMapSpecialHandler,InnerPortalHandler}.cs`
>
> 目的：确认 `pt`（客户端 StringPool key **1708**）一共有哪些取值、每个取值在客户端分别会走哪条分支。
> 结论全部来自反编译/反汇编；WZ 只取少量样本作为佐证（不作为行为依据）。

---

## 1. 结论速览

`pt` 在 v83 数据里只有 **0–13** 共 14 种取值（全量 Map.wz 统计无 `pt >= 14`）。
客户端对它们的处理可以按「**会不会被找到**」「**碰/按上键后做什么**」「**有没有图层/小地图图标**」三条线归类：

| pt | 客户端语义 | 触发方式 | 发出的包 | 图层（MapHelper.img） | 小地图 |
|----|-----------|---------|---------|----------------------|--------|
| 0 | 出生点占位（`sp`），**不可交互** | 无（`FindPortal` 显式跳过 `pt==0`） | — | 无 | 否 |
| 1 | 隐藏传送门（只能按上键或脚本寻路命中） | 上键 | `0x26` | 无 | 否 |
| 2 | 可见地图入口 | 上键 | `0x26` | `portal/game/pv` | **是** |
| 3 | 碰撞式地图入口（不可见） | 走进去（碰撞） | `0x26` / `0x65` | 无 | 否 |
| 4 | 可见地图入口（同图内不走上行传送） | 上键 | `0x26` | `portal/game/pv` | 否 |
| 5 | 脚本入口（同图内不走上行传送） | 上键 | `0x26` + 脚本 | 无 | 否 |
| 6 | **时空门落点数据**，不生成 portal 对象 | —（只把坐标塞进 `CField+94`） | — | 无 | 否 |
| 7 | 脚本入口，`tm` 恒 999999999 | 上键 | `0x64` | `portal/game/pv` | **是** |
| 8 | 脚本入口（无图层，不可见） | 上键 | `0x64` | 无 | 否 |
| 9 | **碰到即自动**发请求（可 delay / onlyOnce） | 碰撞（每帧） | `0x64` | 无 | 否 |
| 10 | 同图内可见双向传送门（带 `image`） | 上键 | 同图 `0x65`，跨图 `0x26` | `portal/game/ph` | 否 |
| 11 | 脚本入口（带 `image`，psh 图层） | 上键 | `0x64` | `portal/game/psh` | 否 |
| 12 | 横向弹跳（弹床） | 碰撞 | 无 | 无 | 否 |
| 13 | 竖/横弹射（跳台） | 碰撞（限 fieldType 19/20/21/25 或地图 180000002） | 无 | 无 | 否 |

一句话记法：
**0 占位 / 1 隐形 / 2 可见入口 / 3 走进去 / 4 可见入口(异图) / 5 脚本入口 / 6 时空门点 /
7 上键脚本(可见) / 8 上键脚本(隐形) / 9 碰到即发 / 10 同图双向 / 11 上键脚本(psh) / 12 横弹 / 13 竖跳。**

四个出站操作码与 `pt` 的对应：

| 包 | 含义 | 由哪些 `pt` 触发 |
|----|------|-----------------|
| `0x26` (38) `CHANGE_MAP` | 正常换图（沿用 WZ 里 portal 的 `tm`/`tn`，包内 `targetMapId` 填 **-1**，由服务端按 portal 名解析） | 1, 2, 3, 4, 5, 10 |
| `0x64` (100) `CHANGE_MAP_SPECIAL` | 特殊换图（脚本入口，`tm` 一般 999999999，服务端按 `pn` 找脚本） | 7, 8, 9, 11 |
| `0x65` (101) `USE_INNER_PORTAL` | **同图内**传送到目标 portal（带目标坐标） | 3, 10（同图时） |
| `0x85` (133) `USE_DOOR` | 使用他人放置的时空门 | 站在 `pt=6` 落点附近按上键时 |

---

## 2. 客户端读取：`CPortalList::RestorePortal` (**0x71165A**)

每个 `portal/<n>` 节点按固定顺序读字段（StringPool key → 结构偏移）：

| 顺序 | key | 名称 | PORTAL 偏移 | 说明 |
|------|-----|------|-------------|------|
| 1 | 1707 | `PN` | +0x04 | `pn`，portal 名（ZXString） |
| 2 | 1708 | `PT` | **+0x08** | `pt`，本问的类型 |
| 3 | 977 | `X` | +0x0C | `x` |
| 4 | 978 | `Y` | +0x10 | `y` |
| 5 | 5103 | `HRANGE` | +0x14 | `hrange`（碰撞半宽×2） |
| 6 | 5189 | `VRANGE` | +0x18 | `vrange` |
| 7 | 1709 | `TM` | +0x1C | `tm`，目标地图 |
| 8 | 1710 | `TN` | +0x20 | `tn`，目标 portal 名 |
| 9 | 3770 | `HIDETOOLTIP` | +0x24 | `hideTooltip` |
| 10 | 5624 | `DELAY` | +0x2C | `delay`（pt 9 的触发间隔，ms） |
| 11 | 4340 | `ONLYONCE` | +0x28 | `onlyOnce`（pt 9 去重） |
| 12 | 5188 | `VERTICALIMPACT` | +0x34 | `verticalImpact`（pt 13） |
| 13 | 5101 | `HORIZONTALIMPACT` | +0x38 | `horizontalImpact`（pt 12/13） |
| 14 | 5102 | `REACTORNAME` | +0x3C | `reactorname`（pt 13 生效条件） |

读取完成后：

1. 用 `atoi($name)+1`、`pt`、`onlyOnce`、`x/y/hrange/vrange/tm/tn`、`tn` 字符串参与 **CRC32**，异或进 `CPortalList+0x40`（反作弊校验值）。
2. `if (pt == 6)` → 调用 `sub_52B922(x, y)` 把坐标 append 进 `CField+94` 的 `(x,y)` 数组，**然后直接丢弃该节点，不创建 PORTAL 对象**。
3. 其余 `pt`：**先无条件插入 `CPortalList+0x4`（普通列表）**，再按 `pt` 追加到别的列表/建图层：

| pt | 追加到 +0x8（碰撞列表） | 追加到 +0xC（特殊列表） | 建图层 |
|----|------------------------|------------------------|--------|
| 0 | | | |
| 1 | | | |
| 2 | | | `sub_712313` → MapHelper `portal/game/pv` |
| 3 | ✅（`0x712243`） | | |
| 4 | | | `sub_712313` → `pv` |
| 5 | | | |
| 7 | | | `sub_712313` → `pv` |
| 8 | | | |
| 9 | ✅（`0x712243`） | | |
| 10 | | ✅（并额外读 4385 `IMAGE`） | `sub_71259D` → `portal/game/ph` |
| 11 | | ✅（并额外读 4385 `IMAGE`） | `sub_712827` → `portal/game/psh` |
| 12 | ✅（`0x712243`） | | |
| 13 | ✅（`0x712243`） | | |

> 三条列表：`+0x4` 普通（`FindPortal` 用，**所有非 6 的 portal 都在里面**）、
> `+0x8` 碰撞（`FindPortal_Collision` 用，只有 3/9/12/13）、
> `+0xC` 特殊（`pt` 10/11，供 `OnResolveMoveAction` 做图层动画）。
> `pt=10/11` 额外读的 `IMAGE`（4385）为空时回填 StringPool 960 `DEFAULT`（即 `ph/default`）。

查找函数：

- `CPortalList::FindPortal` (**0x712AB1**)：遍历 `+0x4`，**倒序**，跳过 `pt == 0`；
  命中矩形 = `[x±k, y-50, y+50]`，`k` 由调用方给（上键 20，其它 50）。
- `CPortalList::FindPortal_Collision` (**0x712B61**)：遍历 `+0x8`，倒序，命中矩形 = `[x±hrange/2, y±vrange/2]`。
- `sub_712B24`：按**名字**在 `+0x4` 里线性找，供"指定目标 portal 名"的传送用。

---

## 3. 各 `pt` 的客户端行为（逐条）

### pt = 0 —— 出生点占位
- 数据上恒为 `pn="sp"`, `tm=999999999`, `tn=""`。
- `FindPortal` 里 `if (portal[2])`（即 `pt != 0`）直接跳过，所以**上键永远选不到它**。
- 不建图层、小地图不画。
- 作用：仅作为地图出生点/复活点坐标由服务端挑选（服务端 `MapleMap.findClosestPlayerSpawnpoint` 一类逻辑）。

### pt = 1 —— 隐藏传送门
- 只在 `+0x4`，无图层，小地图不可见。
- 上键命中后走 `CUserLocal::HandleUpKeyDown` (**0x94DF9B**) 的通用分支：
  - `tm == 999999999` → 不做任何事（跳到"时空门检查"）；
  - `tm == 当前地图` 且 `pt ∉ {4,5}` → `TryRegisterTeleport` → **`0x65`**（同图内换落点）+ `SP_5562` 音效；
  - 否则 → `CField::SendTransferFieldRequest(-1, pn, …)` → **`0x26`** + `SP_5562` 音效。
- 典型用途：脚本/服务端"warp 到某地图的指定 `tn`"（数据里 `tm` 指向本图 1372 处、指向异图 752 处）。

### pt = 2 —— 可见地图入口
- 建 `MapHelper.img/portal/game/pv` 图层；**小地图会画**（`CUIMiniMap::Update` **0x859122** 只对 `pt==2 || pt==7` 画标记）。
- 上键 → `0x26`；跨图落地后播 `portalExit`(4390) 退场动画（`sub_712D35`）。

### pt = 3 —— 碰撞式地图入口
- 只在碰撞列表，无图层，玩家走过去即触发（`CUserLocal::CheckPortal_Collision` **0x94DAC6**，每帧由 `CUserLocal::Update` 调用）：
  - `tm == 999999999` → 返回（无动作）；
  - `tm == 当前地图` → `TryRegisterTeleport(pn, tn)` → `0x65`；
  - 否则 → `SendTransferFieldRequest(-1, pn)` → `0x26`；
  - 成功后播 `SP_5562`。
- 命中矩形用 `hrange/vrange`（数据里只有少数地图配了这俩，其余为 0 宽高的点，靠玩家脚下的判定框压线）。

### pt = 4 —— 可见地图入口（异图专用）
- 同 `pt=2` 的 `pv` 图层，**但小地图不画**。
- `HandleUpKeyDown` 里同图分支显式排除 `pt==4`（`if (v9 != 4 && v9 != 5)`），即**永远不做同图 `0x65`**，直接 `0x26`。
- 数据量极少（全量 9 个）。

### pt = 5 —— 脚本入口
- 无图层；同图分支同样被排除，直接 `0x26`。
- 数据里几乎都带 `script`（16/19），典型如 `103000800` 的 `next00 → script=kpq0`。
- 客户端只负责发 `0x26`；"走脚本"是服务端 `GenericPortal.enterPortal` 的事。

### pt = 6 —— 时空门落点数据点
- `RestorePortal` 读到就 `sub_52B922(x,y)` → `CField+94` 数组，然后**丢弃**，不生成 `IPortal`。
- 使用方：
  - `CField::GetTownPortalPos` (**0x5377FA**)：按索引取 `+94` 里的点；索引超界时**随机取一个普通 portal** 的 `(x,y)`。
  - `sub_5375ED`（地图自带时空门列表）/ `sub_7BD594`（玩家技能时空门池）：玩家按上键且**没有**普通 portal 命中时，用 `[x±20, y±50]` 检查是否站在落点上，符合就发 **`0x85`**（`mapId` + `Encode1(0/1)`），并置 `ctx+2089=1`、刷新 `ctx+2090` 阻止重复发送。
- 数据上 254 个，全部 `tm=999999999`、无 script、无 `pn` 语义。

### pt = 7 —— 上键脚本入口（可见）
- `pv` 图层 + 小地图标记（与 pt 2 同一套）。
- `tm` 恒 999999999。`HandleUpKeyDown` 命中后：
  `CanSendExclRequest(ctx, 500, 0)` 通过 → 发 **`0x64`**：`Encode1(fieldId)`、`EncodeStr(pn)`、`Encode2(playerX)`、`Encode2(playerY)`；并置 `ctx+8356=1`、`ctx+8360=now`（500ms 节流）。
- 数据：1221 个里 1220 个带 script，649 个带 `onlyOnce`、671 个带 `delay`（这两个字段客户端只在 **pt 9** 分支读，pt 7 不看）。

### pt = 8 —— 上键脚本入口（不可见）
- 与 pt 7 完全同一分支（`pt ∈ {7,8,11}` 同判），只是**不建图层、不上小地图**。
- 数据：662 个，全部 `tm=999999999` 且全部有 script。

### pt = 9 —— 碰到即自动发请求（用户举的例子）
- 在碰撞列表；`CUserLocal::CheckPortal_Collision` 专门分支：

```
if CanSendExclRequest(ctx, portal.delay /*+0x2C, 毫秒*/, 0)
   && (!portal.onlyOnce /*+0x28*/ || portal.id != this[3181])   // 本 portal 上次触发的实例
then
   COutPacket(100)                  // 0x64
     Encode1(fieldId)               // get_field()+308
     EncodeStr(pn)
     Encode2(playerX)
     Encode2(playerY)
   置 ctx+8356 = 1, ctx+8360 = now  // 与 0x26 共用同一套 500ms 发送锁
   this[3181] = portal.id           // 记住本次触发的是哪个 portal
```

- `delay` 是**两次触发的最小间隔**（`CanSendExclRequest` 的 `interval` 参数），`onlyOnce=1` 时同一 portal 实例只会自动触发一次。
- 注意它**没有** `pt 7/8/11` 那条 500ms 的 `SendTransferFieldRequest` 节流判断，走的是 `CanSendExclRequest`；但发完会把 `ctx+8356` 置 1，因此紧接着的 `0x26` 会被压掉。
- 数据：661 个，全部 `tm=999999999` 且有 script；143 个带 `delay`、44 个带 `onlyOnce`。

### pt = 10 —— 同图内可见双向传送门（带 image）
- 进**特殊列表 `+0xC`**，并额外读 `IMAGE`(4385)，空则用 `DEFAULT`(960)。
- 图层：`MapHelper.img/portal/game/ph/{image|default}`；`pt=10` 的旧 portal 用 `sub_71259D` + `portalStart`(4388)/`portalExit`(4390) 切动画，`pt=11` 用 `sub_712827` + `psh` + `portalContinue`(4389)/`portalStart`(4388)。
- 上键：同图且 `pt ∉ {4,5}` → `TryRegisterTeleport` → **`0x65`**；跨图 → `0x26`。
- 数据：226 个，`tm` 等于本图 137 个、指向异图 89 个，88 个带 `image`。

### pt = 11 —— 上键脚本入口（psh 图层 + image）
- 进特殊列表 `+0xC`，额外 `IMAGE`；图层 `portal/game/psh/...`。
- `HandleUpKeyDown` 把它与 7/8 归为一类 → `0x64`。
- 数据：29 个，全部 `tm=999999999` 且有 script，17 个带 `image`。

### pt = 12 —— 横向弹跳
- 在碰撞列表。命中后 `sub_94DE22` (**0x94DE22**)：
  - `SetMovePathAttribute(18)`；
  - 水平速度 = `(右键按下?100:0) + (左键按下?-100:0) + (朝左? -200 : +200)`；
  - 垂直速度 = `dbl_B3D330 = -2300.0`；
  - 播 `SP_2731`（`Effect/BasicEff.img/verticalJump`）特效。
- 只要**踩上去**就弹，不需要按键。`CUserLocal::FallDown` (**0x94C4F8**) 与 `DoActiveSkill_CombatStep` (**0x96D95E**) 在脚下碰撞 portal 是 12/13 时会**放弃**坠落/战斗步逻辑（防止冲突）。
- 数据：261 个，全部 `tm=999999999`。

### pt = 13 —— 竖/横弹射（跳台）
- 在碰撞列表。命中后：
  - **前置条件**：`fieldType ∈ {19,20,21,25}`（`CField` vtable+0x1C）或 `mapId == 180000002`；
  - **reactor 条件**：`reactorname` 为空 → 直接生效；非空 → 在 `CReactorPool`(dword_BED774，`sub_9614D6`) 里查该名字，**只有查到且其状态值 == 1** 才生效（`sub_735D13` **0x735D13**）；
  - `SetMovePathAttribute(19)`；
  - `sub_7A6353(vecCtrl, horizontalImpact, -verticalImpact)`；
  - 播 `SP_2731` 特效。
- 数据：134 个，全部有 `verticalImpact`（1100/1500/1700…），103 个有 `horizontalImpact`，85 个带 script（脚本此处不参与弹射，是服务端别的逻辑用）。

---

## 4. 包结构（客户端 → 服务端，按编码顺序）

### 4.1 `0x26` CHANGE_MAP —— `CField::SendTransferFieldRequest` (**0x53035D**)

```
Encode1  fieldId            // get_field()+308
Encode4  targetMapId        // portal 触发时固定为 -1（由 pn 解析）；GM/死亡路径才是真实地图
EncodeStr portalName        // pn；为空时写 ""
if portalName != "": Encode2 x, Encode2 y        // 玩家当前坐标
Encode1  0
Encode1  a3                 // 调用方参数（portal 路径传 0）
Encode1  dword_BED760       // 是否附带调试坐标
if dword_BED760: Encode4 a4, Encode4 a5
```
函数入口自带节流：`if (ctx+8356 || now - ctx+8360 < 500) return 0;`，发送成功后置 `ctx+8356=1`、`ctx+8360=now`。

对服务端 `ChangeMapHandler`：`readByte()`(fieldId→被当 1/0)、`readInt()`(=-1)、`readString()`(pn)
正好对应，`targetMapId == -1` 时服务端不换图，交给 `portal.enterPortal(c)` 用 `pn` 重新找 portal，逻辑是对的。

### 4.2 `0x64` CHANGE_MAP_SPECIAL

客户端发送点（两处，字段一致）：
- `HandleUpKeyDown`：pt 7/8/11 且 `tm == 999999999`；
- `CheckPortal_Collision`：pt 9 自动触发。

```
Encode1  fieldId
EncodeStr pn
Encode2  playerX
Encode2  playerY
```
服务端 `ChangeMapSpecialHandler` 读 `byte` + `string` + `short` 后按 `startwp` 找 portal 再 `enterPortal`，
**丢弃了 playerY**（客户端发的是 x、y 各 2 字节）。

### 4.3 `0x65` USE_INNER_PORTAL —— `CUserLocal::TryRegisterTeleport` (**0x957B74**)

只有"指定目标 portal 名"这一支会发包（`sTargetPortalName != null`，且目标 portal 存在、脚下有 foothold）：

```
Encode1  fieldId
EncodeStr sourcePortalName      // 源 portal 的 pn
Encode2  playerX
Encode2  playerY
Encode2  targetPortalX
Encode2  targetPortalY
```
同一次调用还会：`SendSkillUseRequest`、`SetMovePathAttribute(4)`、播 `SP_2180`（`teleport` 特效）、
写 `this+2758/2759/2760/2761/2762` 并设 **120ms** 冷却（`this+2763`）。

服务端 `InnerPortalHandler` 目前只打日志，**未实现**。
（与之相对的 `0xCF`(207) `OnTeleport` 是**服务端 → 客户端**的强制传送：`Decode1` 清节流、`Decode1` 取 portal 索引后调同一个 `TryRegisterTeleport`。）

### 4.4 `0x85` USE_DOOR

`sub_5375ED` / `sub_7BD594` 在玩家站在 `pt=6` 落点（或本地时空门池）上按上键时发送：

```
Encode4 mapId / portalId
Encode1 0 或 1
```
发送前置 `ctx+2089=1`、`ctx+2090=now`（`CanSendExclRequest` 的忽略 HP 变量在此被借用为"门使用中"）。

---

## 5. WZ 样本（举例，非行为依据）

| 地图 | 节点 | pt | 关键属性 | 说明 |
|------|------|----|----------|------|
| `Map1/109090300.img.xml` | 0–2 `sp` | 0 | `tm=999999999` `tn=""` | 出生点占位 |
| 同上 | 3 `sheep0` … 25 `wolf7` | 1 | `tm=999999999` 部分带 `horizontalImpact=0` | 脚本寻路用的命名点 |
| 同上 | 26 `jump6` | 13 | `verticalImpact=1500` `horizontalImpact=10` `script=""` | 跳台 |
| `Map1/130030000.img.xml` | 1 `west00` | 2 | `tm=130030001` `tn=east00` | 可见跨图入口 |
| 同上 | 2 `scr00` | 9 | `script=tutorHelper` `delay=200` `onlyOnce=0` | 碰到就发 0x64 |
| `Map1/106020403.img.xml` | 3 `jump00` | 12 | `horizontalImpact=0` | 横向弹床 |
| `Map1/100000000.img.xml` | `up00` | 1 | `tm=100000000` `tn=in03` | 同图落点 |
| 同上 | `in03` | 2 | `tm=100000002` `tn=out00` | 可见入口 |
| 同上 | `hp00` | 10 | `tm=100000000` `tn=hp00_1` `image=""` | 同图双向门（回退 default 图层） |
| `Map1/103000800.img.xml` | `next00` | 5 | `script=kpq0` `tm=103000801` `tn=st00` | 脚本入口 |

全量 Map.wz 的 `pt` 分布（仅作存在性佐证）：`0:8800, 1:3435, 2:2088, 3:1484, 4:9, 5:19, 6:254,
7:1221, 8:662, 9:661, 10:226, 11:29, 12:261, 13:134`，**无 `pt >= 14`**。

---

## 6. 与服务端的对应关系

| 客户端语义 | 服务端现状 | 差异 |
|-----------|-----------|------|
| `pt` 0–13 | `PortalConstants` 只定义 `TELEPORT_PORTAL=1`、`MAP_PORTAL=2`、`DOOR_PORTAL=6` | **12 种类型没有具名常量**，注释「9. 触碰触发」挂在类上而不是常量上 |
| 建 portal | `PortalFactory.makePortal(type, tpl)`：2→`MapPortal`，6→`MysticDoorPortal`，其余→`GenericPortal(type)` | 行为上可用（`enterPortal` 只看 script/tm），但 9/12/13 这类"客户端自行处理"的类型在服务端没有语义区分 |
| `pt=6` | 被视为 `MysticDoorPortal` 并分配 id `0x81+` | 客户端 `pt=6` **根本不生成 portal 对象**，只把坐标注册到 `CField+94`。服务端把地图数据里的落点当成门实体，属于实现差异 |
| `pt=9` 碰到即发 | 靠 `ChangeMapSpecialHandler` 收到 `0x64` 后跑脚本 | 可用，但**不校验** `delay`/距离/是否真站在 portal 上 |
| `pt=3` 走进去 | `ChangeMapHandler` 收到 `0x26` 后 `distanceSq > 400000` 拒绝，然后 `enterPortal` | 可用（400000 ≈ 632px，客户端的碰撞矩形远小于此，没有副作用） |
| `pt=10` 同图传送 | 客户端发 `0x65` → `InnerPortalHandler` **空实现** | **缺口**：同图双向门（`ph` 图层那类）目前服务端什么也不做，玩家会被"卡"在原地等 `enableActions` |
| `pt=12/13` 弹床/跳台 | 纯客户端物理 | 无需服务端处理；但服务端若要复刻"跳台位移后的坐标校验"，需读 `verticalImpact/horizontalImpact` |
| 字段模板 | `MapPortalTemplate` 只有 `pn/pt/tm/tn/x/y/script/$name` | **缺** `hrange`、`vrange`、`hideTooltip`、`delay`、`onlyOnce`、`verticalImpact`、`horizontalImpact`、`image`、`reactorname`，服务端无法实现 pt 9 的节流/去重与 pt 13 的服务端侧判定 |
| 小地图 | 由客户端按 `pt==2 \|\| pt==7` 自行绘制 | 无需服务端参与 |

---

## 7. 客户端已验证 / 仍待确认

**已验证（反编译 + 反汇编双确认）**
- `pt` 全部取值 0–13、分派表位置 `0x711F89`–`0x712152`。
- 三条列表的归属：`+0x4` 全部非 6；`+0x8` = {3,9,12,13}；`+0xC` = {10,11}。
- 图层分派：`2/4/7 → pv(1711)`、`10 → ph(4386)`、`11 → psh(4387)`（`sub_712313/71259D/712827` 内嵌字符串确认）。
- `pt=6` 只调 `sub_52B922` 后丢弃；`CField+94` 被 `GetTownPortalPos`(0x5377FA)、`sub_5375ED` 使用。
- `pt=9` 的 `0x64` 编码顺序、`delay`/`onlyOnce` 语义、与 `ctx+8356/8360` 的共用节流。
- `pt=12` 的 ±200/±100/±2300 常量（`dbl_B3D330` 实读 = `-2300.0`）与 `pt=13` 的 fieldType 白名单 {19,20,21,25} ∪ {180000002}。
- `0x26` / `0x64` / `0x65` / `0x85` 的字段顺序（编码函数逐个对照）。
- 小地图只画 `pt==2 || pt==7`（`CUIMiniMap::Update` 中 `cmp eax,2 / cmp eax,7`）。

**未完全确认**
- `pt=13` 的 `reactorname` 判定：`sub_735D13` 返回的是命中项的某个字段，与 `1` 比较；具体是 reactor 的 *state* 还是别的标志位没有从 `CReactor` 结构上确认（v83 数据里 portal 节点普遍没有 `reactorname`，所以实际几乎总是走"空串直接生效"分支）。
- `pt=9` 的 `onlyOnce` 比较对象 `portal[0]` 是 PORTAL 的内部 id/引用计数（`RestorePortal` 开头写入），不是 WZ 里的 `$name`；"同一实例只触发一次"的粒度按地图重载而非按名字，这一点是按代码推的，没有运行时验证。
- `pt=3` 的 `hrange/vrange` 为 0 时仍能触发（判定矩形退化为点），说明客户端"走进 portal 即传送"还依赖玩家碰撞盒与 `dword_BF0484` 的点包含实现；这条没有进一步展开。

---

## 8. 本次改动

- 新增本文档 `docs/ida/portal_pt_类型_客户端行为_分析.md`，并在 `RuaMS.slnx` 的 `/docs/ida/` 列表中登记。
- **未修改任何 `src/` 代码**：本任务只做客户端行为分析。文中第 6 节列出的服务端缺口（`PortalConstants` 常量不全、
  `MapPortalTemplate` 字段缺失、`InnerPortalHandler` 空实现、`pt=6` 建模差异）留待后续单独处理。
