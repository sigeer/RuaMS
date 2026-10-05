# CDraggableItem::OnDoubleClicked — itemId 去向分析

> 函数：`?OnDoubleClicked@CDraggableItem@@UAEHXZ`，地址 `0x4efd25`，大小 `0x1020`

## 1. 概述

双击背包物品时触发，根据 `this` 对象状态与物品 ID 分发到不同处理函数（发 OP 包 / 打开 UI / 调用逻辑）。

`this` 布局：

- `this[6]` = 背包 tab：1=装备(Equip)，2=消耗(Use)，3=设置(Setup)，4=其他(Etc)，5=现金(Cash)
- `this[7]` = 槽位索引；`this[8]` = 槽对象
- 物品 ID 从槽对象 `+12` 偏移读取（`TSecType<long>::GetData`）

通用前置检查：

- `CanPerformAction(0)`
- `CanSendExclRequest(this, 200|500, 0)`
- `IsAbleToConsume`（`0xa10579`）
- `sub_4F1176`（`0x4f1176`）检查目标背包是否有空槽
- 发包后置：`this+2089 = 1`，`this+2090 = get_update_time()`
- 发包统一走 `CClientSocket::SendPacket(dword_BE7914, ...)`
- `dword_BE7910` 非 0 时通常提示 "Not available"
- `dword_BE7918` = `CWvsContext*`（`this = *dword_BE7918`）
- `TSingleton<CUserLocal>::ms_pInstance` = `0xbebf98`

## 2. 窗口路由

`this[9]` 为当前 UI 窗口，通过 vtable+0x48 的 `IsKindOf` 式虚调用与窗口"类型 ID"指针比对：

- `dword_BF0EB0` —— 访问器 `sub_81C554`（`0x81c554`）返回 `&dword_BF0EB0`（由 `sub_81C2C6` 赋值为 `&dword_BF11DC`）
- `dword_BF0E48` —— 由 `sub_7FDE71` 赋值，同为 `&dword_BF11DC`
- `dword_BF0E4C` —— 由 `sub_7FE348` 赋值

| 匹配项 | 行为 |
|---|---|
| `BF0EB0` | 走主分发体（下方第 3 节） |
| `BF0E48` / `BF0E4C` | 播放 DragEnd 音效 → `sub_4F0E97`（`0x4f0e97`）：卸下装备逻辑，要求 `this[6]==1`，`FindEmptySlotPosition` 后把已穿戴装备移回背包 |

> 注：三个全局对应的具体窗口类名（主背包 UI vs 其他 UI）未在本会话中确认。

特殊用例：`this[7] == -18` 且物品为 `1902040 / 1902041 / 1902042` → 直接 `return 0`。

## 3. 主分发（按 tab）

### Tab 1 — 装备 (Equip)

- 任意装备：`get_bodypart_from_item`（`0x4606a0`）映射物品 → bodypart；180/181/182/183 宠物装备走多宠物选择提示 `CUtilDlgEx`（"Please select the pet you would like to put the pet equipment on"）
- `181xxxx` → `sub_4F0D83`（`0x4f0d83`）按宠物索引映射 bodypart（1812000–1812007）
- 穿戴：`WearEquipItem`（`0x4f1c2d` / `0x4f1a93`）；卸下：`GetOffEquipItem`（`0x4f11b4`）

### Tab 2 — 消耗 (Use)

| 判定（itemId） | 处理函数 | 去向 |
|---|---|---|
| 封印物品检查 | — | SP_3416 提示 |
| `sub_4F2ED6`（`0x4f2ed6`，哈希查询 `sub_4F58F4` `0x4f58f4`）+ `CanSendExclRequest(200)` | `sub_A1249F` `0xa1249f` | **OP 0x70 (112)** |
| `/10000==221 && (%10000)/1000==2`（即 2212xxx） | `sub_A1005A` `0xa1005a` → `sub_A53338` `0xa53338` → `sub_A52F31` `0xa52f31` | 打开 `CUniqueModeless` 模态/非模态弹窗 |
| `/10000 ∈ {200,201,202,205,221,236,238,245}` | `SendStatChangeItemUseRequest` `0xa092fb` | **OP 0x48 (72)** 属性变化/药水；属地/骑乘限制提示（SP_4077 / SP_3815） |
| `/10000==219` | `SendAntiMacroItemUseRequest` `0xa262d9` | **OP 0x67 (103)** 反宏/测谎 |
| `/10000==203` | `sub_A1E1CE` `0xa1e1ce` | **OP 0x55 (85)**；读物品属性 SPEC/maxLevel/map（SP_2306/2307/2309），`CanSendExclRequest(500)` |
| `/10000==210` | `SendMobSummonItemUseRequest` `0xa0977b` | **OP 0x4B (75)** 召唤怪物 |
| `/10000==524` 且有消耗现金类型 | `SendConsumeCashItemUseRequest` `0xa0a63f` | **OP 0x4F (79)** + `Encode2(数量)` + `Encode4(itemId)` + 类型 switch（58 分支，`0xa0a6e6`） |
| `/10000==212` | `SendPetFoodItemUseRequest` `0xa09905` | **OP 0x4C (76)** 宠物食物 |
| `/10000==224` | `sub_A0FC6C` `0xa0fc6c` | 结婚/求婚（SP_4255 等），最终 **OP 0x89 (137)** |
| `/10000==226`（需 `+1348/10000==190` 骑乘状态） | `sub_A09A64` `0xa09a64` | **OP 0x4D (77)** |
| `/10000==227` | `sub_A09BDF` `0xa09bdf` | **OP 0x51 (81)** 区域内捕怪（`FindHitMobInRect`，SP_4351/3828：太远/无驯服目标） |
| `/10000 ∈ {228,229}` | `sub_A0A1B2` `0xa0a1b2` | **OP 0x52 (82)** |
| `/10000==231` | `SendShopScannerItemUseRequest` `0xa0a25e` | **OP 0x53 (83)** + `RunShopScanner` |
| `/10000==232` | `sub_A0A3BB` `0xa0a3bb` + 校验 `sub_A0A4AA` `0xa0a4aa` | **OP 0x54 (84)** 传送类（死亡 SP_397 / 地图限制 SP_2950，`sub_8390D1` 地图选择） |
| `/10000 ∈ {239,545}` | `sub_A10075` `0xa10075` | **OP 0x6F (111)** 地图 bit18 限制（SP_276） |
| `/10000==237` | `SendExpUpItemUseRequest` `0xa12685` | 经验类（等级 SP_3177、Solomon SP_4860）；**OP 0x9E (158)** |
| `/10000==243` | `sub_A09B26` `0xa09b26` | **OP 0x4E (78)**；`CanSendExclRequest(500)` + `IsAbleToConsume` |
| `/10000==216` | `sub_A0FE87` `0xa0fe87` → `sub_A515FD` `0xa515fd` → `sub_A50E03` `0xa50e03` | 检查 Etc 空槽（`sub_4F1176(4)`）后打开弹窗（518x188） |

### Tab 3 — 设置 (Setup)

| 判定（itemId） | 处理函数 | 去向 |
|---|---|---|
| `/10000==301` | `SendSitOnPortableChairRequest` `0xa0f9e2` | **OP 0x2B (43)** 便携椅子；检查骑乘/变身/等级需求（SP_4269/3918/3867） |

### Tab 4 — 其他 (Etc)

| 判定（itemId） | 处理函数 | 去向 |
|---|---|---|
| `/10000==408` | `SendCreateMiniGameRequest` `0xa1d4d8` | **OP 0x7B (123)** 创建小游戏（omok 等） |
| `/10000==416` | `OpenBook` `0xa10617` | `CBookDlg::SetBookItem` |
| `∈ {4031377, 4031395}` | `sub_A10199` `0xa10199` | 婚礼请柬：`CUtilDlgEx` "Please enter the guest name" → **OP 0x89 (137)** 类型 5 |
| `∈ {4031406, 4031407}` | `sub_A103E7` `0xa103e7` | **OP 0x89 (137)** + `Encode1(6)` |
| `/1000 == 4220`（4220xxx） | `sub_A1211D` `0xa1211d` → `sub_892FBD` `0x892fbd` | RAISE 奖励弹窗（`UI/UIWindow.img/Raise/backgrnd[top/center/bottom]`，SP_4359/4360/4361；读 questId/uiData/exp/grade/name，发 **OP 0xED (237)** 请求） |
| `/10000==428` | `sub_A1212F` `0xa1212f` | 宝箱弹窗（`UI/UIWindow.img/TreasureBox/backgrnd[Open/Closed]`，SP_4856/4857/4858，`sub_998F7A` `0x998f7a`）；查现金槽 `TSecType::GetData == (&loc_53C54C+4)`，确认后 `sub_A12277` `0xa12277` → **OP 0x73 (115)** |
| `/10000==417`（现金栏找消耗现金类型 27 = 孵化器） | 与 243 同族逻辑 | **OP 0x4F (79)** 飞天猪的蛋 + `sub_81A7B9` 弹窗(`UI/UIWindow.img/Incubator/backgrnd`) |
| `/10000==429` | `SendActiveEffectItemChange` `0xa24530` | **OP 0x34 (52)** 切换特效 |
| `/10000==430` | `ShowNewYearCard` `0xa0fef8` | 新年卡片弹窗（读物品描述字符串） |

### Tab 5 — 现金 (Cash)

现金类型判定：`get_cashslot_item_type`（`0x48645b`，按 `itemId / 10000` 分类，返回内部类型号，非现金段返回 0）：

| itemId/10000 | 类型号 | 说明 |
|---|---|---|
| 500 | 8 | 宠物激活 |
| 501 | 9 | 武器特效 |
| 502 | 10 | - |
| 503 | 11 | 托管商店 |
| 504 | 22 | — |
| 505 | 23 / 24 | `%10==0`→23，`%10∈1..4`→24 |
| 506 | 25 / 26 / 27 / 64 | `/1000==5061`→64；`%10==0`→25，`==1`→26，`==2`→27 |
| 507 | 12 / 13 / 14 / 15 / 46..51 / 60 | 千位 1→12，2→13，6→14，8→15，7→60；千位 5 时 `%10` 0..5 → 46..51 |
| 508 | 18 | — |
| 509 | 21 | — |
| 510 | 20 | — |
| 512 | 16 | — |
| 513 | 7 | — |
| 514 | 4 | — |
| 515 | 1 / 2 / 3 / 5 | 5150/5151→1，51520→2，5153→3，5154→5 |
| 516 | 6 | — |
| 517 | 17 | — |
| 518 | 5 | — |
| 519 | 28 | — |
| 520 | 19 | — |
| 522 | 39 | — |
| 523 | 29 | — |
| 524 | 30 | — |
| 525 | 36 / 35 | — |
| 528 | 33 / 34 | /1000==5280→33，5281→34 |
| 530 | 40 | — |
| 533 | 31 | — |
| 537 | 32 | — |
| 538 | 41 | — |
| 539 | 42 | — |
| 540 | 52 / 53 / 54 / 57 / 65 | /1000==5400→52，5401→53，5420→54，5431..5432→65，其余→57 |
| 542 | 54 / 57 | 同 540 的 5420 分支 |
| 543 | 57 / 65 | 同 540 的 5431..5432 分支 |
| 545 | 37 / 59 | /1000==5451→59，否则 37 |
| 546 | 57 | — |
| 547 | 38 | — |
| 549 | 58 | — |
| 550 | 61 | — |
| 551 | 62 | — |
| 552 | 63 | — |
| 553 | 69 | — |
| 557 | 66 | — |
| 561 | 68 | — |

另两个分类函数：
- `get_consume_cash_item_type`（`0x4863d5`）— 消耗型现金道具 → `SendConsumeCashItemUseRequest` `0xa0a63f` → OP 0x4F
- `get_etc_cash_item_type`（`0x486845`）— 其他现金道具 → `SendEtcCashItemUseRequest` `0xa1dc5b`

| 判定 | 处理函数 | 去向 |
|---|---|---|
| cashslot 类型 8 且不可交易 | 先 `CUtilDlgEx` SP_5213 二次确认 | → `SendCashSlotItemUseRequest` `0xa1dd28` |
| cashslot 类型 8 | `SendActivatePetRequest` `0xa240a2` |  **OP 0x62 (98)** 激活宠物， **OP 0x50 (80)** 销毁无法复活的过期宠物 |
| cashslot 类型 9 | `SendActiveEffectItemChange` `0xa24530` | **OP 0x34 (52)** 武器特效 |
| cashslot 类型 11 | `SendEntrustedShopCheckRequest` `0xa27b7d` | **OP 0x3B (59)** 远程打开托管商店 |
| cashslot 类型 38 | `SendRemoteShopOpenRequest` `0xa24606` | **OP 0x3B (59)** |
| 消耗现金类型 | `SendConsumeCashItemUseRequest` `0xa0a63f` | **OP 0x4F (79)** |
| 其他现金类型 | `SendEtcCashItemUseRequest` `0xa1dc5b` | 见下 |
| — etc 类型 4 | `SendOpenShopRequest` | 打开商店 |
| — etc 类型 5 | `SendWaterOfLife` | **OP 0x75 (117)** 生命之水 |
| — etc 类型 6 | `SendEmotionChange(id%100+8)` | 表情 |
| — etc 类型 57 | `CUIGachaponRemote` `0x81104e` | 远程扭蛋 |

## 4. OP 码速查

| OP（十/十六） | 用途 | 对应 RecvOpcode | 对应 Handler |
|---|---|---|---|
| 43 / 0x2B | 便携椅子 | `USE_CHAIR` | `UseChairHandler` |
| 52 / 0x34 | 切换特效 | `USE_ITEMEFFECT` | `UseItemEffectHandler` |
| 59 / 0x3B | 远程商店 | `REMOTE_STORE` | `RemoteStoreHandler` |
| 72 / 0x48 | 属性变化（药水） | `USE_ITEM` | `UseItemHandler` |
| 75 / 0x4B | 召唤怪 | `USE_SUMMON_BAG` | `UseSummonBagHandler` |
| 76 / 0x4C | 宠物食物 | `PET_FOOD` | `PetFoodHandler` |
| 77 / 0x4D | 骑乘用道具 (226) | `USE_MOUNT_FOOD` | `UseMountFoodHandler` |
| 78 / 0x4E | (243) | `SCRIPTED_ITEM` | `ScriptedItemHandler` |
| 79 / 0x4F | 消耗类现金/孵蛋 (524/417) | `USE_CASH_ITEM` | `UseCashItemHandler` |
| 80 / 0x50 | 销毁无法复活的过期宠物，当前系统已经自动移除 | - | - |
| 81 / 0x51 | 捕怪 (227) | `USE_CATCH_ITEM` | `UseCatchItemHandler` |
| 82 / 0x52 | (228/229) | `USE_SKILL_BOOK` | `SkillBookHandler` |
| 83 / 0x53 | 商店扫描 (231) | — | — |
| 84 / 0x54 | 传送 (232) | `USE_TELEPORT_ROCK` | — |
| 85 / 0x55 | 地图/等级限制用 (203) | `USE_RETURN_SCROLL` | `UseItemHandler` |
| 98 / 0x62 | 召唤宠物 | `SPAWN_PET` | `SpawnPetHandler` |
| 103 / 0x67 | 反宏 (219) | `AntiMacroItemUseRequest` | `AntiMacroItemUseRequestHandler` |
| 111 / 0x6F | 地图限制用 (239/545) | — | — |
| 112 / 0x70 | `sub_A1249F` 映射使用 | `USE_ITEM_REWARD` | `ItemRewardHandler` |
| 115 / 0x73 | (428) | — | — |
| 117 / 0x75 | 生命之水 | WATER_OF_LIFE | UseWaterOfLifeHandler |
| 123 / 0x7B | 创建小游戏 (408) | `PLAYER_INTERACTION` | `PlayerInteractionHandler` |
| 137 / 0x89 | 结婚/邀请/婚戒 (224/4031377/4031395/4031406/4031407) | `RING_ACTION` | `RingActionHandler`（Marriage 模块） |

## 5. 备注 / 未确定项

- `dword_BF0EB0` 等三个窗口类型 ID 对应的具体窗口类名未确认（`sub_81C554` 仅为访问器）。
- 2212xxx、216、428 等物品段的现实名称未在代码中定名，表中按行为描述。
- `dword_BE7910`（"Not available" 判定）与 `dword_BE7914`（CClientSocket）的精确语义未经完整验证。

## 6. 关键地址一览

| 地址 | 名称 |
|---|---|
| `0x4efd25` | `CDraggableItem::OnDoubleClicked` |
| `0x4ef140` | `OnDropped` |
| `0x4f0e97` | `sub_4F0E97` 卸装备 |
| `0x4f1c2d` / `0x4f1a93` / `0x4f11b4` | Wear / GetOffEquipItem |
| `0x4606a0` | `get_bodypart_from_item` |
| `0x4f0d83` | 宠物装备 bodypart 映射 |
| `0x4f1176` | 查空槽 |
| `0x4f58f4` | 哈希查询 |
| `0x48645b` / `0x4863d5` / `0x486845` | `get_cashslot_item_type` / `get_consume_cash_item_type` / `get_etc_cash_item_type` |
| `0x485bf7` | `CanSendExclRequest` |
| `0x437a0c` | `get_field` |
| `0x81c554` | 窗口类型 ID 访问器 |
| `0xbe7914` | CClientSocket 单例 |
| `0xbe7918` | CWvsContext 单例 |
| `0xbebf98` | TSingleton\<CUserLocal\>::ms_pInstance |
