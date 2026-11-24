# RecvOpcode 未定义/未处理的数据包 — 客户端发包调用定位

> IDB：`D:\walker\code\`Angel\`Angel.idb`（IDA Pro 8.3，模块 `` `Angel.exe ``，ImageBase `0x400000`，v83，自动分析完成，Hex-Rays 可用）
> 服务端：`src/Application.Shared/Net/RecvOpcode.cs`、`src/Application.Core/Channel/Net/ChannelPacketProcessor.cs`、`src/Application.Core.Login/Net/LoginPacketProcessor.cs`、`src/modules/*/ServiceCollectionExtensions.cs`
> 相关文档：`docs/ida/UseItem.md`（`CDraggableItem::OnDoubleClicked` 的 itemId 分发，本次补充了 0x54/0x6F 两支）

---

## 1. 结论速览

`RecvOpcode` 之所以看起来"不连贯"，是因为它同时混入了三种来源的编号：

1. **客户端确实会发的 opcode**（本仓库已定义并在 Login/Channel/模块里注册了 Handler）；
2. **客户端确实会发包，但 `RecvOpcode` 里根本没有定义**（本文 §4，共 25 个，全部在客户端找到了发包函数与地址；其中 `0x86` 已于 2026-11-20 补定义并注册 Handler，见 §4.1、§7）；
3. **客户端也不会发的空洞**（本文 §5，共 68 个，扫遍客户端所有构造 `COutPacket` 的函数都没有命中，属于保留号/废弃号）。

另外发现两处枚举/映射问题（§6）：

- `WEDDING_TALK` 与 `WEDDING_TALK_MORE` **都是 `0x8B`**，而客户端实际发的是 `0x8B`(139) 与 `0x8C`(140) 两个不同 opcode；
- `USE_MAPLELIFE = 0x100` 是**错的**：客户端 `0x100` 是"角色名重复检查"（`CUICharacterSaleDlg::SendCheckDuplicateIDPacket`），真正的 Maple Life 请求不在此区间。

此外有 10 个**在 `RecvOpcode` 里定义了、但任何 PacketProcessor/模块都没有注册**的 opcode（§3），进入 Channel 时会走到 `ChannelClient.ProcessPacket` 的 `else` 分支抛出 `BusinessNotsupportException`。

---

## 2. 取号方法（客户端侧，可复现）

客户端所有 C→S 包都经 `COutPacket` 构造：

| 符号 | 地址 | 说明 |
|---|---|---|
| `??0COutPacket@@QAE@J@Z`（ctor） | `0x6ec9ce` | 绝大多数发包点直接调它，参数就是 opcode 字面量 |
| `?Init@COutPacket@@QAEXJ@Z` | `0x6ecaa9` | 仅 5 处 xref |
| `?Init@COutPacket@@QAEXJHH@Z` | `0x6ecacb` | 仅 3 处 xref（内部调用） |
| `?SendPacket@CClientSocket@@QAEXABVCOutPacket@@@Z` | `0x49637b` | 统一出口，411 个调用者 |

步骤：

1. 取 3 个 `COutPacket` 构造/Init 的 `idautils.XrefsTo`，按所属函数去重 → **417 个发包函数**；
2. 对每个函数 `ida_hexrays.decompile`，用正则 `COutPacket::(COutPacket|Init)\((\d+)\)` 取 Hex-Rays 输出的**十进制字面量**（注意：`COutPacket(84)` 是 opcode `0x54`）；
3. 去重后得到客户端会发的 opcode 全集（`<= 0x120` 范围内共 **201** 个）。

> 注意：`CClientSocket::ProcessPacket`（`0x4965f1`）里 `0x10/0x11/0x12/0x13/0x14/0x19` 被单独处理、`0x1D..0x7C` 转发给 `CWvsContext::OnPacket`，`< 0x1D` 或 `> 0x7C` 转发给 login 层（`CLogin::OnPacket` 在 `0x5f80ff`）。这是**收包**分发，与上面的发包枚举互补：收包分发里出现的 `0x11/0x12/0x13` 是 S→C 号，不是客户端发的。

---

## 3. `RecvOpcode` 已定义、但未注册 Handler（10 个）

把 `RecvOpcode.cs` 里的 185 个成员（184 个唯一值）与 Login/Channel/5 个模块的 `TryAddHandler` 全部 `RecvOpcode.XXX` 引用做差集：

| opcode | 枚举名 | 客户端发包调用（地址） | 说明 |
|---|---|---|---|
| `0x0C` | `PLAYER_DC` | `sub_5FAC34`（`0x5fac34`）、`CLogin::GotoWorldSelect`（`0x5fc36c`） | 客户端返回/回退到世界选择时发，裸包无 payload。服务端未注册 → **会抛 `BusinessNotsupportException`** |
| `0x19` | `CLIENT_START_ERROR` | `sub_49538D`（`0x49538d`，由 `CClientSocket::OnConnect` `0x495387` 调用） | payload：`Encode2(len)+EncodeBuffer`。属 login 阶段，Login 未注册（未注册时静默丢弃） |
| `0x1A` | `CLIENT_ERROR` | `CSecurityClient::OnCheckClientIntegrityRequest`（`0xa4bf22`） | payload：`Encode1(4)+Encode2(len)+EncodeBuffer`。同上 |
| `0x41` | `DUEY_ACTION` | `sub_6F0CA3`、`sub_6F0DC3`、`sub_6F1DF5`、`sub_6F36A8`、`sub_6F5691`（均在 `0x6f0c..0x6f57` 区）；`CUIFadeYesNo::OnButtonClicked`（`0x522585`）也发 `0x41` | 实际是**邮寄/商城相关 UI** 的 `Encode1(sub)` 子操作（4/5/2/7/0…）。`RecvOpcode` 名字叫 DUEY_ACTION 有误导性；本项目已有 `DueyHandler`（走 `0x3B` `REMOTE_STORE` 之外的独立流程）但没用这个号 |
| `0x44` | `ADMIN_SHOP` | `CAdminShopDlg::OnPacket`（`0x423db7`）、`sub_424BE8`、`CAdminShopDlg::SendTradeRequest`（`0x427a29`）、`CUIAdminShopWishList::SendRegisterPacket`（`0x7d0209`） | payload 首字节为子操作（0=查询、1=交易、2=关闭、3=登记愿望单） |
| `0x54` | `USE_TELEPORT_ROCK` | `sub_A0A3BB`（`0xa0a3bb`），由 `CDraggableItem::OnDoubleClicked`（`0x4efd25`）调用，条件 `itemId/10000 == 232` | payload：`Encode2(a2)`+`Encode4(itemId)`+`Encode4(tick)`。与注释 `//USE_OWL_ITEM=0x50` 无关；`UseItemHandler` 未覆盖 |
| `0xA4` | `ClientMobCrcKeyCheck` | `CMobPool::OnMobCrcKeyChanged`（`0x6797be`） | 客户端收到 `MOB_CRC_KEY_CHANGED` 后**回**一个同 opcode 的空包（`COutPacket(164)`，无 payload）。服务端可以不处理，但至少应注册空 Handler 避免报错 |
| `0xD6` | `MATCH_TABLE` | `CField::SendChatMsgSlash`（`0x52c40c`） | 竞猜/比赛表格 UI 的 `Encode1(sub)` 子操作。客户端路径存在但项目未实现该玩法 |
| `0xED` | `CLOSE_ITEMUI` | `sub_890CC6`（`0x890cc6`，构造时）、`sub_890F4F`（`0x890f4f`，析构时） | payload：`Encode4(itemId)`+`Encode1(0/1)`。这是 RAISE（`0xEC` OPEN_ITEMUI / `0xEE` USE_ITEMUI）功能的**关闭/放弃**包，当前 `RaiseUIStateHandler` 和 `RaiseIncExpHandler` 只处理开/用，没处理关 |
| `0xFFFE` | `MAPLETV` | **未在客户端找到**（全 `.text` 扫描 `0x3713/0x1000/0x1001/0xFFFE` 均未命中 UDP 号；`0xFFFE` 只出现在普通 `mov` 常量里） | 枚举注释已写 `//Don't know`。建议保留不注册 |

> 补充：`0x0C` 在 **Login** 阶段也会来（`CLogin::GotoWorldSelect`），但 `LoginPacketProcessor` 里没有该号——Login 侧未注册是"静默忽略"，Channel 侧未注册是"抛异常"。若只想消掉 Channel 的报错，注册一个空 Handler 即可。

---

## 4. `RecvOpcode` 未定义、但客户端确实会发的包（25 个，其中 `0x86`、`0xF2` 已补定义）

这一组是真正的"漏定义"。全部由上文 §2 的扫描得到；"调用"一列是发送函数，括号内为地址。

| opcode | 客户端发包函数（地址） | payload（按 Encode 顺序） | 语义（客户端侧证据） |
|---|---|---|---|
| `0x0F` | `CLogin::MakeVACDlg`（`0x5fb0e1`）、`CLogin::ResetVAC`（`0x5fb155`） | `Encode1(1/0)` | 登录界面 VAC（多角色预览）开关。由 `CLogin::Update`、`OnViewAllCharResult` 驱动 |
| `0x23` | `CLogin::Init`（`0x5f42cd`） | 裸包 | 登录流程初始化后发（另一处是 `0x04`）。**Login 未注册 → 静默丢弃**，若将来需要客户端上报启动状态，此处是入口 |
| `0x4A` | `CWvsContext::TryRecovery`（`0xa02e34`） | 裸包 | 客户端断线/卡死后的"恢复请求"。服务端未处理 → 无响应 |
| `0x50` | `CWvsContext::SendActivatePetRequest`（`0xa240a2`） | `Encode4(tick)`+`EncodeBuffer(8字节)` | 同一函数另一个分支发 `0x62`（`SPAWN_PET` 已有）。`0x50` 与 `UseItem.md` 里的 `0x50` 是同一处（宠物激活） |
| `0x53` | `CWvsContext::SendShopScannerItemUseRequest`（`0xa0a25e`） | `Encode2(slot)`+`Encode4(itemId)` | 商店扫描器道具使用 |
| `0x6F` | `sub_A10075`（`0xa10075`） | `Encode2(x)`+`Encode4(itemId)` | 由 `CDraggableItem::OnDoubleClicked` 调用，条件 `itemId/10000 ∈ {545, 239}`（与 `0x54` 分支并列）。同类分支还有 `itemId/10000==224 → sub_A0FC6C` 与 `==237` 两支，未在本表展开 |
| `0x72` | `CField::SendChatMsgSlash`（`0x52c40c`） | `Encode4(targetId)`+`Encode1(sub)`+`EncodeStr(msg)` | 聊天格式宏。等号另一支还发 `0x76`（ADMIN_CHAT）、`0x86`、`0xD6` 以及 `0x80/0x81/0x31` 的管理员/斜杠命令变体 |
| `0x73` | `sub_A12277`（`0xa12277`），由 `sub_A1212F`（`0xa1212f`）调用 | `Encode2(a1)`+`Encode4(a2)` | 组队搜索相关（`sub_A1212F` 遍历现金背包 `tab 5` 取道具）；邻接函数 `StartSearch/StopPartySearch/CloseShopScanner` |
| `0x86` | `CField::SendChatMsgSlash`（`0x52c40c`），指令 `push 86h` 在 `0x52e25f` | 仅 `Encode1(0)`（`0x52e274`），**无字符串** | `/slideax` 专用握手包，见下方 §4.1。**已修复**（`RecvOpcode.ADMIN_SLIDE` + `AdminSlideHandler`） |
| `0x8C` | `CField_Wedding::OnWeddingProgress`（`0x58153d`） | 裸包 | **婚礼进程确认之一**。同一函数还发 `0x8B`(139) 带 `Encode1`。即客户端 `0x8B`/`0x8C` 是一对请求/应答，而非同一个号 |
| `0x8D` | `CWvsContext::SendBoobyTrapAlert`（`0xa328c2`，发 `COutPacket(141)`），由 `CDropPool::BoobyTrapCheckPickupItem`（`0x5081b0`）调用 3 次 | `Encode4(alert)`，`alert ∈ {1, 2}` | 不是"陷阱道具"，是**拾取框自律校验**，见下方 §4.2 |
| `0x8E` | `sub_85BAFC`（`0x85bafc`），installed 于 vtable `0xb39ce4`（同 vtable 邻位是 `CUIMiniMap::OnMouseMove` 等），`param a4 == 514` | 裸包 | 小地图（`CUIMiniMap` 体系）上的点击/快捷操作。`CUIMiniMap` 类名来自邻位符号，未逐一确认 |
| `0xA0` | `sub_A5337F`（`0xa5337f`），由 `sub_A532AC`(`0xa532ac`，vtable `0xb40800`) / `sub_A532F2`(`0xa532f2`, `CDialog::OnKey` 回车分支) 调用 | `Encode4(tick)`+`Encode2(x)`+`Encode4(y)`+`EncodeStr(text)` | 一个 `CCtrlEdit` 输入框 + 回车提交的对话框（含 `CanSendExclRequest(500)` 与 `SP_297 请求处理中` 提示）。类型名未定位（vtable 家族 `0xb39430/0xb394bc` 未带符号） |
| `0xA5` | `CUISkillEx::OnButtonClicked`（`0x8bb2bf`，按钮 2024）、`CDragon::NeedToTalk`（`0x4ffb8d`） | 裸包 | 龙骑（`CDragon`）对话/技能拓展 UI 点击。`CDragon::NeedToTalk` 在鼠标命中小龙后发 |
| `0xBE` | `sub_66E91F`（`0x66e91f`），由 `sub_504A63`(`0x504a63`，`CDropPool` 家族) 调用 | `Encode4(secureId)`+`Encode4(a2)` | `CMob` 侧：同一 mob 3 秒内不重复上报；`this+95/97` 走 `_ZtlSecureFuse`。语义与"怪物被点击/交互"相关 |
| `0xC6` | `sub_6D40FB`（`0x6d40fb`），由 `CUser::OnChat`（`0x937138`）调用 | `Encode4(npcId)`+`EncodeStr(text)` | NPC 对话文本上报（`CNpc` 家族） |
| `0xD7` | `CField_GuildBoss::BasicActionAttack`（`0x558b45`） | 裸包 | 公会 Boss 战场：普通攻击命中后的额外上报（`if (!this[507])`） |
| `0xEF` | `sub_892B4B`（`0x892b4b`），installed 于 vtable `0xb3af28`（`CWnd` 派生窗口） | `Encode2(a2)`+`Encode4(a1)`+`Encode4(a3)`+`Encode4(1<<v4)` | 某窗口的"选择/启用"动作（`sub_891481` 取索引）。窗口类名未定位 |
| `0xF2` | `sub_92123B`（`0x92123b`），由 `sub_9206C9`(`0x9206c9`，窗口构造) / `sub_920C49`(`0x920c49`，窗口 Update) 调用 | 裸包（opcode 后无任何 `Encode*`） | **公会公告板（GuildBoard）内嵌网页窗口的 3 分钟保活心跳**，窗口资源 `UI/UIWindow.img/UserList/Guild/GuildBoard/*`。旧版本此处写的"属队伍/HP 面板体系"是**错的**，详见 `docs/ida/GuildBoard网页窗口_0xF2分析.md`。**已补定义**（`RecvOpcode.GUILD_BOARD_PING` + `LoginRequiringNoOpHandler`） |
| `0xF4` | `sub_4929E5`（`0x4929e5`），由 `sub_492438`(`0x492438`) / `sub_492A3A`(`0x492a3a`) 调用 | 裸包 | 与 `0xF2` 同构（`dword_BF060C` 取时间写 `this[402]`） |
| `0xF5` | `sub_92A793`（`0x92a793`），由 `sub_929FE1`(`0x929fe1`) / `sub_92A577`(`0x92a577`) 调用 | `Encode1(0)` | 与 `0xF4` 同构的第三个分支 |
| `0xF7` | `sub_537E40`（`0x537e40`） | `Encode1(flags)`+`Encode1(a2)`+`Encode1(a3)`+`Encode1(a1)`+`Encode4(tick)`+`EncodeStr`×6 | 位置在 `CField::ApplyUserLook`/`UpdateQuestLayer` 邻域，属 `CField` 家族。无调用者（vtable 未落在已命名槽） |
| `0xF8` | `CWvsContext::Update`（`0xa03350`） | `EncodeStr(name)`+`Encode4(tick)`+`Encode4(x)` | 见 `SP_3543 您已游戏 D 小时` / `SP_889 [MapleTip]` —— **防沉迷/在线时长上报** |
| `0xFB` | `sub_59EBDA`（`0x59ebda`），由 `sub_5B8688`（`0x5b8688`，`a2==1000`）调用 | 裸包 | `CITC`（物品交易中心/MTS）家族，`this[6]` 做了"只发一次"标记 |
| `0xFC` | `sub_59EECE`（`0x59eece`），由 `sub_59EC2C`（`0x59ec2c`，`a2==1001`）调用 | 裸包 | 与 `0xFB` 同族；`this[6]` 已置位则返回 0 |

> §4 的"语义"列只写**客户端能证明**的部分（调用了哪些 API、有哪条 `SP_xxx` 字符串、被谁调用）。窗口类名未能从 RTTI/符号定位的一律标"未定位"，不做猜测。

## 5. 客户端也不发的空洞（68 个）

在 `0x00..0x110` 区间内，`RecvOpcode` 未定义、且**任何构造 `COutPacket` 的函数都没有使用**的 opcode：

```
0x03 0x11 0x21 0x22 0x24 0x25 0x36 0x37 0x60 0x84 0x9A 0xA6 0xAD 0xAE
0xB3 0xB4 0xB6 0xB8 0xB9 0xBA 0xBB 0xC3 0xC4 0xC7 0xC8 0xC9 0xCB 0xCC
0xD0 0xD1 0xD2 0xD8 0xD9 0xDB 0xDD 0xE0 0xE1 0xE2 0xE3 0xE7 0xE8 0xE9
0xEA 0xEB 0xF0 0xF1 0xF3 0xF6 0xF9 0xFA 0xFE 0xFF 0x101 0x102 0x103
0x105 0x106 0x107 0x108 0x109 0x10A 0x10B 0x10C 0x10D 0x10E 0x10F 0x110
```

这些不需要补定义。判定依据：ctor 的 490 个 xref 覆盖了全部发包点，字面量扫描确认无命中（`0x3713/0x1000/0x1001` 这三个"自定义号"同样无命中，属于本仓库自加，不是客户端协议）。

---

## 6. 枚举/映射错误（建议修正）

1. **`WEDDING_TALK` 与 `WEDDING_TALK_MORE` 同为 `0x8B`**
   `RecvOpcode.cs:162-163`。客户端实际：
   - `0x8B`(139) → `CField_Wedding::OnWeddingProgress` 内 `COutPacket(139)` + `Encode1(x)`（`0x58153d`）；
   - `0x8C`(140) → 同函数 `COutPacket(140)` 裸包。
   现在 `WEDDING_TALK_MORE` 被注释掉（`Marriage.Channel/ServiceCollectionExtensions.cs:58`），而 `WEDDING_TALK` 只注册了一个 Handler，所以 `0x8C` 在 Channel 侧必然抛 `BusinessNotsupportException`。应改为 `WEDDING_TALK = 0x8B`、`WEDDING_TALK_MORE = 0x8C`，并把 `WeddingTalkMoreHandler` 打开。

2. **`USE_MAPLELIFE = 0x100` 与客户端不符**
   客户端 `0x100`(256) 的发包函数是 `CUICharacterSaleDlg::SendCheckDuplicateIDPacket`（`0x7d75ab`），payload `EncodeStr(name)`，随后 `sub_7D766A` 发送——即**角色名重复检查**。真正的 Maple Life 请求不在此区间（`0x101..0x110` 全空，见 §5）。
   服务端 `UseMapleLifeHandler` 读 `readString()`，恰好与"查重名"的 payload 对得上，但它回的是 `MAPLELIFE_RESULT/MAPLELIFE_ERROR`(0x15D/0x15E)，语义对不上。要么把这个号改成 `CHECK_CHAR_NAME_SALE` 之类并用 `CHAR_NAME_RESPONSE` 回应，要么保留但注明该号实际来自角色出售 UI。

3. **`0x50` 注释**
   `RecvOpcode.cs:99` 的 `//USE_OWL_ITEM=0x50, ... no idea` 已被客户端证伪：`0x50` 是 `CWvsContext::SendActivatePetRequest`（宠物激活，`0xa240a2`）。

---