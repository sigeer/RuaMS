# CUserRemote::DecodeForRemote（GIVE_FOREIGN_BUFF）分析与 GiveRemoteBuff 实现

> IDA 数据库：`Angel.i64`（v83，基址 0x400000）
> 相关代码：`src/Application.Core/Channel/Net/Packets/BuffPackets.cs`、`src/Application.Core/tools/PacketCreator.cs`
> 关联文档：`docs/CWvsContext_OnTemporaryStatSet_分析.md`（本地包 DecodeForLocal）

---

## 1. 结论速览

| 项目 | 本地包（自己看自己） | 远程包（别人看自己） |
|------|---------------------|---------------------|
| opcode | `GIVE_BUFF = 0x20` | `GIVE_FOREIGN_BUFF = 0xC7` |
| 客户端入口 | `CWvsContext::OnTemporaryStatSet` (0xA202BE) | `CUserRemote` → `sub_98385D` (0x98385D) |
| 解码函数 | `sub_781D0E` (0x781D0E) | `sub_788156` (0x788156) |
| 掩码 | 16 字节（同一个 UINT128） | 16 字节（同一个 UINT128） |
| 普通字段长度 | 固定 short + int + int（7 字节/位） | **逐位不同：0 / 1 / 2 / 4 / 6 字节** |
| 参与的状态 | 全部 82 个位 | 只有 31 个位 + 7 个特殊位 |
| 收尾 | 2 字节 + 2 字节 | 同上 |

`sub_98385D` 的完整流程：

```c
int __thiscall sub_98385D(CUser* this, CInPacket* p) {
    UINT128 mask = sub_788156(&this->SecondaryStat, p);   // ① 读掩码 + 读各字段
    if (!this->flag_0x568) {
        unsigned short delay = p->Decode2();              // ② 尾部 2 字节
        this->vtable[8](this, mask, delay, 0);            // ③ CUser::OnTemporaryStatChanged
        if (mask & (1 << 85))                             // ④ MONSTER_RIDING
            CUser::ShowRideVehicleEffect(this, ...);
        if (mask & dword_BF1378 /* 1<<50 */)              // ⑤ AURA 周围特效
            sub_456FAB(this + 34);
    }
}
```

因此远程包的结构就是 `sub_788156` 顺序读下来的东西：

```
[int]     chrId                                    （由 CUserPool::OnUserRemotePacket 先读掉）
[16 字节] mask                                      DecodeBuffer(&mask, 0x10)
-- 下面 31 个字段按固定顺序，每一位读多少字节都不一样
[?]       ...
[byte]    DefenseAtt                                Decode1  (0x788950)
[byte]    DefenseState                              Decode1  (0x78896A)
-- 掩码位 82..88：客户端 SecondaryStat 里 7 个独立对象的 vtable[6] 解码
[?]       ...
[short]   delay                                     Decode2  (0x983899)
```

---

## 2. 128 位掩码编码

### 2.1 客户端 UINT128 的存放顺序

`firstmask`/`secondmask` 由服务端这样写出：

```csharp
p.writeLong(firstmask);   // 前 8 字节
p.writeLong(secondmask);  // 后 8 字节
```

客户端 `DecodeBuffer(&mask, 0x10)` 原样填进 `UINT128`，但**这个 UINT128 是按 dword 逆序存放的**：

* `UINT128::UINT128(unsigned long)`（0x873B22）的实现是 `this[0]=0; this[1]=0; this[2]=0; this[3]=value;`
  —— 值放进**最后一个** dword，也就是 `dword[3]` 才是低位。
* `sub_873B8C(this, n)`（0x873B8C）是左移 n 位的实现（n=32 时 `result[0]=src[1], result[1]=src[2]...`），
  进位从高索引 dword 传向低索引 dword，同样说明 **索引越大位越低**。
* `sub_78D977(dst, n)`（0x78D977）= `(UINT128)1 << (n + 82)`，`n=3` 就是 `1<<85`（MONSTER_RIDING）。

于是客户端位号与包内字节的关系是：

```
bit  96..127 ← mask 字节 0..3   （= firstmask 低 dword）
bit  64..95  ← mask 字节 4..7   （= firstmask 高 dword）
bit  32..63  ← mask 字节 8..11  （= secondmask 低 dword）
bit  0..31   ← mask 字节 12..15 （= secondmask 高 dword）
```

### 2.2 位号计算公式

旧表示（`EnumClass` 的「2 的幂 + `IsFirst`」）要经过下面这种换算才能得到位号：

```csharp
// 旧表示：value 只有一个 bit，bit = 该 bit 的位置
if (IsFirst)  return bit < 32 ? 96 + bit : 64 + (bit - 32);
else          return bit < 32 ? 32 + bit : bit - 32;
```

现在 `BuffStat` 的值**直接就是客户端掩码位**（第 7 节），所以封包层不需要任何换算，
位号 → 16 字节掩码统一由 `BuffPackets.WriteStatMask` 生成。
位号 → 名字的对照表见测试工程里的 `test/ServiceTest/Infrastructure/TemporaryStatType.cs`。

### 2.3 位号锚点（IDA 实测证据）

| 客户端位 | 状态 | 证据 |
|---------|------|------|
| 85 | `MONSTER_RIDING`（0x20000000000000，IsFirst） | `sub_98385D`/`sub_983921` 用 `sub_78D977(_,3)=1<<85` 判断后调用 `CUser::ShowRideVehicleEffect`；本地侧 `dword_BF5548` 同样是 `1<<85`，并配合 `SecondaryStat::IsRidingSkillVehicle` |
| 87 | `GuidedBullet`（`BuffStat.HOMING_BEACON`，0x80000000000000，IsFirst） | `dword_BF5528 = sub_78D977(_,5) = 1<<87`，本地解码后用怪物 oid 调 `CMob::SetGuided`；其 vtable[6] 是 `sub_77C442`（旧代码注释里就写着 sub_77C442 = HOMING_BEACON） |
| 13 | `HYPERBODYHP`（0x200000000000） | `dword_BF5518 = 1<<13` → `CUIPartyHP::Create`（超级体力要重建队伍 HP 条） |
| 68 | `ARAN_COMBO`（0x1000000000，IsFirst） | `dword_BF5558 = 1<<68` → `CSkillInfo::GetSkillLevel(..., 5221006, ...)`（Combo Ability） |
| 50 | `AURA`（0x40000） | `dword_BF5538 = 1<<50`（本地）/ `dword_BF1378 = 1<<50`（远程）→ `sub_456FAB` 加载角色周围特效层；0x40000 在旧枚举里就是 ARIANT_COSS_IMU「角色周围的白球」 |
| 65 / 66 | `ELEMENTALRESET` / `WIND_WALK` | `BuffStat.ELEMENTAL_RESET` / `WIND_WALK` 的值与位号一致；注意 `BuffStat.SLOW` / `MAGIC_SHIELD` / `MAGIC_RESISTANCE` 也落在 65/66/67（第 7.1 节）。`writeLongMaskSlowD` 的 `0x00000800_0000_0000` = firstmask bit 43 → 位 75（第 8 节遗留 2） |
| 7 / 21 | `SPEED` / `COMBO` | 客户端只读 1 字节（速度、连击数） |
| 10 / 16 / 26 | `DARKSIGHT` / `SOULARROW` / `SHADOWPARTNER` | 客户端只置位不读数据，与技能语义一致 |
| 33 / 49 | `MORPH` / `GHOST_MORPH` | 客户端读 2 字节（形态 id） |
| 62 | `DOJANGSHIELD`（`BuffStat.MAP_CHAIR`，同一位） | 客户端读 4 字节 |
| 31 / 39 | `CURSE` / `SEDUCE` | `Disease.CURSE = 0x8000000000000000 → 31`、`Disease.SEDUCE = 0x80 → 39` |

> 注意：`docs/CWvsContext_OnTemporaryStatSet_分析.md` 里 §5.1~5.4 的 bit→BuffStat 表是按“读取顺序 = 枚举声明顺序”
> 推出来的，和掩码实测位号对不上；该文档 §6.2 的语义标注（bit 85/87/13/50/68）与本文一致。

---

## 3. 31 个普通字段（严格按客户端读取顺序）

| 序 | 位 | 状态 | 读取 | 字节 | IDA 常量 / 指令 |
|----|----|------|------|------|----------------|
| 1 | 7 | `SPEED` | Decode1 | 1 | `dword_BF0000` / 0x788197 |
| 2 | 21 | `COMBO` | Decode1 | 1 | `dword_BEFE20` / 0x7881CE |
| 3 | 22 | `WK_CHARGE` | Decode4 | 4 | `dword_BEFE10` / 0x78821C |
| 4 | 17 | `STUN`（疾病） | Decode4 | 4 | `dword_BEFFE0` / 0x788265 |
| 5 | 20 | `DARKNESS`（疾病） | Decode4 | 4 | `dword_BEFE30` / 0x7882AE |
| 6 | 19 | `SEAL`（疾病） | Decode4 | 4 | `dword_BEFE40` / 0x7882F7 |
| 7 | 30 | `WEAKEN`（疾病） | Decode4 | 4 | `dword_BEFFD0` / 0x788340 |
| 8 | 31 | `CURSE`（疾病） | Decode4 | 4 | `dword_BEFD90` / 0x788389 |
| 9 | 18 | `POISON`（疾病） | Decode2 + Decode4 | 6 | `dword_BEFE50` ×2 / 0x7883BF、0x78840B |
| 10 | 26 | `SHADOWPARTNER` | 只置位 | 0 | `dword_BEFDD0` / 0x788423 |
| 11 | 10 | `DARKSIGHT` | 只置位 | 0 | `dword_BEFEC0` / 0x788452 |
| 12 | 16 | `SOULARROW` | 只置位 | 0 | `dword_BEFE60` / 0x788481 |
| 13 | 33 | `MORPH` | Decode2 | 2 | `dword_BEFFB0` / 0x7884CE |
| 14 | 49 | `GHOST_MORPH` | Decode2 | 2 | `dword_BEFFA0` / 0x788507 |
| 15 | 39 | `SEDUCE`（疾病） | Decode4 | 4 | `dword_BEFF80` / 0x788553 |
| 16 | 40 | `SHADOW_CLAW` / `FISHABLE` | Decode4 | 4 | `dword_BEFD40` / 0x788589 |
| 17 | 46 | `PUPPET` / `ZOMBIFY` | Decode4 | 4 | `dword_BEFCE0` / 0x7885D2 |
| 18 | 50 | `AURA` | Decode4 | 4 | `dword_BEFCC0` / 0x78861B |
| 19 | 62 | `MAP_CHAIR` | Decode4 | 4 | `dword_BEFCB0` / 0x788664 |
| 20 | 51 | `CONFUSE`（疾病） | Decode4 | 4 | `dword_BEFCA0` / 0x7886AD |
| 21 | 53 | `COUPON_EXP2` / `RESPECT_PIMMUNE` | Decode4 | 4 | `dword_BEFC70` / 0x7886E3 |
| 22 | 54 | `COUPON_EXP3` / `RESPECT_MIMMUNE` | Decode4 | 4 | `dword_BEFC60` / 0x788719 |
| 23 | 55 | `COUPON_DRP1` / `DEFENSE_ATT` | Decode4 | 4 | `dword_BEFC50` / 0x78874F |
| 24 | 56 | `COUPON_DRP2` / `DEFENSE_STATE` | Decode4 | 4 | `dword_BEFC40` / 0x788785 |
| 25 | 59 | `BERSERK_FURY` | 只置位 | 0 | `dword_BEFC30` / 0x78879D |
| 26 | 60 | `DIVINE_BODY` | 只置位 | 0 | `dword_BEFC20` / 0x7887CC |
| 27 | 66 | `WIND_WALK`（`BuffStat.MAGIC_SHIELD` 当前也落在这一位，见第 7.1 节） | 只置位 | 0 | `dword_BEFBD0` / 0x7887FB |
| 28 | 73 | `BERSERK` | Decode4 | 4 | `dword_BEFB60` / 0x78885B |
| 29 | 75 | `StopPotion`（疾病） | Decode4 | 4 | `dword_BEFB20` / 0x7888A4 |
| 30 | 76 | `StopMotion`（疾病） | Decode4 | 4 | `dword_BEFB10` / 0x7888ED |
| 31 | 77 | `FEAR`（疾病，`Disease.Blind`） | Decode4 | 4 | `dword_BEFB00` / 0x788936 |

之后恒有 2 个 `Decode1`（0x788950 / 0x78896A）→ 对应本地包同一位置的 `DefenseAttChar` / `DefenseStateChar`。

**疾病类字段的 4 字节就是 `writeMobSkillId` 的 `short(type) + short(level)`**
（等价于一个 int：`type | (level << 16)`）；POISON 额外在前面多 2 字节（中毒值）。

---

## 4. 7 个特殊字段（掩码位 82..88）

`sub_788156` 末尾循环 `i = 0..6`，用 `sub_78D977(&tmp, i)` 得到位 `82+i`，
再取 `SecondaryStat + 0xCA4 + i*8` 处的对象指针，调用其 `vtable[0x18]`：

| 位 | 状态 | vtable[0x18] | 读取内容 | 总字节 |
|----|------|-------------|---------|--------|
| 82 | `ENERGY_CHARGE` | `sub_7941B7` | `sub_793EF2` + Decode2 | 15 |
| 83 | `DASH2` | `sub_794332` | `sub_793EF2` + Decode2 | 15 |
| 84 | `DASH` | `sub_794332` | `sub_793EF2` + Decode2 | 15 |
| 85 | `MONSTER_RIDING` | `sub_79407D` | `sub_793EF2` | 13 |
| 86 | `SPEED_INFUSION` | `sub_793E28` | `sub_793EF2` + Decode1+Decode4 + Decode2 | 20 |
| 87 | `HOMING_BEACON` | `sub_77C442` | `sub_793EF2` + Decode4 | 17 |
| 88 | 未命名（IsFirst 组 0x4000_0000_0000_0） | `sub_794332` | `sub_793EF2` + Decode2 | 15 |

`sub_793EF2`（所有特殊字段的公共头，13 字节）：

```c
DecodeBuffer(obj + 12, 4);      // int  值（骑宠=坐骑道具 id）
DecodeBuffer(obj + 16, 4);      // int  来源（技能/道具 id）
obj[20] = sub_77BBF1(packet);   // byte 符号 + int 差值（0 → 当前时间+差值）
```

对 SPEED_INFUSION，`sub_793E28` 会再调一次 `sub_77BBF1`（byte + int），
这与仓库现有本地实现 `BuffPackets.GiveBuff` 里 `if (statup.Key == BuffStat.SPEED_INFUSION)` 的那 5 字节一致。

---

## 5. 本地包 GiveBuff 同样是固定顺序（本次一并修复）

`sub_781D0E`（DecodeForLocal）是 **82 个 `if (mask & 常量) { Decode2 + Decode4 + Decode4 }`** 串起来的固定链，
客户端就是按这条链顺序消费包的，所以本地包也必须按同样的顺序写字段。

读取顺序（位号，来自 IDA 里 82 次 `mask & 常量` 的先后 + 每个常量初始化函数给出的位号）：

```
0..33, 49, 34..47, 50, 62, 51, 48, 52..56, 59, 60, 61, 63, 64..74, 57, 58, 75..81
```

换算成状态就是：
`WATK,WDEF,MATK,MDEF,ACC,AVOID,HANDS,SPEED,JUMP,MAGIC_GUARD,DARKSIGHT,BOOSTER,POWERGUARD,HYPERBODYHP,HYPERBODYMP,INVINCIBLE,SOULARROW,STUN,POISON,SEAL,DARKNESS,COMBO,WK_CHARGE,DRAGONBLOOD,HOLY_SYMBOL,MESOUP,SHADOWPARTNER,PICKPOCKET,MESOGUARD,THAW,WEAKEN, CURSE, 0x1, MORPH, GHOST_MORPH, RECOVERY,MAPLE_WARRIOR,STANCE,SHARP_EYES,MANA_REFLECTION, SEDUCE, SHADOW_CLAW,INFINITY,HOLY_SHIELD,HAMSTRING,BLIND,CONCENTRATE,PUPPET,ECHO_OF_HERO, AURA, MAP_CHAIR, CONFUSE, MESO_UP_BY_ITEM, COUPON_EXP1..2, COUPON_DRP1..2, BERSERK_FURY,DIVINE_BODY,SPARK, FINALATTACK, …, SLOW,MAGIC_SHIELD,MAGIC_RESISTANCE,ARAN_COMBO,…,BERSERK,EXP_BUFF, HPREC,MPREC, …`

注意它**不是位号升序**：`49(GHOST_MORPH)` 紧跟在 `33(MORPH)` 后面，`HPREC/MPREC(57/58)` 排在 `EXP_BUFF(74)` 之后。

> **更正**：早期版本这里写过「远程包顺序是本地顺序的子序列，客户端只有一份字段顺序」——这是错的。
> 两个解码函数各自写死了一套顺序，本地包开头读 `7,8,9…`（状态序），远程包开头读 `7,21,22,17,20,19…`。
> 远程包必须用自己那张表（第 3 节 / `ClientRemoteFields`），套用本地顺序会把字段整体排错（详见第 9 节遗留 8）。

### 5.1 旧 `GiveBuff` 的问题

旧实现直接按 `ctx.NormalBuffStats`（= `StatEffect.statups`，来自 WZ 映射 + 每技能追加）的顺序写，
与客户端顺序没有任何关系，例如：

| 位置 | 实际顺序 | 客户端要求 |
|------|---------|-----------|
| `StatEffect.cs:220-230` | `HPREC, MPREC, WATK, WDEF, MATK, MDEF, ACC, AVOID, SPEED, JUMP` | `WATK, WDEF, MATK, MDEF, ACC, AVOID, SPEED, JUMP, …, HPREC, MPREC` |
| `PotionItemTemplate`（`StatEffect.cs:252-254`） | `AURA(50), BERSERK(73), BOOSTER(11)` | `BOOSTER(11), AURA(50), BERSERK(73)` |
| `MAP_CHAIR`（`StatEffect.cs:353`）先于技能状态 | 椅子状态在最前 | `MAP_CHAIR(62)` 在第二组末尾 |

另外 `BuffStat.COMBO` 与 `BuffStat.SUMMON` 共用同一个掩码位（0x20000000000000 → 位 21），
旧实现会为**同一个位写两份 7 字节数据**，客户端只读一份 → 也会错位。

### 5.2 修复

`BuffPackets.GiveBuff` 现在：

1. 直接遍历 `ClientFieldOrder`（客户端**本地包**字段顺序表，元素就是 `BuffStat` 成员）按位输出，
   调用方给出的 `NormalBuffStats` 是什么顺序都无所谓；
2. 遍历时用 `NormalBuffStats.TryGetValue(stat, …)` 命中才写，所以同一位天然只写一份
   （`COMBO`/`SUMMON` 这类别名在枚举里是同一个值，字典里也不可能同时存在两个键）；
3. 特殊 buff 走 `BuffParameter.EncodeSpecial`，同样按 `ClientSpecialFieldOrder`（82..88）遍历；
4. 掩码由 `WriteStatMask(ctx.AllBuffStats.Keys)` 生成，与写出的字段集合一致（两边用的是同一套位号）。

远程包 `GiveRemoteBuff` 用的是**另一张**顺序表 `ClientRemoteFields`（就是第 3 节那 31 个字段），
它的顺序与本地包不同、不能共用；三张表（`ClientFieldOrder` / `ClientRemoteFields` / `ClientSpecialFieldOrder`）
都是“遍历顺序表 + 命中就写”的同一种写法，不再有排序、下标表之类的额外结构。

---

## 6. 与旧实现的差异

| 旧实现 | 问题 |
|--------|------|
| `PacketCreator.giveForeignBuff`：掩码后给每个状态写 `short` 值，最后 `int 0` + `short 0` | 客户端根本不这样读：SPEED/COMBO 是 1 字节，MORPH/GHOST_MORPH 是 2 字节，大部分“有值”的状态是 4 字节，DARKSIGHT/SOULARROW/SHADOWPARTNER 等**不读任何字节**。字段长度一错，后面的字段全部错位；而且 31 个字段是**固定顺序**，不是按传入列表顺序 |
| `PacketCreator.giveForeignDebuff`：按传入的 Disease 列表顺序写 `mobSkillId` | 客户端按掩码位固定顺序（…19 SEAL、20 DARKNESS、30 WEAKEN、18 POISON…）读；一次上两个以上疾病就会串位 |
| `PacketCreator.showMonsterRiding` | 手写包，字节数恰好对得上，但掩码/字段含义全靠“凑”，且没有调用方（死代码） |
| `PacketCreator.giveForeignPirateBuff` / `giveForeignWKChargeEffect` | 手写包 + `skip()` 占位，字节数正确但语义不清晰（本次未改动，后续可统一到 `GiveRemoteBuff`） |

修正后的实现（`BuffPackets`）：

```csharp
// 唯一的远程包入口：掩码取 AllBuffStats，普通字段取 NormalBuffStats（按 ClientRemoteFields 顺序），
// 特殊字段由 ctx.EncodeSpecial 写（按 ClientSpecialFieldOrder）
public static Packet GiveRemoteBuff(int chrId, BuffParameter ctx);
```

远程 debuff 也走同一条链：`BuffParameterBuilder(MobSkill)` 把疾病放进 `AllBuffStats` / `NormalBuffStats`，
`SourceId` 用 `MobSkillId.getEncodedId()`（= `type | (level << 16)`，就是客户端疾病字段里那 4 字节），
`GiveRemoteBuff` 对 `DiseaseInfo.IsDisease` 的位只写这 4 字节（POISON 前面多 2 字节中毒值），
所以不再需要单独的 `GiveRemoteDebuff`。

`PacketCreator.giveForeignBuff` / `PacketCreator.giveForeignDebuff` 保留为兼容入口，内部都构造 `BuffParameter`
后调用 `GiveRemoteBuff`，所有既有调用点（DARKSIGHT、WIND_WALK、COMBO、SHADOWPARTNER、SOULARROW、AURA、MORPH、
StatEffect 的通用广播、疾病广播等）自动获得正确编码。

单疾病且无特殊字段时，新旧 `giveForeignDebuff` 的字节数一致——旧实现结尾的 `writeShort(0) + writeShort(900)`
恰好相当于客户端要读的「2 字节 Defense + 2 字节 delay」，区别只有 delay 从 900 变成 0；
但一次上两个以上疾病、或带特殊字段时，旧实现顺序错乱，只有新实现是对的。

有完整上下文（来源 id、时长、`ExtraValue0`）的调用点改成直接用 ctx 版本，
这样特殊 buff（DASH / SPEED_INFUSION / ENERGY_CHARGE / HOMING_BEACON / MONSTER_RIDING）
才能带上正确的来源与时长：

* `StatEffect.applyBuffEffect` 的 `isCombo()`、`activeMorphId > 0` 分支：`BuffPackets.GiveRemoteBuff(id, buffParameter)`；
* 只发单个状态的 `isDs()` / `isWw()` / `isShadowPartner()` / `isSoulArrow()` / `isAriantShield()` 分支保持原样（走简版重载）。

---

## 7. TemporaryStatType：客户端位表（本次用它替换了裸位号）

测试工程里的 `test/ServiceTest/Infrastructure/TemporaryStatType.cs` 是一张现成的位表（**枚举值 = 客户端掩码位**）。
本次直接**用它的位号替换了 `BuffStat` 各成员的值**：

* `BuffStat.getValue()` 现在返回客户端掩码位（0..89）；类型和成员名不变
  （成员名会被 `DataService`、玩家 buff 存档按名字反查，不能改名）；
* `BuffStat` 改成**真正的 `enum`**（以前是 `EnumClass` 用 class 模拟，因为每个状态带 value + IsFirst + name；
  现在只剩一个位号，直接 `值 = 位号`）：`getValue()` / `name()` / `IsFirst` / `ordinal()` 全部去掉，
  改用 `(int)stat` / `ToString()`；`From(string)` / `FromBit(int)` 这类静态方法移到
  新增的 `BuffStatUtils`（`TryParse` 忽略大小写、`FromBit` 做 `Enum.IsDefined` 校验、`Values` 给遍历用）；
  同一个位的多个名字（COMBO/SUMMON 等 7 组）就是枚举里的重复值，`ToString()` 取声明在前的那个；
* 为了让字段顺序表能写成名字，另外补了 5 个“客户端有、服务端暂无逻辑”的成员：
  `WINDBREAKERFINAL`(64)、`EVENTRATE`(67)、`EVANSLOW`(78)、`SOULSTONE`(81)、`NOTDAMAGED`(88)；
* 封包层不再需要做 value/IsFirst 换算：
  * `BuffPackets.GiveBuff` 按 `ClientFieldOrder`、`GiveRemoteBuff` 按 `ClientRemoteFields` 遍历输出，
    位号直接取 `(int)stat`，同一个位只写一份（`COMBO`/`SUMMON` 这类别名在枚举里是同一个值）；
  * 掩码统一由 `BuffPackets.WriteStatMask` 从位号生成；`PacketCreator` 的
    `writeLongMask` / `writeLongMaskFromList` / `showMonsterRiding` 以及某处 `buffmask >> 32` 都改成调它；
  * `Disease` 已并入 `BuffStat`（第 8 节），随类型一起消失的还有 `GetMaskBit` 和 `writeLongMaskD`。

### 7.1 位号发生变化的成员

把旧值（2 的幂 + IsFirst 换算）与新位号逐条比对，87 个成员里只有 3 个位号变了，其余完全一致：

| 成员 | 旧位号 | 新位号 | 说明 |
|------|--------|--------|------|
| `SLOW` | 65 | 32 | 原来与 `ELEMENTAL_RESET` 撞位，32 才是客户端的 SLOW |
| `MAGIC_SHIELD` | 66 | 79 | 原来与 `WIND_WALK` 撞位（放魔法盾时发出去的是风之步的位） |
| `MAGIC_RESISTANCE` | 67 | 80 | 原来落在客户端的 `EVENTRATE` 上 |

这三处就是 `BuffStat.cs` 里 “all incorrect buffstats” 注释所指，换成位号后自动修正；
又因为施加/取消 buff 的掩码都走同一套位号，不会再出现“加了但取消不掉”。
客户端的位 81 是 `SOULSTONE`（原注释 “// needs Soul Stone”），`BuffStat` 里没有对应成员，未新增。

### 7.2 名字不同但位相同的别名（不影响出包）

`SHADOW_CLAW`≈`SPIRITJAVELIN`(40)、`PUPPET`≈`BANMAP`(46)、`MAP_CHAIR`≈`DOJANGSHIELD`(62)、
`FINALATTACK`≈`SOULMASTERFINAL`(63)、`HOMING_BEACON`≈`GuidedBullet`(87)、`SPEED_INFUSION`≈`ENRAGE`(86)、
`EXP_BUFF`≈`EXPBUFFRATE`(74)、`COUPON_EXP1/DRP*` 与 `ITEM_UP_BY_ITEM`/`RESPECT_*`/`DEFENSE_*` 共用位。
这些只是叫法不同，掩码位和字段长度都一样，因此 `BuffStat` 侧不需要改（`BuffStat` 的名字还会被
`DataService` / 玩家 buff 存档按名字反查，不能随便改名）。

`TemporaryStatType` 里另外还有 `BuffStat` 没有的位：64 `WINDBREAKERFINAL`、67 `EVENTRATE`、
78 `EVANSLOW`、81 `SOULSTONE`、88 `NOTDAMAGED`（目前没有代码会设置它们）。

---

## 8. Disease 合并进 BuffStat

客户端本来就只有一张 TemporaryStat 表（`BuffStat.getValue()` 就是它的位号），服务端把疾病单独做成一个
`Disease` 类型，只会多出一层「2 的幂 + isFirst」的换算。本次合并掉：

* **`BuffStat` 补上疾病独有位**：`CURSE`(31)、`SEDUCE`(39)、`FISHABLE`(40)、`StopPotion`(75)、
  `StopMotion`(76)、`FEAR`(77)、`ZOMBIFY`(89)；`SLOW`(32)、`STUN`(17)、`POISON`(18)、`SEAL`(19)、
  `DARKNESS`(20)、`WEAKEN`(30)、`CONFUSE`(51) 本来就在。另加 `BuffStatUtils.FromBit(bit)` 供存档还原。
* **新增 `DiseaseInfo`**（`Application.Shared/GameProps/DiseaseInfo.cs`）保存 `BuffStat` 表达不了的元数据：
  疾病 → `MobSkillType` 映射、`GetBySkill` / `GetBySkillTrust`、怪物卡片缩写 `GetByAb`、`IsDisease`、`Diseases`。
* **`Disease` 类型删除**：`PlayerDisease(BuffStat Stat, …)`、`Player.Diseases: Dictionary<BuffStat, PlayerDisease>`；
  `StatEffect`（`cureDebuffs` / `DefenseState`）、`MobSkill`（`applyDisease`）、`Monster`、`Character`、
  `CarnivalFactory`、`DebuffCommand` 全部改用 `BuffStat`。
* **存档**：`DiseaseProto.diseaseOrdinal` → `diseaseBit`（值 = 客户端掩码位），
  加载时 `BuffStat.FromBit` + `DiseaseInfo.IsDisease` 过滤——旧存档存的是 `Disease` 声明序号（0..14），
  换算成位号后都不是疾病位，会被安全丢弃（疾病本来就是登录后由怪物重新施加的临时状态）。
* **包层**：`GetMaskBit` 与 `writeLongMaskD` 一并删除，疾病和 Buff 统一走 `WriteStatMask`；
  `giveDebuff` / `giveForeignDebuff` / `giveForeignSlowDebuff` / `cancelDebuff` / `cancelForeignDebuff`
  全部收 `BuffStat`，`cancelForeignFirstDebuff(cid, 1L << 50)` 这种裸掩码改成
  `cancelForeignDebuff(cid, BuffStat.ENERGY_CHARGE)`。

注意点：

* `ZOMBIFY = 89` 超出客户端包字段范围（82 个普通 + 7 个特殊 = 位 0..88），所以客户端不会显示丧尸图标，
  服务端“治疗减半”的逻辑照常；若要客户端也显示，需要实测确认它到底是位 89 还是位 46（客户端位 46 = BANMAP）。
* `giveForeignSlowDebuff` 仍写死位 75（客户端叫 `StopPotion`），本次没动（见第 9 节遗留 2）。
* `Disease.isFirst()` 那种「既是掩码半边又是优先标记」的双关语义随类型一起消失——查过全仓，
  它除了决定掩码落在哪一半之外没有任何地方消费。

---

## 9. 遗留 / 待确认

1. `ZOMBIFY` 的位号：**IDA 已验证客户端不存在位 89**
   （静态位常量只到 83，84..88 由 `sub_788156`/`sub_781D0E` 末尾的特殊循环动态构造，
   两个解码器的字段表都是 0..81 + 82..88，全二进制没有任何 `1<<89` 的构造），
   所以当前取 89 等于“只在服务端生效、客户端不显示”。
   位 46 那边也没有任何客户端逻辑消费者（只有通用特效派发 `sub_7889DA` / `sub_93E344`），
   “46 = BANMAP”还是“46 = zombify”在 exe 里无法判定（状态图标是 WZ 数据），要不要改回 46 需要游戏里看。
2. `PacketCreator.writeLongMaskSlowD` 写的是位 75。**IDA 已验证**：远程链只有 75/76/77 三个 4 字节字段、
   **没有位 32**；且通过 `CUserLocal::IsStun()`（读字段 +0x2E4 = 位 17）与
   `CUserLocal::IsWeakened()`（读 +0x4BC = 位 30）反查出疾病段是
   17/18/19/20(STUN/POISON/SEAL/DARKNESS)、30/31(WEAKEN/CURSE)、32(SLOW)，
   而 75/76/77 属于另一组（枚举标 StopPotion/StopMotion/FEAR）。
   也就是说现在发给其他玩家的是“StopPotion”的位；真正的 SLOW（位 32）在远程包里没有字段。
   客户端里也搜不到 `IsSlow` 之类的 getter（SLOW 只影响图标，减速本身是服务端在 Stat 包里的速度值），
   所以“远程不发 slow”不会影响实际减速——继续发位 75 还是干脆不发，建议游戏里对比图标后决定。
3. 本地包 `BuffPackets.GiveBuff` 的字段顺序问题本次已按第 5 节修复；
   `StatEffect.statups` 本身的顺序仍然“脏”，但只要不绕过 `BuffPackets` 直接手写包就不会有问题。
4. HOMING_BEACON 的额外 4 字节（目标怪物 objectId）取自 `BuffParameter.ExtraValue0`，
   与 `StatEffect.applyBeaconBuff` 的用法一致。
5. 仍然手写 `GIVE_FOREIGN_BUFF` 的方法还有 `showMonsterRiding`（无调用方）、
   `giveForeignPirateBuff`（DASH/SPEED_INFUSION）、`giveForeignWKChargeEffect`（WK_CHARGE）、
   `giveForeignSlowDebuff`（减速），字节数都能对上，后续可以逐步并入 `BuffPackets`。
6. `BuffStat` 保留为对外（游戏侧）状态类型，只是值语义换成了客户端位号。
   若连类型也要换成 `TemporaryStatType`，还要处理：名字被
   `DataService`（`BuffStat.From(name)` + 玩家 buff 存档）反查、被 `Buff.cs`/命令等当行为开关
   （`SUMMON`/`PUPPET`/`COUPON_*`），以及 `Disease` 仍是老编码。
7. `Disease` 已并入 `BuffStat`（第 8 节），因此“两套编码风格不一致”的问题不复存在；
   仍待确认的是 `ZOMBIFY` 的位号（89 vs 46）和 `giveForeignSlowDebuff` 写死的位 75。
8. **远程字段顺序**（本轮修掉的一个真错）：早期实现让远程包复用本地包的 `ClientFieldOrder`，
   理由是“两者只差一部分字段”。实际客户端是**两个各自写死顺序**的函数：
   `sub_788156` 依次读 `7,21,22,17,20,19,30,31,18,26,10,16,33,49,39,40,46,50,62,51,53,54,55,56,59,60,66,73,75,76,77`，
   而 `sub_781D0E` 读 `7,8,9,10…`。两者只有字段集合重合，顺序不同——套用本地顺序会从第 2 个字段起就错位。
   现已拆成 `ClientRemoteFields`（第 3 节那张表）单独维护，核对方法见第 10 节末尾。
   同时修掉 `GiveRemoteBuff` 里的两个笔误：opcode 写成了 `GIVE_BUFF`（应为 `GIVE_FOREIGN_BUFF`），
   以及字段命中判断漏了 `!`（`|| !ctx.NormalBuffStats.TryGetValue(...)`），后者会让所有普通字段都不写。

---

## 10. IDA 复查记录（位表语义锚点）

`Angel.exe`（v83）里能直接读出语义的“位号 → 行为”对照，用来反证我们的位表：

| 位 | 客户端证据 |
|----|-----------|
| 7 SPEED / 21 COMBO / 26 SHADOWPARTNER / 33 MORPH / 49 GHOST_MORPH / 50 AURA / 82 ENERGY_CHARGE | `CUser::OnTemporaryStatChanged` 里的掩码常量：`dword_BF1218`=7、`dword_BF1248`=21、`dword_BF1208`=26、`dword_BF11F8`=33（→ `CUser::SetMorphed`）、`dword_BF11E8`=49（→ `CUser::SetGhostState`）、`dword_BF1378`=50、`dword_BF1238`=82 |
| 17 STUN | `CUserLocal::IsStun()` 读 SecondaryStat 的 +0x2E4 字段（= 我们表里位 17 的 value 偏移），调用方有 `HandleXKeyDown` / `UseFuncKeyMapped` / `IsImmovable` |
| 30 WEAKEN | `CUserLocal::IsWeakened()` 读 +0x4BC（= 位 30 的 value 偏移），调用方有 `Jump` / `FallDown` / `TryDoingWings` / `DoActiveSkill_BoundJump` |
| 33 MORPH / 49 GHOST_MORPH | 字段偏移再次对上：位 33 value=0x530（`SetMorphed` 用 0x528/0x530）、位 49 value=0x554（`SetGhostState` 用 0x54C/0x554） |
| 85 / 87 | `sub_98385D` / `sub_983921` 用 `sub_78D977(_, 3)`＝位 85 判骑乘、`(_, 5)`＝位 87 判导向 |

由此可确认：`BuffStat` 的位号表与客户端一致；疾病段就是 17..20 / 30..32，75..77 是另一组
（`TemporaryStatType` 标的 StopPotion/StopMotion/FEAR），并且位 89 在客户端不存在。

位号（field offset）提取方法：`sub_781D0E` 每个字段块调用三个 setter（value/skill/time），
setter 里 `*(obj + V) = fuse(value, obj + V - 8)`，所以“value 偏移 = V、secure key = V - 8”；
读取方一律走 `_ZtlSecureFuse(ptr = obj + V - 8, value = *(obj + V))`，据此能反查某位的消费者。

远程字段顺序的核对方法（第 9 节遗留 8）：两个解码函数引用的是**同一批掩码常量**
（`0xBEFB00..0xBF0000`，间隔 0x10，运行时由掩码对象填充，静态镜像里全是 0，所以读不到位号）。
把 `sub_788156` 里 31 个掩码常量按地址映射到 `sub_781D0E` 里同一常量的位置，就得到远程每一位的位号/状态名；
再用两处独立证据交叉验证：远程第 4 个字段写槽位 185（`0x2E4`，正是 `CUserLocal::IsStun` 读的 STUN 槽位），
以及 POISON 是唯一的 `Decode2 + Decode4` 结构（远程链第 9 位、本地链第 19 位）。
另外 `sub_98385D` 在 `sub_788156` 返回后还会读一个 `Decode2`，就是第 1 节流程图里的尾部 delay
（`GiveRemoteBuff` 结尾的 `writeShort(0)` 由它消费）。

---

## 11. 改动文件

| 文件 | 说明 |
|------|------|
| `src/Application.Shared/GameProps/BuffStat.cs` | 改成**真正的 enum**，值 = 客户端掩码位（含疾病位与 5 个“仅用于字段表”的成员、7 组同值别名）；不再继承 `EnumClass` |
| `src/Application.Shared/GameProps/BuffStatUtils.cs` | **新增**：`TryParse`（按名字，含别名）/ `FromBit`（按位号，存档用）/ `Values` |
| `src/Application.Shared/GameProps/DiseaseInfo.cs` | **新增**：疾病 → `MobSkillType` 等元数据（原 `Disease` 里 `BuffStat` 表达不了的部分） |
| `src/Application.Shared/GameProps/Disease.cs` | **删除**（类型并入 `BuffStat`） |
| `src/Application.Core/Channel/Net/Packets/BuffPackets.cs` | `GiveBuff`（本地顺序）+ `GiveRemoteBuff(int, BuffParameter)`（远程顺序，31 个普通字段 + 7 个特殊字段）；掩码统一 `WriteStatMask`；`ClientFieldOrder` / `ClientRemoteFields` / `ClientSpecialFieldOrder` 三张表都用 `BuffStat` 成员表达，不再有裸位号 |
| `src/Application.Core/tools/PacketCreator.cs` | `giveForeignBuff` / `giveForeignDebuff` 保留为兼容入口，内部构造 `BuffParameter` 后走 `BuffPackets.GiveRemoteBuff`；`writeLongMask*` / `showMonsterRiding` / `buffmask` 改用位号掩码；debuff 系列改收 `BuffStat`，删掉 `writeLongMaskD` / `cancelForeignFirstDebuff` |
| `src/Application.Shared/Battle/Skills/MobSkillId.cs` | **新增** `getEncodedId()`（`type \| (level << 16)`）：疾病字段/特殊字段“来源”的统一算法 |
| `src/Application.Core/Game/Gameplay/BuffParameter.cs` | `BuffParameter`（掩码 + 普通/特殊字段 + 来源/时长上下文）与 `EncodeSpecial`（7 个特殊字段 82..88 的写入） |
| `src/Application.Core/Game/Players/Disease.cs` (Player 分部) | `Diseases` 改成 `Dictionary<BuffStat, PlayerDisease>`，疾病施放/取消走统一掩码 |
| `src/Application.Core/Game/Players/Buff.cs`、`Game/Players/Character.cs`、`Channel/Net/Handlers/MonsterCarnivalHandler.cs`、`Server/StatEffect.cs`、`Server/life/MobSkill.cs` | 跟着改成 `BuffStat` / `DiseaseInfo`；`BuffStat` 由引用类型变值类型后，`BuffStat?` 传参处补 `.Value` |
| `src/Application.Core/Game/Players/PlayerProps/PlayerDisease.cs` | 成员类型 `BuffStat` |
| `src/Application.Core/Server/StatEffect.cs` | `cureDebuffs` / `DefenseState` / `GetByAb` / `GetBySkillTrust` / `hasDisease` 改用 `BuffStat` + `DiseaseInfo`；`isCombo`/`activeMorphId` 用带上下文的 `GiveRemoteBuff` |
| `src/Application.Core/Server/life/MobSkill.cs` | 疾病变量与 `applyDisease` 改 `BuffStat`；`DefenseState` 走 `DiseaseInfo` |
| `src/Application.Core/Channel/Services/DataService.cs`、`src/Application.Protos/Model/Disease.proto`、`src/Application.Core.Login/Models/PlayerBuffSaveModel.cs` | 存档字段 `diseaseOrdinal` → `diseaseBit` |
| `src/Application.Core/Game/Life/Monster.cs`、`Game/Players/Character.cs`、`Server/partyquest/CarnivalFactory.cs`、`Game/Commands/Gm3/DebuffCommand.cs` | 改 `BuffStat` / `DiseaseInfo` |
| `test/ServiceTest/Infrastructure/TemporaryStatType.cs` | 枚举未改；对照用例改成只遍历 `BuffStat`（疾病也已在其中） |

验证：`Application.Core`、`Application.Host`、`ServiceTest` 三个工程均 0 错误；
脚本比对新旧位号，87 个成员里只有第 7.1 节的 3 个发生变化；
`ClientFieldOrder` 的 82 个位号已核对为 0..81 的正确排列；
`ClientRemoteFields` 的 31 项已逐项核对为 IDA 的位号序列与读取方式
（`Byte` / `Short` / `Int` / `Flag` / `ShortThenInt`）完全一致，且确认它**不是** `ClientFieldOrder` 的子序列。

---

## 12. 包尾那个 short（delay）和 point 到底是什么

两个包的最后都有一个 short，客户端读出来之后作为 `CUser::OnTemporaryStatChanged` 的第 2、3 个参数：

```c
// 0x93CDBC  CUser::OnTemporaryStatChanged(UINT128 mask, int tDelay, int nPoint)
//   0x93E341  retn 18h  → 24 字节参数 = 16(mask) + 4(tDelay) + 4(nPoint)，签名由此确认
```

| 包 | 读取点 | 传参 |
|----|--------|------|
| 远程 | `sub_98385D` 0x983899 `Decode2` | `push 0`(0x9838A3，point) + `push delay`(0x9838A5) → `call [vtable+0x20]`(0x9838B7) |
| 本地 | `OnTemporaryStatSet` 0xA2032F `Decode2` | `v35 = delay`、`v36[0] = 0`(0xA20588) → 同一个虚函数(0xA205A4) |

* **point**：**不在包里**。两条路都是客户端自己写死 0（0x9838A3 / 0xA20588），
  在 `CUser::OnTemporaryStatChanged` 里只用于 `cmp [ebp+arg_14], 0` 这类分支
  （0x93CE13、0x93DC92、0x93DEB2）。服务端没有可写的位置，也不需要处理。
* **delay**：在 `CUser::OnTemporaryStatChanged` 里只出现一次 —— 结尾
  `sub_93E344(this, tDelay, 0)`（0x93E328 push 0、0x93E329 push delay、0x93E32E call）；
  在 `sub_93E344` 里也只出现一次：0x93E882 `lea ebx, [eax + tDelay]` → 0x93E8A8 `[记录+4] = ebx`，
  0x93E8A2~0x93E8AD 处若小于 1 会被改成 1。
  那条记录存在 `CUser+0x2A54` 的哈希表里（`sub_94457E` 取位、`sub_47CCB7` 插入）。
  也就是说 delay 只是**把该状态记录里的计时基准整体后移 tDelay**，属于客户端效果/图标侧的记账；
  状态数值本身在 `CUser::OnTemporaryStatChanged` 里是立即生效的。发 0 时客户端取 1（= 立即），
  所以“始终发 0”是合理的。

补充：旧代码里 debuff 发 900、个别技能发 900/1000，是 OdinMS 一路抄下来的数，
从客户端看不出单位（没有证据说明是毫秒）。本次重构后远程 debuff 的 delay 从 900 变成了 0；
如果要在意这个差值，把 delay 做成 `BuffParameter` 的一个字段透传即可（暂未做）。

另有一个未定论的点：本地包在 `OnTemporaryStatSet` 里调用完 `OnTemporaryStatChanged` 之后，
还有一处**条件读取** —— 0xA205B9 `if (sub_77DC78()) { v21 = Decode1(); CUserLocal::SetSecondaryStatChangedPoint(v21); }`，
即掩码满足某组位（`dword_BF0010` = 0xBEFF50..0xBEFFF0 那一批掩码的聚合）时，包尾还会多 1 字节。
服务端从来没写过这 1 字节（`GiveBuff` 也一样），客户端读越界通常只拿到 0。
要不要补，得先把 `sub_77DC78` 判定的是哪几个位确定下来。
（本机沙箱下 `dotnet test` 的 testhost 起不来，测试用例只做了编译验证。）
