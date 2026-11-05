# CalcDamage 随机种子与暴击同步（v83）

> IDA 数据库：`` `Angel.i64 ``（v83，基址 `0x400000`）
> 服务端：`src/Application.Core/Tools/PacketCreator.cs`（`getCharInfo`）、
> `src/Application.Core/Channel/Net/Handlers/AbstractDealDamageHandler.cs`（`canCrit` / `isCrit` 推断）
> 关联文档：`docs/ida/CUserRemote_OnAttack_暴击伤害编码_分析与实现.md`、
> `docs/ida/CUserLocal_攻击包_客户端发包结构_分析.md`
> 客户端补丁工程：`BeiDou-ijl15`

> **状态**：曾按"服务端复刻客户端随机流"（下称方案 C）实现过一版，因不可控已**全部回滚**。
> 现改为**由客户端在攻击包里携带暴击位**（方案 D），已实现，见 §6.1。
> 本文档保留完整的客户端行为分析，作为该补丁的依据与参考。

---

## 1. 结论速览

| 项目 | 客户端实测 |
|------|-----------|
| 是否存在随机种子 | **是**，`SET_FIELD` 里的 3 个 dword，由服务端生成 |
| 注入点 | `CStage::OnSetField` **0x776020** → `CalcDamage::SetSeed`（0x77615C）+ `CWvsContext::SetActionRndSeed`（0x776170） |
| 注入次数 | **整个会话一次**：只有 `bCharacterData != 0` 的那次 `SET_FIELD`（即 `getCharInfo`）才读这 3 个 dword |
| 算法 | `CRand32::Random` **0x4165EC**，三个 32 位状态的移位/异或 |
| 暴击用的哪条流 | `CalcDamage` 的第 0 个 `CRand32`（`m_RndGenForCharacter`，CUserLocal+0x3430） |
| 每次伤害计算抽几次 | **恰好 7 次**，之后在 7 个值里按 `index % 7` 循环复用（`PDamage` 0x78E001、`MDamage` 0x791632） |
| 攻击包里有没有暴击位 | **没有**（186/187/188 三个发包循环只写 `Encode4(target[6+i])`） |
| 包内有没有 RNG 状态 / 序号 | **没有**，唯一像"同步标记"的只有每目标尾部的 mob 模板 CRC32（`sub_6711AC`） |
| 会话内是否有重同步点 | **没有**。`SetSeed` 全程序只有一个 xref |

**判定：种子确实是"共享"的（服务端发、客户端用、算法相同），客户端侧的随机流本身也不会
漂移（每个目标固定抽 7 个）。难点在于暴击读的是 `arr[v347 % 7]`，而 `v347` 的累加次数
由 `PDamage` 内部的 37 处分支决定，其中多处置于服务端看不见的客户端本地状态上。
因此方案 C 无法做到逐次精确，已放弃。**

---

## 2. 种子协议

### 2.1 服务端发的 3 个 dword 就是种子

`CStage::OnSetField`（0x776020）的读取顺序：

```c
v105 = Decode1();          // ① 未使用（服务端写 1）
v102 = Decode1();          // ② bCharacterData（服务端写 1）
v103 = Decode2();          // ③ 通知消息条数（服务端写 0）
if (v103) { ...DecodeStr × v103... }

if (v102)                  // 0x77612F
{
    v110 = Decode4();      // 0x776133
    a2   = Decode4();      // 0x77613D
    v109 = Decode4();      // 0x776147
    CalcDamage::SetSeed(v2 + 13360, v110, a2, v109);      // 0x77615C  → CUserLocal+0x3430
    CWvsContext::SetActionRndSeed(v2 + 13620, v110, a2, v109); // 0x776170 → CWvsContext+0x3534
    CWvsContext::OnSetLogoutGiftConfig(...);
    CharacterData::Decode(...);
}
else { /* 换图分支：读 mapId、portal、HP...，不读种子 */ }
```

`13360 = 0x3430` 正是 `CalcDamage` 在 `CUserLocal` 里的偏移（三个攻击函数都用
`lea ecx, [local+3430h]` 传 `this`）；`13620 = 0x3534` 是另一个独立 `CRand32`（动作随机）。

> 换图包 `getWarpToMap` 写的是 `bCharacterData = 0`，客户端走 else 分支**不读种子**，
> 所以 RNG 状态跨地图连续延续，不会重置。

### 2.2 种子的落地方式（有掩码，可逆）

```c
// CalcDamage::SetSeed(this, a, b, c)  0x78DDB0
for (i = 0; i < 6; i++)                     // 6 个 CRand32 全部用同一组种子
    CWvsContext::SetActionRndSeed(this + 32 * i, a, b, c);

// CWvsContext::SetActionRndSeed(this, a, b, c)  0x7345E2
s1 = past_s1 = a | 0x100000;
s2 = past_s2 = b | 0x1000;
s3 = past_s3 = c | 0x10;
```

**注意三个 OR 掩码**：客户端真正使用的状态是 `a|0x100000, b|0x1000, c|0x10`，
不是原始值，必须做同样的掩码才能复刻。

`CalcDamage` 结构里 6 个 `CRand32` 依次是 `m_RndGenForCharacter`(0)、
`m_RndForCheckDamageMiss`(0x20)、`m_RndForMortalBlow`(0x40)、`m_RndForSummoned`(0x60)、
`m_RndForMob`(0x80)、`m_RndGenForMob`(0xA0)。它们初始状态相同，但各自由不同功能消耗，
所以只会在一开始一致。

### 2.3 `CRand32::Random`（0x4165EC）

```c
s1 = *this, s2 = this[1], s3 = this[2];
this[3] = s1; this[4] = s2; this[5] = s3;      // past_*（给 ZtlSecure 用）
n1 = ((s1 & 0xFFFFFFFE) << 12) ^ (((s1 >> 13) ^ (s1 & 0x7FFC0)) >> 6);
n2 = ((s2 & 0xFFFFFFF8) << 4)  ^ (((s2 >> 2)  ^ (s2 & 0x3F800000)) >> 23);
n3 = ((s3 & 0xFFFFFFF0) << 17) ^ (((s3 >> 3)  ^ (s3 & 0x1FFFFF00)) >> 8);
*this = n1; this[1] = n2; this[2] = n3;
return n1 ^ n2 ^ n3;
```

**三条状态是一一对应的：s1→n1、s2→n2、s3→n3**（0x4165FC–0x416661 可逐条核对）。
纯整数运算，无平台相关行为。

---

## 3. 谁在消耗这条流

| 消耗者 | `this` | 说明 |
|--------|--------|------|
| `CalcDamage::PDamage` **0x78E003** | CUserLocal+0x3430 | **入口一次性抽 7 个**，之后只用 `v334[n % 7]` |
| `CalcDamage::MDamage` **0x791634** | 同上 | 同样入口抽 7 个、7 元素循环复用 |
| 其余大量 xref | `dword_BEDD38`、`obj+0x80`(RndForMob)、`obj+0x3534` 等 | 都是别的 `CRand32` 实例 |

第 0 条流**只被"玩家打怪的伤害计算"消耗**，每次调用固定推进 7 次；
`PDamage` / `MDamage` 内部再没有任何 `Random` 调用。

`PDamage` 的 4 个调用点：

| 调用点 | 所在函数 |
|--------|----------|
| 0x951E42 | `CUserLocal::TryDoingMeleeAttack` |
| 0x954CF0 | `CUserLocal::TryDoingShootAttack` |
| 0x95F3ED | `sub_95F135`（`CUserLocal::Update` 里构造攻击包并发 `COutPacket(0x2F)`） |
| 0x95689B | `CUserLocal::TryDoingMagicAttack` → 实际调 `MDamage` |

> 每个**目标**调一次，所以一次攻击包消耗 `7 × numAttacked` 个随机数。
> 另外每近战/远程/魔法攻击还会从 **+0x3534**（动作随机）抽 1 个用于选攻击动作，
> 那是另一条流，不影响暴击。

---

## 4. 暴击判定

### 4.1 通用暴击（PDamage 0x79014E–0x7901B5）

```c
v232 = v347 % 7;                         // v347 = 复用下标，初值 0
++v347;
if (v334[v232] % 10000000 * 1e-5 < v337) // v337 = 暴击率(%)
{
    *(critArray + 4 * j) = 1;            // 0x7901A2：暴击标记
    damage = damage + (v339 - 100) * 0.01 * base;  // v339 = 暴击伤害%
}
```

- `% 10000000`（`off_989680` = 10,000,000）× `dbl_AFE838`(≈1e-5) ⇒ 取值落在 `[0, 100)`，
  与 `v337`（百分数）比较，语义就是"暴击率 %"。
- 暴击倍率 = `(暴击伤害% - 100)%` 的额外伤害，默认 100% 额外（≈ 2 倍）。
- **前置门控**（0x790113）：
  ```asm
  0x790113  cmp [ebp+var_40], 0    ; 暴击率
  0x790116  jle 0x7901B8           ; <= 0 整段跳过，v347 不推进
  0x79011C  cmp [ebp+var_34], 0    ; 暴击伤害
  0x79011F  jle 0x7901B8
  ```
- 暴击标记写进 `critArray`（target 结构 +0x54），**不写包**（见 §5.3）。

### 4.2 暴击率与暴击伤害的来源

`PDamage` 里 `var_40`（暴击率）/ `var_34`（暴击伤害）有两条初始化路径，
**都是覆盖写，不是累加**：

**① 普通攻击（远程武器）** —— 0x78E101–0x78E11A：

```c
sub_765095(a2 /*CharacterData*/, nItemID /*武器*/, a11, &var_40, &var_34);
```

`sub_765095`（0x765095）**按武器类型选技能**，再取该技能的等级数据：

| 武器 | 技能 |
|------|------|
| 弓(45) | 3000001 |
| 弩(46) | 13000000 |
| 拳套(47) | 4100001 |
| 火枪(48 分支) | 14100001 |

```c
*a4 = SkillLevelData[+0x0F4];   // 暴击率
*a5 = SkillLevelData[+0x0D0];   // 暴击伤害
```

> **`prop` 不是通用暴击率**。它只是技能里的整数字段，绝大多数技能用它表示状态触发概率
> （常见值 100）。只有上面这几个 **Critical 系列技能**的 `prop` 才有暴击语义。
> 另外 `sub_765095` 的 `a3` 必须为 1 才取数；近战武器没有对应被动暴击技能，`*a4 = 0`。

**② 技能攻击** —— 0x78E11F–0x78E15D：取当前技能的 `SkillLevelData + 0xF4` / `+0x1B0`。

**③ Sharp Eyes 加成** —— 0x78E160–0x78E1CA，**来自 `SecondaryStat`，即 buff**：

```c
ebx = a4;                                    // SecondaryStat*（0x78E02A）
eax = Fuse(ebx + 0x5DC) >> 8;                // 高字节 = 暴击率加成
if (eax <= 0) eax = 0;
if (eax >= 100) eax = 100;                   // 上限 100
var_40 += eax;                                // 0x78E18B
var_34 += (Fuse(ebx + 0x5DC) & 0xFF);         // 低字节 = 暴击伤害加成
```

- 打包值 = `(x << 8) | y`，与服务端 `StatEffect.cs:518` 的
  `statups[BuffStat.SHARP_EYES] = x << 8 | y` **一致**。
- 因为是 `SecondaryStat`（buff 下发链），**队友只要身上有 buff 就享受**，
  与是否学会该技能无关。
- WZ 参考：3121002 满级 `x = 15`、`y = 140`。

> **注意区分**：`PDamage` 开头（0x78DFB1–0x78DFEF）确实按职业查了 3121002 / 4111002，
> 但那两个值是 **`var_70` / `var_74`**，一直到 0x79021A 才使用，读的是
> `SkillLevelData + 0x148`，用于 **Shadow Partner 后半段伤害**，**与暴击无关**。

### 4.3 技能强制暴击（PDamage 0x78F9BE–0x78F9FA）

技能表里某一类（`case` 命中后走 `LevelData + 0x13C`）会直接判暴击：

```c
if (v334[v347 % 7] * 1e-5 <= SkillLevelData[暴击率])  // 0x78F9B8 fcomp
    { v347++;  *(critArray + 4*j) = 1;  damage *= (LevelData+0x13C + 100)%; }
```

### 4.4 魔法暴击（MDamage 0x791D80–0x791DFD）

旧版本把"客户端魔法是否存在暴击"列为未验证。本次确认：**魔法有客户端暴击**，
结构与物理一致：

```c
if (critRate > 0 && critDamage > 0)          // 0x791D80 / 0x791D88
{
    idx = v125 % 7; ++v125;                  // 0x791D8E
    if (v103[idx] % 10000000 * 1e-5 < critRate)   // 0x791DC1
    {
        bonus = (critDamage <= 200) ? critDamage - 100 : critDamage - 200;
        critArray[j] = 1;                    // 0x791DE7
        damage = (bonus * 0.01 + 1.0) * damage;
    }
}
```

（`MDamage` 里被 Hex-Rays 命名成 `nDragonFury` 的参数其实是**暴击数组**，
对照 `if (!a6[81] || *(nDragonFury + a13))` 与 `PDamage` 的
`if (!a6[81] || *(a17 + 4 * nItemID))` 即可确认。）

### 4.5 发包侧：暴击标记确实不上线

三个发送循环仍然是"纯伤害"：

```asm
; 近战 0x952B1D    mov eax,[ebp+arg_8] / push dword ptr [eax] / call Encode4   ; [edi+18h] 起
; 远程 0x955423    push dword ptr [edi]      / call Encode4                    ; [esi+18h] 起
; 魔法 0x957056    push dword ptr [edi]      / call Encode4                    ; [esi+18h] 起
```

`edi/esi + 0x18` 就是 `target[6]`（伤害数组），`target[21]`（+0x54 暴击数组）没有任何读取。

---

## 5. 为什么方案 C（服务端复刻随机流）被放弃

### 5.1 可行的那一半

1. 种子是服务端自己生成的，初始状态完全一致；
2. `CRand32` 算法无歧义，可 bit 级复刻；
3. 第 0 条流只被 `PDamage`/`MDamage` 消耗，**每次固定 7 次**：

```c
push 7; pop edi;
do { *esi++ = CRand32::Random(this); } while (--edi);   // 0x78E001 / 0x791632
```

**这段在任何分支之前无条件执行**，函数体内再没有第二个 `Random` 调用，
所以随机流**不会累积漂移**——分支只影响"复用下标指向哪一个值"。

### 5.2 卡住的另一半：`v347` 的推进次数

`PDamage` 内 `inc [ebp+var_4]` 共 **37 处**，全部可达暴击判定
（对 416 个基本块做反向可达性分析，无死路径）：

| 站点 | 门控条件 | 服务端能否判断 |
|------|---------|--------------|
| 0x78E47E | `MobStat[0xA0] == 0`（未冰冻） | ✅ |
| 0x78E4FE | 3221007 且 `a11 == 1` | ✅ |
| 0x78E575 / 0x78E5EA | `SecondaryStat` 本地缓存 | ❌ |
| 0x78E727 | `SkillLevelData+0xDC != 0` | ❌ |
| 0x78E7EA | `a14 == NULL`（无技能） | ✅ |
| 0x78E890 / 0x78E8F2 / 0x78E916 | `SecondaryStat+0x348` | ❌ |
| 0x78E986 | 技能 ID 白名单 | ✅ |
| 0x78EAB3…0x78FDC2（20 个） | 武器类型 switch | ✅ |
| 0x78FF93 | 技能 ID 白名单 | ✅ |
| 0x790163 | 暴击判定自身 | ✅ |

❌ 的那些是客户端本地缓存字段，服务端拿不到。加上 `critRate` 本身依赖
"`prop` 是暴击率"这一前提（只对 Critical 系列成立），任何一处判断偏差都会
**静默**产生错误，且服务端无法察觉——这正是"不可控"。

### 5.3 已回滚

方案 C 曾经实现过（`ClientRand32` / `CalcDamageRandom` / `CalcDamageFlow` /
`Player.CalcDamageRandom` / `getCharInfo` 存种子），**已全部回滚**，
工作区无相关残留。记录于此仅为日后参考。

---

## 6. 备选方案对照

| 方案 | 做法 | 状态 |
|------|------|------|
| A. 维持现状 | 只有高伤才显示暴击 | 保留为原版客户端的兜底 |
| B. 概率补全 | 服务端 RNG 按暴击率掷一次 | 未采用：频率对、单次仍假 |
| C. RNG 重放 | 复刻 `CalcDamage` 随机流与 `v347` | **已放弃并回滚**，见 §5 |
| **D. 改客户端** | 发包时把 `target[21]` 的暴击位 OR 进伤害 bit31 | **已实现** |

### 6.1 方案 D 的实现

**客户端**（补丁工程 `BeiDou-ijl15`，`Client::EnableCritFlagInAttackPacket`）：
在近战/远程/魔法三处发包循环的 `Encode4` 之前，把暴击位合并进伤害值。

```asm
; 近战 hook 于 0x952B22（原 lea ecx,[ebp-88h]）
lea ecx, [ebp - 88h]
mov eax, [ebp + 10h]        ; &target[6+i]
mov edx, [eax]              ; damage
mov eax, [eax + 3Ch]        ; crit[i]（&target[6+i] + 0x3C == &target[21+i]）
shl eax, 31
or  edx, eax
push edx
jmp 0x952B28                ; 原 call Encode4
```

远程（0x955425）与魔法（0x957058）结构相同，只是 `edi` 直接指向 `&target[6+i]`，
且栈帧上的 outPacket 偏移不同。

**服务端**（`DamageLine.Decode`，由 `parseDamage` 调用）——同时兼容两种客户端：

```csharp
if (rawDamage < 0)   // 补丁客户端：bit31 = 暴击位
    return new DamageLine(true, (rawDamage & int.MinValue) != 0, rawDamage & int.MaxValue);
return new DamageLine(false, false, rawDamage);   // 原版客户端：伤害恒 >= 0
```

原版客户端的伤害不可能为负（`PDamage` 0x790586 用 `max(1.0, min(raw, dbl_AFE8A0))`
钳制后才写包），因此负数只可能来自补丁客户端。未带标记时退回
`canCrit && damage > hitDmgMax` 推断，并在原版客户端下保留 SNIPE 的固定伤害生成。

> `MESO_EXPLOSION`（`parseMesoExplosion`）走独立的发包路径，客户端未打补丁，保持原样。

---

## 7. 未确认 / 待验证

1. `SkillLevelData` 偏移 `+0xF4`（暴击率）、`+0x1B0` / `+0xD0`（暴击伤害）对应的
   WZ 字段名，未从 WZ → 结构体的填充代码逐字段核对。已按数值语义验证：
   3000001 的 `prop` 为 12~18、4100001 为 21~23，符合暴击率档位。
2. `0x78E948` 判定的四个技能 ID 在 IDA 伪代码里被渲染成指针，实际数值按
   `cmp ebx, imm` 读取为 `0x111AEE / 0x12A18F / 0x14282B / 0x12A191`，未在游戏内验证。
3. `PDamage` 的 `a11`（第 9 个参数，0x78E647 处 `cmp arg_24, 3`）在客户端的取值来源未确认。
4. `sub_95F135`（`Update` 内发 `COutPacket(0x2F)` 的攻击）的触发条件未展开。
5. Sharp Eyes 的 `x`/`y` 与 `SecondaryStat + 0x5DC` 高/低字节的对应关系按汇编确认，
   但 `x`/`y` 在 WZ 里的字段名未核对填充代码。

> 本次分析**未修改服务端逻辑**。方案 C 的实现已全部回滚，本文档保留作为客户端行为参考。
