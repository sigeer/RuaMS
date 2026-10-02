# CWvsContext::OnTemporaryStatSet 数据包接收分析

> IDA 数据库: Angel.idb (base 0x400000)

---

## 1. 函数概览

| 属性 | 值 |
|------|-----|
| 函数名 | CWvsContext::OnTemporaryStatSet |
| 地址 | 0xA202BE |
| 大小 | 0x461 (1121) 字节 |
| 签名 | void __thiscall CWvsContext::OnTemporaryStatSet(CWvsContext* this, CInPacket* iPacket) |
| 分发 opcode | **32** (在 CWvsContext::OnPacket 的 switch-case 中) |
| 调用链 | CWvsContext::OnPacket(nType=32) -> OnTemporaryStatSet |

---

## 2. 数据包总体结构

```
+---------------------------------------------------+
| UINT128 mask (16 bytes)                           |  DecodeBuffer(&mask, 0x10)
|   低64位 = isFirst=true 的buff掩码                 |
|   高64位 = isFirst=false 的buff掩码                |
+---------------------------------------------------+
| 【模式A: 位 0~81, 标准buff】                       |
| 对于mask中每个置位的bit:                            |
|   int16   nValue        (Decode2)                 |  buff 数值
|   int32   nSkillID      (Decode4)                 |  技能/来源ID
|   int32   nTimeOffset   (Decode4)                 |  相对时间偏移(ms)
|   小计: 7 字节/位                                  |
+---------------------------------------------------+
| 【无条件读取】                                     |
|   1字节  flag1         (Decode1)                   |  -> SecondaryStat+0x90C
|   1字节  flag2         (Decode1)                   |  -> SecondaryStat+0x938
+---------------------------------------------------+
| 【模式B: 位 82~88, 扩展buff (通过虚表调用)】        |
| 对于mask中每个置位的bit:                            |
|   vtable[6](object, packet) 读取:                  |
|     4字节 nID          (DecodeBuffer)              |  buff ID
|     4字节 nValue       (DecodeBuffer)              |  buff 数值
|     1字节 sign         (Decode1)                   |  时间方向标志
|     4字节 timeDelta    (Decode4)                   |  时间差值
|     [可选] 额外字段 (类型相关)                      |
|   小计: 13~20 字节/位 (取决于buff类型)             |
+---------------------------------------------------+
| int16   tDelay          (Decode2)                 |  全局延迟时间 (在OnTemporaryStatSet中)
+---------------------------------------------------+
| [条件] uint8  nPoint    (Decode1)                 |  仅当入参掩码命中 sub_77DC78 的12位之一时 (见 §5.4)，与 tDelay 无关
+---------------------------------------------------+


## 3. UINT128 掩码编码机制

### 3.1 服务端编码 (WriteLongMask)

```csharp
// BuffPackets.cs
private static void WriteLongMask(OutPacket p, List<BuffStatValue> statups)
{
    long firstmask = 0;   // isFirst=true 的buff
    long secondmask = 0;  // isFirst=false 的buff
    foreach (var statup in statups)
    {
        if (statup.BuffState.IsFirst)
            firstmask |= statup.BuffState.getValue();
        else
            secondmask |= statup.BuffState.getValue();
    }
    p.writeLong(firstmask);   // 前8字节
    p.writeLong(secondmask);  // 后8字节
}
```

### 3.2 客户端解码

客户端通过 DecodeBuffer(&uFlagTemp, 0x10) 一次性读取16字节到 UINT128。

x86 小端序下：
- bytes 0-7 -> UINT128 低64位 = firstmask (isFirst=true)
- bytes 8-15 -> UINT128 高64位 = secondmask (isFirst=false)

因此：
- UINT128 bit 0~63 对应 firstmask (isFirst=true 组)
- UINT128 bit 64~127 对应 secondmask (isFirst=false 组)

### 3.3 掩码常量生成机制

sub_873B8C(bitIndex) 创建 UINT128，仅设置第 bitIndex 位。

对于 bit >= 64 的情况，使用 sub_78D977(output, n)，内部调用 sub_873B8C(n + 82)，即实际设置 bit n+82。

---

## 4. SecondaryStat 结构体布局

每个 buff 占 12 DWORD (48字节)，按 UINT128 bit 序号顺序排列。

### 4.1 每个 buff 组内部结构 (48字节)

| 偏移(DWORD) | 偏移(字节) | 内容 | 说明 |
|-------------|-----------|------|------|
| +0 | +0x00 | nValue | buff 数值 |
| +1 | +0x04 | nValue_CS | 安全校验 |
| +2 | +0x08 | (保留) | |
| +3 | +0x0C | nSkillID | 技能/来源ID |
| +4 | +0x10 | nSkillID_CS | 安全校验 |
| +5 | +0x14 | (保留) | |
| +6 | +0x18 | nTime | 绝对时间戳 |
| +7 | +0x1C | nTime_CS | 安全校验 |
| +8 | +0x20 | (保留) | |
| +9~+11 | +0x24~+0x2C | (其他字段) | |

### 4.2 DecodeForLocal 读取模式

sub_781D0E (等价于 SecondaryStat::DecodeForLocal) 包含两种解码模式：

#### 模式A: 标准 buff (位 0~81, 82个条件块)

对每个置位的 bit (0~81)：

```
if (mask & CTS_bitmask) {
    value    = Decode2(iPacket);     // int16 buff值
    skillID  = Decode4(iPacket);     // uint32 技能ID
    timeOff  = Decode4(iPacket);     // uint32 时间偏移
    // 存入 SecondaryStat + 创建 VIEWELEM
    VIEWELEM.nTime = timeGetTime() + timeOff;
    VIEWELEM.bitmap |= CTS_bitmask;
}
```

#### 模式B: 扩展 buff (位 82~88, 通过虚表调用)

对每个置位的 bit (82~88)，使用 `esi+0xCA0` 处的对象数组（7个8字节条目）：

```
for (i = 0; i < 7; i++) {
    maskBit = sub_78D977(&temp, i);  // 创建 bit(i+82) 的 UINT128 掩码
    if (packetMask & maskBit) {
        object = *(esi + 0xCA0 + i*8 + 4);  // 获取对象指针
        object->vtable[6](object, packet);   // 从包中读取数据 (类型相关)
        value = *(object + 16);              // 获取存储的值
        timeout = object->vtable[4](object); // 获取超时时间(毫秒)
        // 创建 VIEWELEM 并插入 mElem 映射
    }
}
```

**核心解码函数 sub_793EF2**（所有虚表函数共用）：
```
DecodeBuffer(packet, object+12, 4)   // 读4字节 → nID
DecodeBuffer(packet, object+16, 4)   // 读4字节 → nValue
sub_77BBF1(packet)                   // 读1字节(符号) + 4字节(值) → 过期时间
```

#### 无条件读取

- 2个无条件 Decode1 -> SecondaryStat+0x90C 和 +0x938
- CTS_SwallowBuff: Decode1 -> esi+0x11B0
- CTS_Dice: 循环22次 Decode4 -> esi+0x1270 数组

---

## 5. 完整 Bit 位映射表

客户端二进制中 `sub_7807BF` (Reset函数)、`sub_873B8C`/`sub_78D977` 初始化函数确认的 82个标准buff位 (bit 0~81)，
7个扩展位 (bit 82~88)，以及 `OnTemporaryStatSet` 中额外使用的5个特殊掩码位。

**映射来源**：
- bit 0~6: `sub_7807BF` Reset函数前7个全局变量 + `sub_873B8C` 初始化
- bit 7~81: `sub_7807BF` Reset函数中82个全局变量的顺序 = `sub_781D0E` (DecodeForLocal) 的读取顺序
- bit 82~88: `sub_78D977(_, n)` = `sub_873B8C(_, n+82)` + 虚表循环
- 特殊掩码: `OnTemporaryStatSet` 中5个独立检测位

**注意**：旧版文档 §5.1~5.4 的 BuffStat 名称是按"枚举声明顺序 = 读取顺序"推导的，与掩码实测位号**完全对不上**。以下为经过 IDA 交叉验证的正确映射。

### 5.1 标准 buff 位 (bit 0~81) — `sub_7807BF` Reset 函数顺序

`sub_7807BF` 按 `sub_781D0E` 的读取顺序处理82个全局变量。每位占 SecondaryStat 结构体48字节（3个 `_ZtlSecureTear<long>` 对 = 12 DWORD）。

| 序 | 位号 | 全局变量 | 初始化函数 | BuffStat | 说明 |
|----|------|---------|-----------|---------|------|
| 1 | 0 | `dword_BEFF40` | `sub_873B8C(_, 0)` | WATK | 物攻 |
| 2 | 1 | `dword_BEFF30` | `sub_873B8C(_, 1)` | WDEF | 物防 |
| 3 | 2 | `dword_BEFF20` | `sub_873B8C(_, 2)` | MATK | 魔攻 |
| 4 | 3 | `dword_BEFF10` | `sub_873B8C(_, 3)` | MDEF | 魔防 |
| 5 | 4 | `dword_BEFF00` | `sub_873B8C(_, 4)` | ACC | 命中 |
| 6 | 5 | `dword_BEFEF0` | `sub_873B8C(_, 5)` | AVOID | 回避 |
| 7 | 6 | `dword_BEFEE0` | `sub_873B8C(_, 6)` | HANDS | 手部 |
| 8 | 7 | `dword_BF0000` | `sub_873B8C(_, 7)` | SPEED | 速度 |
| 9 | 8 | `dword_BEFFF0` | `sub_873B8C(_, 8)` | JUMP | 跳跃 |
| 10 | 9 | `dword_BEFED0` | `sub_873B8C(_, 9)` | MAGIC_GUARD | 魔法盾 |
| 11 | 10 | `dword_BEFEC0` | `sub_873B8C(_, 10)` | DARKSIGHT | 隐身 |
| 12 | 11 | `dword_BEFEB0` | `sub_873B8C(_, 11)` | BOOSTER | 加速 |
| 13 | 12 | `dword_BEFEA0` | `sub_873B8C(_, 12)` | POWERGUARD | 力量屏障 |
| 14 | 13 | `dword_BEFE90` | `sub_873B8C(_, 13)` | HYPERBODYHP | 超级体力 |
| 15 | 14 | `dword_BEFE80` | `sub_873B8C(_, 14)` | HYPERBODYMP | 超级魔力 |
| 16 | 15 | `dword_BEFE70` | `sub_873B8C(_, 15)` | INVINCIBLE | 无敌 |
| 17 | 16 | `dword_BEFE60` | `sub_873B8C(_, 16)` | SOULARROW | 灵魂之箭 |
| 18 | 17 | `dword_BEFFE0` | `sub_873B8C(_, 17)` | STUN | 眩晕（疾病） |
| 19 | 18 | `dword_BEFE50` | `sub_873B8C(_, 18)` | POISON | 中毒（疾病） |
| 20 | 19 | `dword_BEFE40` | `sub_873B8C(_, 19)` | SEAL | 封印（疾病） |
| 21 | 20 | `dword_BEFE30` | `sub_873B8C(_, 20)` | DARKNESS | 黑暗（疾病） |
| 22 | 21 | `dword_BEFE20` | `sub_873B8C(_, 21)` | COMBO / SUMMON | 连击 |
| 23 | 22 | `dword_BEFE10` | `sub_873B8C(_, 22)` | WK_CHARGE | 骑士冲击 |
| 24 | 23 | `dword_BEFE00` | `sub_873B8C(_, 23)` | DRAGONBLOOD | 龙血 |
| 25 | 24 | `dword_BEFDF0` | `sub_873B8C(_, 24)` | HOLY_SYMBOL | 神圣符号 |
| 26 | 25 | `dword_BEFDE0` | `sub_873B8C(_, 25)` | MESOUP | 金币增加 |
| 27 | 26 | `dword_BEFDD0` | `sub_873B8C(_, 26)` | SHADOWPARTNER | 暗影伴侣 |
| 28 | 27 | `dword_BEFDC0` | `sub_873B8C(_, 27)` | PICKPOCKET | 扒窃 |
| 29 | 28 | `dword_BEFDB0` | `sub_873B8C(_, 28)` | MESOGUARD | 金币保护 |
| 30 | 29 | `dword_BEFDA0` | `sub_873B8C(_, 29)` | THAW | 解冻 |
| 31 | 30 | `dword_BEFFD0` | `sub_873B8C(_, 30)` | WEAKEN | 虚弱（疾病） |
| 32 | 31 | `dword_BEFD90` | `sub_873B8C(_, 31)` | CURSE | 诅咒（疾病） |
| 33 | 32 | `dword_BEFFC0` | `sub_873B8C(_, 32)` | SLOW | 减速（疾病） |
| 34 | 33 | `dword_BEFFB0` | `sub_873B8C(_, 33)` | MORPH | 变身 |
| 35 | 49 | `dword_BEFFA0` | `sub_873B8C(_, 49)` | GHOST_MORPH | 幽灵变身 |
| 36 | 34 | `dword_BEFD80` | `sub_873B8C(_, 34)` | RECOVERY | 回复 |
| 37 | 35 | `dword_BEFF90` | `sub_873B8C(_, 35)` | MAPLE_WARRIOR | 冒险王 |
| 38 | 36 | `dword_BEFD70` | `sub_873B8C(_, 36)` | STANCE | 站姿 |
| 39 | 37 | `dword_BEFD60` | `sub_873B8C(_, 37)` | SHARP_EYES | 眼神凝聚 |
| 40 | 38 | `dword_BEFD50` | `sub_873B8C(_, 38)` | MANA_REFLECTION | 魔法反射 |
| 41 | 39 | `dword_BEFF80` | `sub_873B8C(_, 39)` | SEDUCE | 诱惑（疾病） |
| 42 | 40 | `dword_BEFD40` | `sub_873B8C(_, 40)` | SHADOW_CLAW / FISHABLE | 暗影之爪 |
| 43 | 41 | `dword_BEFD30` | `sub_873B8C(_, 41)` | INFINITY | 无穷 |
| 44 | 42 | `dword_BEFD20` | `sub_873B8C(_, 42)` | HOLY_SHIELD | 神圣之盾 |
| 45 | 43 | `dword_BEFD10` | `sub_873B8C(_, 43)` | HAMSTRING | 制裁 |
| 46 | 44 | `dword_BEFD00` | `sub_873B8C(_, 44)` | BLIND | 致盲 |
| 47 | 45 | `dword_BEFCF0` | `sub_873B8C(_, 45)` | CONCENTRATE | 集中 |
| 48 | 46 | `dword_BEFCE0` | `sub_873B8C(_, 46)` | PUPPET / BANMAP | 傀儡 |
| 49 | 47 | `dword_BEFCD0` | `sub_873B8C(_, 47)` | ECHO_OF_HERO | 英雄回声 |
| 50 | 50 | `dword_BEFCC0` | `sub_873B8C(_, 50)` | AURA | 光环 |
| 51 | 62 | `dword_BEFCB0` | `sub_873B8C(_, 62)` | MAP_CHAIR / DOJANGSHIELD | 椅子 |
| 52 | 51 | `dword_BEFCA0` | `sub_873B8C(_, 51)` | CONFUSE | 混乱（疾病） |
| 53 | 48 | `dword_BEFC90` | `sub_873B8C(_, 48)` | MESO_UP_BY_ITEM | 金币增加道具 |
| 54 | 52 | `dword_BEFC80` | `sub_873B8C(_, 52)` | COUPON_EXP1 / ITEM_UP_BY_ITEM | 优惠券经验1 |
| 55 | 53 | `dword_BEFC70` | `sub_873B8C(_, 53)` | COUPON_EXP2 / RESPECT_PIMMUNE | 优惠券经验2 |
| 56 | 54 | `dword_BEFC60` | `sub_873B8C(_, 54)` | COUPON_EXP3 / RESPECT_MIMMUNE | 优惠券经验3 |
| 57 | 55 | `dword_BEFC50` | `sub_873B8C(_, 55)` | COUPON_DRP1 / DEFENSE_ATT | 优惠券掉落1 |
| 58 | 56 | `dword_BEFC40` | `sub_873B8C(_, 56)` | COUPON_DRP2 / DEFENSE_STATE | 优惠券掉落2 |
| 59 | 59 | `dword_BEFC30` | `sub_873B8C(_, 59)` | BERSERK_FURY | 狂暴之怒 |
| 60 | 60 | `dword_BEFC20` | `sub_873B8C(_, 60)` | DIVINE_BODY | 神圣之躯 |
| 61 | 61 | `dword_BEFC10` | `sub_873B8C(_, 61)` | SPARK | 闪电 |
| 62 | 63 | `dword_BEFC00` | `sub_873B8C(_, 63)` | FINALATTACK / SOULMASTERFINAL | 最终攻击 |
| 63 | 64 | `dword_BEFBF0` | `sub_873B8C(_, 64)` | WINDBREAKERFINAL | - |
| 64 | 65 | `dword_BEFBE0` | `sub_873B8C(_, 65)` | ELEMENTAL_RESET | 属性重置 |
| 65 | 66 | `dword_BEFBD0` | `sub_873B8C(_, 66)` | WIND_WALK / MAGIC_SHIELD | 风之步 |
| 66 | 67 | `dword_BEFBC0` | `sub_873B8C(_, 67)` | EVENTRATE | - |
| 67 | 68 | `dword_BEFBB0` | `sub_873B8C(_, 68)` | ARAN_COMBO | 阿兰连击 |
| 68 | 69 | `dword_BEFBA0` | `sub_873B8C(_, 69)` | COMBO_DRAIN | 连击吸收 |
| 69 | 70 | `dword_BEFB90` | `sub_873B8C(_, 70)` | COMBO_BARRIER | 连击屏障 |
| 70 | 71 | `dword_BEFB80` | `sub_873B8C(_, 71)` | BODY_PRESSURE | 身体压力 |
| 71 | 72 | `dword_BEFB70` | `sub_873B8C(_, 72)` | SMART_KNOCKBACK | 智能反击 |
| 72 | 73 | `dword_BEFB60` | `sub_873B8C(_, 73)` | BERSERK | 狂暴 |
| 73 | 74 | `dword_BEFB50` | `sub_873B8C(_, 74)` | EXP_BUFF / EXPBUFFRATE | 经验增益 |
| 74 | 57 | `dword_BEFB40` | `sub_873B8C(_, 57)` | HPREC | HP回复 |
| 75 | 58 | `dword_BEFB30` | `sub_873B8C(_, 58)` | MPREC | MP回复 |
| 76 | 75 | `dword_BEFB20` | `sub_873B8C(_, 75)` | StopPotion | 停药（疾病） |
| 77 | 76 | `dword_BEFB10` | `sub_873B8C(_, 76)` | StopMotion | 停止动作（疾病） |
| 78 | 77 | `dword_BEFB00` | `sub_873B8C(_, 77)` | FEAR | 恐惧（疾病） |
| 79 | 78 | `dword_BEFAF0` | `sub_873B8C(_, 78)` | EVANSLOW | 龙魔减慢 |
| 80 | 79 | `dword_BEFAE0` | `sub_873B8C(_, 79)` | MAGIC_SHIELD | 魔法盾 |
| 81 | 80 | `dword_BEFAD0` | `sub_873B8C(_, 80)` | MAGIC_RESISTANCE | 魔法抗性 |
| 82 | 81 | `dword_BEFAC0` | `sub_873B8C(_, 81)` | SOULSTONE | 灵魂石 |

**读取顺序注意**：位号不是升序！客户端按上述"序"列处理：先 bit 0~33，然后跳到 bit 49 (GHOST_MORPH)，再回 bit 34~47，然后 bit 50 (AURA)、bit 62 (MAP_CHAIR)、bit 51 (CONFUSE)、bit 48 (MESO_UP_BY_ITEM)、bit 52~56、bit 59~63、bit 64~74、bit 57~58，最后 bit 75~81。

### 5.2 扩展位 (bit 82~88) — 通过虚表调用解码

`sub_78D977(output, n)` 内部调用 `sub_873B8C(n+82)`，即 n=0 → bit 82，n=6 → bit 88。

| bit | i | 初始化函数 | 虚表地址 | vtable[6] 解码函数 | 解码字节 | vtable[4] 超时函数 | 说明 |
|-----|---|-----------|---------|-------------------|---------|-------------------|------|
| 82 | 0 | `sub_78D977(_, 0)` → `dword_BEDF48` | off_AFE6D4 | sub_7941B7 | 15 (13+Decode2) | sub_794157: `1000*nSec` | ENERGY_CHARGE |
| 83 | 1 | `sub_78D977(_, 1)` → `dword_BEFF60` | off_AFE774 | sub_794332 | 15 (13+Decode2) | sub_7942D2: `1000*nSec` | DASH2 |
| 84 | 2 | `sub_78D977(_, 2)` → `dword_BEFF50` | off_AFE774 | sub_794332 | 15 (13+Decode2) | sub_7942D2: `1000*nSec` | DASH |
| 85 | 3 | `sub_78D977(_, 3)` → `dword_BEFF70` | off_AFE698 | sub_79407D | 13 (仅核心) | sub_794031: `0x7FFFFFFF` | MONSTER_RIDING, 永久 |
| 86 | 4 | `sub_78D977(_, 4)` | off_AFE738 | sub_793E28 | 20 (13+Decode2+额外5) | sub_793DBB: `1000*nSec` | SPEED_INFUSION |
| 87 | 5 | `sub_78D977(_, 5)` → `dword_BF5528` | off_AFE644 | sub_77C442 | 17 (13+Decode4) | sub_794031: `0x7FFFFFFF` | HOMING_BEACON, 永久 |
| 88 | 6 | `sub_78D977(_, 6)` | off_AFE774 | sub_794332 | 15 (13+Decode2) | sub_7942D2: `1000*nSec` | NOTDAMAGED |

**核心解码 sub_793EF2 读取 (13字节)**：
- 4字节 nID (DecodeBuffer)
- 4字节 nValue (DecodeBuffer)
- 1字节 sign + 4字节 timeDelta (sub_77BBF1) → 过期时间

### 5.3 OnTemporaryStatSet 中的5个特殊检测掩码

这些全局变量在 `OnTemporaryStatSet` 主循环之外单独使用，用于触发特殊逻辑：

| 全局变量 | 初始化函数 | UINT128 位号 | BuffStat | 客户端行为 |
|---------|-----------|------------|---------|-----------|
| `dword_BF5558` | `sub_873B8C(_, 68)` | bit 68 | ARAN_COMBO | **负向检查**：存在则跳过整个主循环（ComboAbility 类buff） |
| `dword_BF5548` | `sub_78D977(_, 3)` → bit 85 | bit 85 | MONSTER_RIDING | 骑宠/梯子处理，调用 `ShowRideVehicleEffect` |
| `dword_BF5538` | `sub_873B8C(_, 50)` | bit 50 | AURA | 调用 `sub_456FAB` 更新角色周围特效层 |
| `dword_BF5528` | `sub_78D977(_, 5)` → bit 87 | bit 87 | HOMING_BEACON | `CMobPool::GetMob` → `CMob::SetGuided` 设置引导子弹 |
| `dword_BF5518` | `sub_873B8C(_, 13)` | bit 13 | HYPERBODYHP | 销毁并重建 `CUIPartyHP` 窗口 |

### 5.4 `sub_77DC78` — 包格式检查位（触发额外 `Decode1`）

`sub_77DC78` OR 了**12个**全局变量。当这些buff中任何一个在掩码中置位时，`OnTemporaryStatSet`/`OnTemporaryStatReset` 在包尾额外读1字节（`SetSecondaryStatChangedPoint`）。

> **⚠ 反编译陷阱**：或链由 `sub_873F63`（`dest = op1 | op2`，**三操作数**：ECX=op1、栈上 dest/op2）串联而成。
> Hex-Rays 把每次调用都渲染成 `sub_873F63(vX, dword_Y)` 两参数、**把 ECX 操作数丢掉了**：
> 链上第 2~11 次丢的是上一次的累加值（无害），**唯独第 1 次丢的是实打实的常量 `dword_BF0000`**。
> 伪代码文本里搜不到 `BF0000`，必须看汇编码里 `mov ecx, offset dword_BF0000`（在 `sub_77DC78` 内，0x77DD06）
> 或用 xref 反查。下表第「种子」行即此前遗漏的那一位——它导致隐身术10级(speed=-12)少发1字节而闪退。

| 序 | 全局变量 | 位号 | BuffStat | 说明 |
|----|---------|------|---------|------|
| 种子 | `dword_BF0000` | 7 | SPEED | 速度（或链种子，走 ECX） |
| 1 | `dword_BEFFF0` | 8 | JUMP | 跳跃 |
| 2 | `dword_BEFFE0` | 17 | STUN | 眩晕 |
| 3 | `dword_BEFFD0` | 30 | WEAKEN | 虚弱 |
| 4 | `dword_BEFFC0` | 32 | SLOW | 减速 |
| 5 | `dword_BEFFB0` | 33 | MORPH | 变身 |
| 6 | `dword_BEFFA0` | 49 | GHOST_MORPH | 幽灵变身 |
| 7 | `dword_BEFF90` | 35 | MAPLE_WARRIOR | 冒险岛勇士 |
| 8 | `dword_BEFF80` | 39 | SEDUCE | 诱惑 |
| 9 | `dword_BEFF70` | 85 | MONSTER_RIDING | 骑宠 |
| 10 | `dword_BEFF60` | 83 | DASH2 | 冲刺2 |
| 11 | `dword_BEFF50` | 84 | DASH | 冲刺 |

### 5.5 `sub_7938D6` — 统计更新确认位

`sub_7938D6` OR 了**12个**全局变量后检查结果是否非零。当这些buff中任何一个在掩码中置位时，
客户端向服务端发送 opcode=0x6C 的统计更新确认包。在 `OnTemporaryStatSet`（0xA206C7）和 `OnTemporaryStatReset`（0xA208BA）中调用。

服务端对应数组：`BuffPackets.StatUpdateRequiredBuffStats`。

> **⚠ 同样的 ECX 种子陷阱**（见 §5.4）：或链种子是 `dword_BEFF40` = bit 0 = WATK，
> 汇编码 `mov ecx, offset dword_BEFF40` 在 `sub_7938D6` 内（0x79394B），伪代码同样看不到。

| 序 | 全局变量 | 位号 | BuffStat |
|----|---------|------|---------|
| 种子 | `dword_BEFF40` | 0 | WATK |
| 1 | `dword_BEFF20` | 2 | MATK |
| 2 | `dword_BEFF00` | 4 | ACC |
| 3 | `dword_BEFEF0` | 5 | AVOID |
| 4 | `dword_BEFE30` | 20 | DARKNESS |
| 5 | `dword_BEFE20` | 21 | COMBO |
| 6 | `dword_BEFE10` | 22 | WK_CHARGE |
| 7 | `dword_BEFF90` | 35 | MAPLE_WARRIOR |
| 8 | `dword_BEFD60` | 37 | SHARP_EYES |
| 9 | `dword_BEFCD0` | 47 | ECHO_OF_HERO |
| 10 | `dword_BEDF48` | 82 | ENERGY_CHARGE |
| 11 | `dword_BEFBB0` | 68 | ARAN_COMBO |

### 5.6 结构体布局

每个标准 buff (bit 0~81) 占 SecondaryStat 结构体48字节（3个 `_ZtlSecureTear<long>` 对）。
82个标准位 = 82 × 48 = 3936 字节 (0xF60)。
特殊位 bit 85 在偏移 +0xFC0，bit 87 在 +0x1020。
SecondaryStat 结构体总大小 >= 0x1050 (4176) 字节。

---

## 6. OnTemporaryStatSet 处理流程

### 6.1 主流程

```
1. 初始化 ZMap<long, ZRef<VIEWELEM>> mElem
2. 调用 sub_781D0E (DecodeForLocal):
   a. 读取 UINT128 mask (16字节)
   b. 获取当前时间 timeGetTime()
   c. 【标准解码】对 mask 中 bit 0~81 每个置位的 bit:
      * Decode2 -> nValue
      * Decode4 -> nSkillID
      * Decode4 -> nTimeOffset
      * 创建 VIEWELEM，存入 SecondaryStat 和 mElem
   d. 无条件 Decode1 -> SecondaryStat+0x90C
   e. 无条件 Decode1 -> SecondaryStat+0x938
   f. 【扩展解码】循环 i=0~6，检查 bit(i+82):
      * 若 mask 中该位置位:
        - 获取对象: object = *(esi+0xCA0 + i*8 + 4)
        - vtable[6](object, packet) 从包中读取数据
        - vtable[4](object) 获取超时时间
        - 创建 VIEWELEM，插入 mElem
   g. 返回 UINT128 (哪些位被成功解码)
3. Decode2 -> tDelay (延迟时间)
4. 检查特殊掩码位:
   - bit 68 (ARAN_COMBO): 若为真，检查是否属于 ComboAbility/NotDamaged/Aura 类
   - 若非上述类型，遍历 mElem 执行视效处理
5. 检查 bit 85 (MONSTER_RIDING): 骑乘坐骑处理
6. 检查 bit 50 (AURA): 护盾效果
7. 检查 bit 87 (HOMING_BEACON): 引导子弹
8. 检查 bit 13 (HYPERBODYHP): 重建队伍HP界面
9. 条件性 Decode1 -> nPoint (仅当入参掩码命中 sub_77DC78 的12位之一, 见 §5.4；与 tDelay 无关)
10. ValidateStat + 发送确认包
```

### 6.2 5个特殊检测常量

| 常量 | 初始化方式 | UINT128 位 | BuffStat | 功能 |
|------|-----------|-----------|---------|------|
| dword_BF5558 | sub_873B8C(68) | bit 68 | ARAN_COMBO | **负向检查**：存在则跳过整个主循环（ComboAbility 类buff） |
| dword_BF5548 | sub_78D977(3) -> bit 85 | bit 85 | MONSTER_RIDING | 骑乘坐骑，触发 ShowRideVehicleEffect |
| dword_BF5538 | sub_873B8C(50) | bit 50 | AURA | 护盾效果，加载 LoadBarrier 动画 |
| dword_BF5528 | sub_78D977(5) -> bit 87 | bit 87 | HOMING_BEACON | 引导子弹，设置怪物 SetGuided |
| dword_BF5518 | sub_873B8C(13) | bit 13 | HYPERBODYHP | 最大HP变更，重建 CUIPartyHP |

---

## 7. 相关函数地址索引

### 7.1 主要处理函数

| 地址 | 函数 | 说明 |
|------|------|------|
| 0xA202BE | CWvsContext::OnTemporaryStatSet | 主处理函数 |
| 0x781D0E | DecodeForLocal 等价函数 | 解码 buff 数据 (展开循环, 0x4CE7字节) |
| 0x7807BF | Reset 等价函数 | 重置 SecondaryStat |
| 0x873B8C | sub_873B8C(n) | 创建 UINT128 掩码 (bit n) |
| 0x78D977 | sub_78D977(a1, n) | 创建 UINT128 掩码 (bit n+82) |
| 0x7940E0 | sub_7940E0(this) | 返回 this[1] (对象指针) |
| 0x672293 | sub_672293(this) | 引用计数释放 + 返回 this+16 |

### 7.2 特殊常量初始化

| 地址 | 函数 | 说明 |
|------|------|------|
| 0xA1D3C0 | dword_BF5558 初始化 | bit 68 掩码 |
| 0xA1D3EB | dword_BF5548 初始化 | bit 85 掩码 |
| 0xA1D413 | dword_BF5538 初始化 | bit 50 掩码 |
| 0xA1D446 | dword_BF5528 初始化 | bit 87 掩码 |
| 0xA1D47A | dword_BF5518 初始化 | bit 13 掩码 |

### 7.3 扩展位 (82-88) 虚表地址

| 虚表地址 | 对应对象构造函数 | 适用位 |
|---------|----------------|--------|
| off_AFE6D4 (0xAFE6D4) | sub_77C550 (48字节) | bit 82 |
| off_AFE774 (0xAFE774) | sub_77C60B (48字节) | bit 83, 84, 88 |
| off_AFE698 (0xAFE698) | sub_77C4F8 (36字节) | bit 85 |
| off_AFE738 (0xAFE738) | sub_77C5E4 (52字节) | bit 86 |
| off_AFE644 (0xAFE644) | sub_77C4F8 修改版 (40字节) | bit 87 |

### 7.4 扩展位 vtable[6] 解码函数

| 地址 | 适用位 | 读取内容 |
|------|--------|---------|
| sub_7941B7 (0x7941B7) | bit 82 | sub_793EF2 + Decode2 |
| sub_794332 (0x794332) | bit 83, 84, 88 | sub_793EF2 + Decode2 |
| sub_79407D (0x79407D) | bit 85 | 仅 sub_793EF2 |
| sub_793E28 (0x793E28) | bit 86 | sub_793EF2 + Decode2 + 额外 Decode1+Decode4 |
| sub_77C442 (0x77C442) | bit 87 | sub_793EF2 + Decode4 |

### 7.5 核心解码辅助函数

| 地址 | 函数 | 说明 |
|------|------|------|
| 0x793EF2 | sub_793EF2(this, packet) | 核心解码: DecodeBuffer(4) + DecodeBuffer(4) + sub_77BBF1 |
| 0x77BBF1 | sub_77BBF1(packet) | 过期时间: Decode1(符号) + Decode4(值) |
| 0x79439A | sub_79439A | VIEWELEM 构造辅助 |
| 0x79463D | sub_79463D | mElem 插入检查 |
| 0x7943A1 | sub_7943A1 | VIEWELEM 获取 |
| 0x7943E2 | sub_7943E2 | mElem 插入操作 |
| 0x794384 | sub_794384 | VIEWELEM 析构 |

---

## 9. 位 82~88 扩展解码详细分析

### 9.1 对象数组结构

SecondaryStat 构造函数 (`0x77C241`) 在 `this+0xCA0` (偏移 3232) 处初始化 7 个 8 字节条目：

```
for (i = 0; i < 7; i++) {
    *(this + 0xCA0 + i*8 + 0) = type_indicator;  // 第一个 DWORD (类型)
    *(this + 0xCA0 + i*8 + 4) = object_ptr;       // 第二个 DWORD (对象指针, 带引用计数)
}
```

对象通过 `sub_7940E4` 存储，使用 `InterlockedIncrement` 进行引用计数管理。

### 9.2 各位的对象类型和构造

| i | 位 | 构造函数 | 对象大小 | vtable |
|---|---|---------|---------|--------|
| 0 | 82 | sub_77C550 | 48字节 | off_AFE6D4 |
| 1 | 83 | sub_77C60B | 48字节 | off_AFE774 |
| 2 | 84 | sub_77C60B | 48字节 | off_AFE774 |
| 3 | 85 | sub_77C4F8 | 36字节 | off_AFE698 |
| 4 | 86 | sub_77C5E4 | 52字节 | off_AFE738 |
| 5 | 87 | sub_77C4F8 (修改版) | 40字节 | off_AFE644 |
| 6 | 88 | sub_77C60B | 48字节 | off_AFE774 |

### 9.3 核心解码函数 sub_793EF2

所有 vtable[6] 函数都调用此核心解码函数：

```c
// sub_793EF2 (0x793EF2)
void sub_793EF2(Object* this, CInPacket* packet) {
    Lock(this + 24);  // ZSynchronizedHelper<ZFatalSection>
    
    DecodeBuffer(packet, this + 12, 4);  // 读4字节 → nID
    DecodeBuffer(packet, this + 16, 4);  // 读4字节 → nValue
    this[20] = sub_77BBF1(packet);       // 过期时间
    
    Unlock(this + 24);
}
```

### 9.4 过期时间计算 sub_77BBF1

```c
// sub_77BBF1 (0x77BBF1)
int sub_77BBF1(CInPacket* packet) {
    int currentTime = dword_BF060C();  // GetTickCount()
    int sign = Decode1(packet);        // 1字节: 0=加, 非0=减
    int delta = Decode4(packet);       // 4字节: 时间差值
    
    if (sign)
        return currentTime - delta;    // 过去的时间点
    else
        return currentTime + delta;    // 未来的时间点
}
```

### 9.5 各位的完整解码流程

#### bit 82 (i=0, sub_7941B7)

```c
void vtable6_bit82(Object* this, CInPacket* packet) {
    Lock(this + 24);
    sub_793EF2(this, packet);           // 13字节: nID + nValue + 过期时间
    this[22] = Decode2(packet);         // 2字节: nSec (秒数)
    Unlock(this + 24);
}

int vtable4_bit82(Object* this) {
    return 1000 * this[22];             // 返回毫秒: nSec * 1000
}
```

**总字节**: 15 (13 + Decode2)

#### bit 83, 84, 88 (i=1,2,6, sub_794332)

```c
void vtable6_bit83_84_88(Object* this, CInPacket* packet) {
    Lock(this + 24);
    sub_793EF2(this, packet);           // 13字节
    this[22] = Decode2(packet);         // 2字节: nSec
    Unlock(this + 24);
}

int vtable4_bit83_84_88(Object* this) {
    return 1000 * this[22];             // 返回毫秒
}
```

**总字节**: 15 (13 + Decode2)

#### bit 85 (i=3, sub_79407D) — MONSTER_RIDING

```c
void vtable6_bit85(Object* this, CInPacket* packet) {
    Lock(this + 24);
    sub_793EF2(this, packet);           // 13字节: 仅核心解码
    Unlock(this + 24);
}

int vtable4_bit85(Object* this) {
    return 0x7FFFFFFF;                  // 永久 (INT_MAX)
}
```

**总字节**: 13 (仅核心解码)

#### bit 86 (i=4, sub_793E28)

```c
void vtable6_bit86(Object* this, CInPacket* packet) {
    Lock(this + 24);
    sub_793EF2(this, packet);           // 13字节
    this[40] = sub_77BBF1(packet);      // 5字节: 额外过期时间
    this[48] = Decode2(packet);         // 2字节: nSec
    Unlock(this + 24);
}

int vtable4_bit86(Object* this) {
    return 1000 * this[48];             // 返回毫秒
}
```

**总字节**: 20 (13 + 5 + Decode2)

#### bit 87 (i=5, sub_77C442) — HOMING_BEACON

```c
void vtable6_bit87(Object* this, CInPacket* packet) {
    Lock(this + 24);
    sub_79407D(this, packet);           // 13字节 (调用 sub_793EF2)
    this[36] = Decode4(packet);         // 4字节: nValue
    Unlock(this + 24);
}

int vtable4_bit87(Object* this) {
    return 0x7FFFFFFF;                  // 永久 (INT_MAX)
}
```

**总字节**: 17 (13 + Decode4)
