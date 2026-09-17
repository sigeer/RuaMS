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
| [条件] uint8  nPoint    (Decode1)                 |  仅当 tDelay > 0 时 (在OnTemporaryStatSet中)
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

客户端二进制中 sub_7807BF (Reset函数) 和 sub_873B8C 初始化函数确认的 82个 buff 位 (bit 0~81)，以及 OnTemporaryStatSet 中额外使用的2个位 (bit 85, 87)。

### 5.1 bit 0~17

| bit | 常量 | 结构体偏移 | BuffStat 名 | 说明 |
|-----|------|-----------|-------------|------|
| 0 | dword_BEFF40 | +0x00 | MORPH | 变身 |
| 1 | dword_BEFF30 | +0x30 | RECOVERY | 回复 |
| 2 | dword_BEFF20 | +0x60 | MAPLE_WARRIOR | 冒险王 |
| 3 | dword_BEFF10 | +0x90 | STANCE | 站姿 |
| 4 | dword_BEFF00 | +0xC0 | SHARP_EYES | 眼神凝聚 |
| 5 | dword_BEFEF0 | +0xF0 | MANA_REFLECTION | 魔法反射 |
| 6 | dword_BEFEE0 | +0x120 | SHADOW_CLAW | 暗影之爪 |
| 7 | dword_BF0000 | +0x150 | INFINITY | 无穷 |
| 8 | dword_BEFFF0 | +0x180 | HOLY_SHIELD | 神圣之盾 |
| 9 | dword_BEFED0 | +0x1B0 | HAMSTRING | 制裁 |
| 10 | dword_BEFEC0 | +0x1E0 | BLIND | 致盲 |
| 11 | dword_BEFEB0 | +0x210 | CONCENTRATE | 集中 |
| 12 | dword_BEFEA0 | +0x240 | PUPPET | 傀儡 |
| 13 | dword_BEFE90 | +0x270 | ECHO_OF_HERO | 英雄回声 |
| 14 | dword_BEFE80 | +0x2A0 | MESO_UP_BY_ITEM | 金币增加道具 |
| 15 | dword_BEFE70 | +0x2D0 | GHOST_MORPH | 幽灵变身 |
| 16 | dword_BEFE60 | +0x300 | AURA | 光环 |
| 17 | dword_BEFFE0 | +0x330 | CONFUSE | 混乱 |

### 5.2 bit 18~48

| bit | 常量 | 结构体偏移 | BuffStat 名 | 说明 |
|-----|------|-----------|-------------|------|
| 18 | dword_BEFE50 | +0x360 | EXP_BUFF | 经验增益 |
| 19 | dword_BEFE40 | +0x390 | COUPON_EXP1 | 优惠券经验1 |
| 20 | dword_BEFE30 | +0x3C0 | COUPON_EXP2 | 优惠券经验2 |
| 21 | dword_BEFE20 | +0x3F0 | COUPON_EXP3 | 优惠券经验3 |
| 22 | dword_BEFE10 | +0x420 | COUPON_DRP1 | 优惠券掉落1 |
| 23 | dword_BEFE00 | +0x450 | COUPON_DRP2 | 优惠券掉落2 |
| 24 | dword_BEFDF0 | +0x480 | ITEM_UP_BY_ITEM | 道具升级 |
| 25 | dword_BEFDE0 | +0x4B0 | RESPECT_PIMMUNE | 无视物理抵抗 |
| 26 | dword_BEFDD0 | +0x4E0 | RESPECT_MIMMUNE | 无视魔法抵抗 |
| 27 | dword_BEFDC0 | +0x510 | DEFENSE_ATT | 属性抗性 |
| 28 | dword_BEFDB0 | +0x540 | DEFENSE_STATE | 状态抗性 |
| 29 | dword_BEFDA0 | +0x570 | HPREC | HP回复 |
| 30 | dword_BEFFD0 | +0x5A0 | MPREC | MP回复 |
| 31 | dword_BEFD90 | +0x5D0 | BERSERK_FURY | 狂暴之怒 |
| 32 | dword_BEFFC0 | +0x600 | DIVINE_BODY | 神圣之躯 |
| 33 | dword_BEFFB0 | +0x630 | SPARK | 闪电 |
| 34 | dword_BEFD80 | +0x660 | MAP_CHAIR | 椅子 |
| 35 | dword_BEFF90 | +0x690 | FINALATTACK | 最终攻击 |
| 36 | dword_BEFD70 | +0x6C0 | WATK | 物攻 |
| 37 | dword_BEFD60 | +0x6F0 | WDEF | 物防 |
| 38 | dword_BEFD50 | +0x720 | MATK | 魔攻 |
| 39 | dword_BEFF80 | +0x750 | MDEF | 魔防 |
| 40 | dword_BEFD40 | +0x780 | ACC | 命中 |
| 41 | dword_BEFD30 | +0x7B0 | AVOID | 回避 |
| 42 | dword_BEFD20 | +0x7E0 | HANDS | 手部 |
| 43 | dword_BEFD10 | +0x810 | SPEED | 速度 |
| 44 | dword_BEFD00 | +0x840 | JUMP | 跳跃 |
| 45 | dword_BEFCF0 | +0x870 | MAGIC_GUARD | 魔法盾 |
| 46 | dword_BEFCE0 | +0x8A0 | DARKSIGHT | 隐身 |
| 47 | dword_BEFCD0 | +0x8D0 | BOOSTER | 加速 |
| 48 | dword_BEFC90 | +0x900 | POWERGUARD | 力量屏障 |

### 5.3 bit 49~63

| bit | 常量 | 结构体偏移 | BuffStat 名 | 说明 |
|-----|------|-----------|-------------|------|
| 49 | dword_BEFFA0 | +0x930 | HYPERBODYHP | 超级体力 |
| 50 | dword_BEFCC0 | +0x960 | HYPERBODYMP | 超级魔力 |
| 51 | dword_BEFCA0 | +0x990 | INVINCIBLE | 无敌 |
| 52 | dword_BEFC80 | +0x9C0 | SOULARROW | 灵魂之箭 |
| 53 | dword_BEFC70 | +0x9F0 | STUN | 眩晕 |
| 54 | dword_BEFC60 | +0xA20 | POISON | 中毒 |
| 55 | dword_BEFC50 | +0xA50 | SEAL | 封印 |
| 56 | dword_BEFC40 | +0xA80 | DARKNESS | 黑暗 |
| 57 | dword_BEFB40 | +0xAB0 | COMBO | 连击 |
| 58 | dword_BEFB30 | +0xAE0 | WK_CHARGE | 骑士冲击 |
| 59 | dword_BEFC30 | +0xB10 | DRAGONBLOOD | 龙血 |
| 60 | dword_BEFC20 | +0xB40 | HOLY_SYMBOL | 神圣符号 |
| 61 | dword_BEFC10 | +0xB70 | MESOUP | 金币增加 |
| 62 | dword_BEFCB0 | +0xBA0 | SHADOWPARTNER | 暗影partner |
| 63 | dword_BEFC00 | +0xBD0 | PICKPOCKET | 扒窃 |

### 5.4 bit 64~81

| bit | 常量 | 结构体偏移 | BuffStat 名 | 说明 |
|-----|------|-----------|-------------|------|
| 64 | dword_BEFBF0 | +0xC00 | MESOGUARD | 金币保护 |
| 65 | dword_BEFBE0 | +0xC30 | THAW | 解冻 |
| 66 | dword_BEFBD0 | +0xC60 | WEAKEN | 虚弱 |
| 67 | dword_BEFBC0 | +0xC90 | CURSE | 诅咒 |
| 68 | dword_BEFBB0 | +0xCC0 | SLOW | 减速 |
| 69 | dword_BEFBA0 | +0xCF0 | ELEMENTAL_RESET | 属性重置 |
| 70 | dword_BEFB90 | +0xD20 | MAGIC_SHIELD | 魔法盾 |
| 71 | dword_BEFB80 | +0xD50 | MAGIC_RESISTANCE | 魔法抗性 |
| 72 | dword_BEFB70 | +0xD80 | WIND_WALK | 风之步 |
| 73 | dword_BEFB60 | +0xDB0 | ARAN_COMBO | 阿连连击 |
| 74 | dword_BEFB50 | +0xDE0 | COMBO_DRAIN | 连击吸收 |
| 75 | dword_BEFB20 | +0xE10 | COMBO_BARRIER | 连击屏障 |
| 76 | dword_BEFB10 | +0xE40 | BODY_PRESSURE | 身体压力 |
| 77 | dword_BEFB00 | +0xE70 | SMART_KNOCKBACK | 智能反击 |
| 78 | dword_BEFAF0 | +0xEA0 | BERSERK | 狂暴 |
| 79 | dword_BEFAE0 | +0xED0 | SUMMON_BOMB | 召唤炸弹 |
| 80 | dword_BEFAD0 | +0xF00 | SWALLOW_BUFF | 吞噬增益 |
| 81 | dword_BEFAC0 | +0xF30 | BLESSING_ARMOR | 祝福之铠 |

### 5.5 扩展位 (位 82~88, 通过虚表调用解码)

sub_78D977(output, n) 内部调用 sub_873B8C(n+82)，即 n=0 -> bit 82，n=6 -> bit 88。

| bit | i | 虚表地址 | vtable[6] 解码函数 | 解码字节 | vtable[4] 超时函数 | 说明 |
|-----|---|---------|-------------------|---------|-------------------|------|
| 82 | 0 | off_AFE6D4 | sub_7941B7 | 15 (13+Decode2) | sub_794157: `1000*nSec` | 标准扩展 |
| 83 | 1 | off_AFE774 | sub_794332 | 15 (13+Decode2) | sub_7942D2: `1000*nSec` | 标准扩展 |
| 84 | 2 | off_AFE774 | sub_794332 | 15 (13+Decode2) | sub_7942D2: `1000*nSec` | 标准扩展 |
| 85 | 3 | off_AFE698 | sub_79407D | 13 (仅核心) | sub_794031: `0x7FFFFFFF` | MONSTER_RIDING, 永久 |
| 86 | 4 | off_AFE738 | sub_793E28 | 20 (13+Decode2+额外5) | sub_793DBB: `1000*nSec` | 额外 Decode1+Decode4 |
| 87 | 5 | off_AFE644 | sub_77C442 | 17 (13+Decode4) | sub_794031: `0x7FFFFFFF` | HOMING_BEACON, 永久 |
| 88 | 6 | off_AFE774 | sub_794332 | 15 (13+Decode2) | sub_7942D2: `1000*nSec` | 标准扩展 |

**核心解码 sub_793EF2 读取 (13字节)**：
- 4字节 nID (DecodeBuffer)
- 4字节 nValue (DecodeBuffer)
- 1字节 sign + 4字节 timeDelta (sub_77BBF1) → 过期时间

### 5.6 特殊常量位 (OnTemporaryStatSet 中用于特效处理)

| bit | 常量 | 结构体偏移 | BuffStat 名 | 说明 |
|-----|------|-----------|-------------|------|
| 85 | dword_BF5548 | +0xFC0 | MONSTER_RIDING | 骑乘坐骑 |
| 87 | dword_BF5528 | +0x1020 | HOMING_BEACON | 导向信标 |

### 5.7 结构体总大小

82个连续位 (bit 0~81) = 82 x 48 = 3936 字节 (0xF60)。
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
   - bit 68 (STANCE): 若为真，检查是否属于 ComboAbility/NotDamaged/Aura 类
   - 若非上述类型，遍历 mElem 执行视效处理
5. 检查 bit 85 (MONSTER_RIDING): 骑乘坐骑处理
6. 检查 bit 50 (MESOGUARD): 护盾效果
7. 检查 bit 87 (HOMING_BEACON): 引导子弹
8. 检查 bit 13 (HYPERBODYHP): 重建队伍HP界面
9. 条件性 Decode1 -> nPoint (仅当 tDelay > 0)
10. ValidateStat + 发送确认包
```

### 6.2 5个特殊检测常量

| 常量 | 初始化方式 | UINT128 位 | 功能 |
|------|-----------|-----------|------|
| dword_BF5558 | sub_873B8C(68) | bit 68 | 检测 ComboAbility/NotDamaged/Aura 类buff |
| dword_BF5548 | sub_78D977(3) -> bit 85 | bit 85 | 骑乘坐骑，触发 ShowRideVehicleEffect |
| dword_BF5538 | sub_873B8C(50) | bit 50 | 护盾效果，加载 LoadBarrier 动画 |
| dword_BF5528 | sub_78D977(5) -> bit 87 | bit 87 | 引导子弹，设置怪物 SetGuided |
| dword_BF5518 | sub_873B8C(13) | bit 13 | 最大HP变更，重建 CUIPartyHP |

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
