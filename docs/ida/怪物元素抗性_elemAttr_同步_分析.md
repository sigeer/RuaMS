# 怪物元素抗性（elemAttr）同步分析

> IDA 数据库: ``Angel.idb``（v83，基址 `0x400000`）
> 服务端: `src/Application.Core/Game/Life/Monster.cs`（`getMonsterEffectiveness` / `setTempEffectiveness`）
> 关联文档: `docs/ida/CMob_OnStatSet_分析.md`（0xF2/0xF3 掩码与 per-status 字段布局）
> 
> 触发背景：服务端把 `setTempEffectiveness` 从 `MobClearSkillService` 改成本地到期表后，
> 需要确认这个"临时元素抗性"到底有没有发给客户端、走的是哪条通道。

---

## 1. 结论速览

| 项目 | 值 |
|------|-----|
| 客户端能否读服务器端的元素抗性 | **不能**。客户端只用 `MobStat+0x8..0x1C` 这 5 个 dword 参与伤害判定，而它们只来自 `CMobTemplate+0x118` |
| 该字段的来源 | `CMobTemplate`（WZ `Mob.wz/xxxxxxx.img/info/elemAttr`），在 `MobStat::SetFrom` / `SetFromWhenDoom` 里从模板 `memcpy` |
| 能否通过封包改 | **只有两个特例**：`MobStat::AdjustDamagedElemAttr` (**0x78C7AB**)，仅当 `CMob+0x250 == 2121003 / 2221003` 时把 ICE/FIRE 两条置成 WEAK |
| 触发路径 | `CMob::ProcessStatSet` (**0x671B71**)，条件是 mask 命中 POISON（`dword_BEFA20`，位 9） |
| 恢复路径 | `sub_78C7E8` = `memcpy(MobStat+4, template+0x118, 0x20)`，同样在 POISON 位命中时执行 |
| 也就是说 | 元素抗性**搭 POISON 的车**同步：0xF2 包里带 POISON 项 = 客户端把这两条改成 WEAK；0xF3 取消 POISON = 客户端从模板重新拷贝，恢复原值 |
| 结论 | 服务端那份 `MonsterStats.resistance` 的**临时覆盖**必须同时以 POISON 状态的形式发给客户端，否则客户端算出的伤害和服务端校验用的 `calcDmgMax` 会对不上 |

---

## 2. 客户端结构

### 2.1 涉及的两个对象

```
CMob                       // 0x662884 Init，大小 1352
  +0x188  CMobTemplate*    // 模板指针（非 link 时 = +0x188，link 时取 +0x18C）
  +0x1A0  MobStat          // 内嵌，0x208 字节
  +0x250  int              // **注意：这是 `MobStat+0xB0` 同一个地址**，见 §2.3

CMobTemplate
  +0x118  4 dword          // 受击元素抗性（elemAttr 解码结果）
  +0x250  4 dword          // 普通元素抗性（未被本仓库的服务端使用）
```

`CMob::Init`（**0x662884**）在末尾调用 `MobStat::SetFrom(this+416 /*0x1A0*/, *(this+0x188))`，
即怪物出生时把模板抗性整份拷进 `MobStat`。

### 2.2 `MobStat::SetFrom`（**0x789DC7**）

```c
DWORD MobStat::SetFrom(char* this, int template) {
    memset(this, 0, 0x208);
    *this = ...;                                  // maxHP 之类
    memcpy(this + 4, template + 0x118, 0x20);     // ← 元素抗性 4 dword = 8 种元素
    ...
}
```

`MobStat+4..+0x24` 是 8 个 dword，对应 8 个元素。`sub_790782` 的解码语义（见 §2.4）说明每个 dword 的取值是
`0=NORMAL / 1=IMMUNE / 2=STRONG / 3=WEAK`。

`MobStat::SetFromWhenDoom`（**0x789EFD**）同样做一次 `memcpy(this+4, template+0x118, 0x20)`。

### 2.3 偏移确认：`CMob+0x250` 就是 `MobStat+0xB0`

`ProcessStatSet` 里那段"从模板恢复"的代码：

```
0x671BE7  cmp dword ptr [ebx+2A4h], 0     ; link 标志
0x671BF0  mov eax, [ebx+18Ch]             ; link 模板
0x671BFA  mov eax, [ebx+188h]             ; 或 普通模板
0x671C00  add eax, 118h                   ; → template->elemAttr
0x671C08  call sub_78C7E8                 ; memcpy(MobStat+4, eax, 0x20)
```

而 `0x671BCD` 的 `mov eax, [ebx+250h]` 与 `MobStat`（`ebx+1A0`）的偏移差是 `0xB0`，
即 `_DWORD index 44`——正好落在 `DecodeTemporary` 写 POISON 的那三个 dword（`this[43..45]`）之内：

```
0x78B390  this[43] = Decode2();   // POISON 的 value
0x78B39D  this[44] = Decode4();   // POISON 的 source/skill id
0x78B3B3  this[45] = now + 500 * Decode2();  // POISON 的到期时刻
```

所以 `[ebx+250h]` 读到的是 **POISON 状态的 source id**（`this[44]`），
`2121003`/`2221003` 正是 Fire Demon / Ice Demon 的技能 id（`FPArchMage.FIRE_DEMON` / `ILArchMage.ICE_DEMON`）。

**这是客户端唯一的判断依据**：它不读任何"元素"字段，只读"给这个怪上的毒是谁放的"。

### 2.4 `MobStat::AdjustDamagedElemAttr`（**0x78C7AB**）

```asm
78C7AB  cmp [esp+arg_0], 205D2Bh    ; 2121003 = FPArchMage.FIRE_DEMON
78C7B3  jnz loc_78C7C3
78C7B5  xor eax, eax
78C7B7  mov dword ptr [ecx+8], 3    ; MobStat+8  (ICE)  = 3 (WEAK)
78C7BE  mov [ecx+0Ch], eax          ; MobStat+0xC (???)  = 0
78C7C1  jmp loc_78C7D9
78C7C3  cmp [esp+arg_0], 21E3CBh    ; 2221003 = ILArchMage.ICE_DEMON
78C7CB  jnz locret_78C7E5           ; 其他技能直接返回
78C7CD  xor eax, eax
78C7CF  mov [ecx+8], eax            ; MobStat+8  (ICE)  = 0 (NORMAL)
78C7D2  mov dword ptr [ecx+0Ch], 3  ; MobStat+0xC (FIRE) = 3 (WEAK)
78C7D9  mov [ecx+10h], eax          ; 清零
78C7DC  mov [ecx+14h], eax
78C7DF  mov [ecx+18h], eax
78C7E2  mov [ecx+1Ch], eax
```

`ecx` 是 `MobStat*`，所以 `+8` 是元素数组的 **第 2 个 dword**、`+0xC` 是第 3 个。
对照 `MobStat::SetFromWhenDoom`（`*(this+9)` 对应 `a2+184`）与 `SetFrom`（`this+9` ← `a2+184`）的
"跳过第 0/1 个" 的规律，元素数组的 dword index 与技能 id 的换算关系是：

| `MobStat` 偏移 | dword idx | 元素 | 客户端判定 |
|---|---|---|---|
| `+4`  | 1 | *(未使用)* | — |
| `+8`  | 2 | **ICE**（代码直接写死） | Fire Demon → `3` (WEAK) |
| `+0xC` | 3 | **FIRE**（代码直接写死） | Ice Demon → `3` (WEAK) |
| `+0x10`| 4 | 推测 LIGHTNING | 清零 |
| `+0x14`| 5 | 推测 POISON | 清零 |
| `+0x18`| 6 | 推测 HOLY | 清零 |
| `+0x1C`| 7 | 推测 DARKNESS | 清零 |

> **+8 = ICE、+0xC = FIRE 是代码直接证实的**（Fire Demon → ICE 弱、Ice Demon → FIRE 弱，符合游戏内表现）。
> `+0x10..+0x1C` 四行只是被 `xor eax,eax` 一起清零，"推测为 LIGHTNING/POISON/HOLY/DARKNESS" 由
> `MonsterStats` 的 8 元素顺序类推而来，**未经证实**；但它们被清零是确定事实。
>
> 另注意：这里**不是**按 0-based 元素序号对齐的（`Element.FIRE=2` 落在 dword 3）。
> `sub_790782` 里 `case 0..3` 是抗性枚举值，不是元素 id，别混。

### 2.5 元素抗性如何参与伤害

`CalcDamage::PDamage`（**0x78DF87**）/ `MDamage`（**0x791617**）都会调用 `sub_790660`：

```c
double sub_790660(double dmg, SKILLENTRY* skill, int* mobStatElem, int level, double prop) {
    if (skill->id == 2111006) {                 // Element Composition (FP)
        dmg *= 0.5;
        sub_790782(dmg, mobStatElem[4], 1.0);   // POISON
        sub_790782(dmg, mobStatElem[2], 1.0);   // ICE
        return dmg + dmg;
    }
    if (skill->id == 2211006) {                 // Element Composition (IL)
        dmg *= 0.5;
        sub_790782(dmg, mobStatElem[3], 1.0);   // FIRE
        sub_790782(dmg, mobStatElem[1], 1.0);   // ??
        return dmg + dmg;
    }
    ...
    sub_790782(dmg, mobStatElem[skill->elemIndex], prop);   // 普通技能按自身元素
}
```

`sub_790782`（**0x790782**）：

```c
switch (eff) {
  case 0: return dmg;                       // NORMAL
  case 1: return (1.0 - prop) * dmg;        // IMMUNE → 减伤
  case 2: return (1.0 - prop*0.5) * dmg;    // STRONG → 减伤
}
if (eff != 3) return dmg;
r = (prop*0.5 + 1.0) * dmg;                 // WEAK → 增伤
if (dmg >= r) r = dmg;
if (r >= 199999.0) r = 199999.0;
return r;
```

调用点传的 `prop`：
- `MDamage` → `sub_790660(v124, a10, a6 + 1, a11, 1.0 - v130 * 0.01)`（`v130` = 元素强化，1.0 表示满额）
- `PDamage` → `sub_790660(var_20, skill, mobStat+1, level, 1.0)`

所以客户端在**本地**用 `MobStat+8/+0xC` 计算伤害，服务端算出来的值与它不一致时就会出现
"打不死 / 伤害数字与服务端记录不符"。

---

## 3. 同步通道：挂在 POISON 上

### 3.1 设置

`CMob::ProcessStatSet`（**0x671B71**）在 `DecodeTemporary` 读完全部 per-status 字段后：

```c
if (mask & dword_BEFA20) {                    // POISON 位（位 9）
    *(this + 0x45C) = now;                    // POISON 起效时刻（CMob::Update 的每秒毒伤判定用）
    int src = *(this + 0x250);                // POISON 的 source id = 技能 id
    if (src == 2121003 || src == 2221003) {
        MobStat::AdjustDamagedElemAttr(this + 0x1A0, src, ...);
    } else {
        sub_78C7E8(templateElem);             // 非 Demon → 从模板恢复
    }
}
```

即：**只要 0xF2 包里 POISON 位的 `this[44]` 是 Fire/Ice Demon，客户端就自动把 ICE 或 FIRE 改 WEAK。**

`CMob::SetTemporaryStat`（**0x66FA1D**，出生携带 buff 的路径）在读完 `DecodeTemporary` 后
**无条件**调用一次 `MobStat::AdjustDamagedElemAttr(this+0x1A0, *(this+0x250), ...)`——
此时 `this+0x250` 还是模板里的值，所以对普通怪是空操作（只有模板里恰好是 2121003/2221003 才会改）。

### 3.2 恢复

`CANCEL_MONSTER_STATUS (0xF3)` → `CMob::OnStatReset`（**0x66C424**）→ `CMob::ProcessStatReset`（**0x672111**）：

```c
sub_78C70A(...)                    // = MobStat::Reset(mask)：按 mask 清零
if (mask & dword_BEFA20) {         // POISON
    sub_78C7E8(templateElem);      // ← 从模板重新 memcpy，元素抗性复位
}
if (mask & dword_BEF9B0) {         // DOOM
    MobStat::SetFrom(this+0x1A0, template);
    CMob::OnDoomed(this, 0);
}
```

**两个要点：**

1. 恢复是"整份 4 dword 从模板重拷"，不是"只恢复被改的那两条"——所以 `AdjustDamagedElemAttr`
   顺手清零的 LIGHTNING/POISON/HOLY/DARKNESS 四条也会一起回到模板值。
2. 取消 POISON 的最早时机由 `CMob::Update`（**0x6675A8**）的毒伤判定驱动，不是服务端主动发：
   只有毒伤/毒反弹相关逻辑判定该结束了，客户端才可能从 `MobStat` 里移除 POISON。

---

## 4. 与服务端的对应关系

| 服务端 | 客户端 |
|---|---|
| `MonsterStats.resistance`（WZ `elemAttr`，`LifeFactory.decodeElementalString`） | `CMobTemplate+0x118` → `MobStat+4..0x24` |
| `Monster.setTempEffectiveness(e, WEAK, ms)` | `MobStat::AdjustDamagedElemAttr`（经 POISON 位触发） |
| 到期移除（`processTempEffectiveness`） | `ProcessStatReset` 里的 `sub_78C7E8` |

### 4.1 当前实现的缺口

服务端在 `AbstractDealDamageHandler` 里做临时覆盖的两处：

```csharp
// FIRE_DEMON：给怪上 ICE WEAK
monster.setTempEffectiveness(Element.ICE, ElementalEffectiveness.WEAK, duration);
// ILArchMage.ICE_DEMON：给怪上 FIRE WEAK
monster.setTempEffectiveness(Element.FIRE, ElementalEffectiveness.WEAK, duration);

// 白骑士/圣骑士属性攻击
case BW_ICE_CHARGE / SWORD_ICE_CHARGE:
    monster.setTempEffectiveness(Element.ICE, ElementalEffectiveness.WEAK, duration);
...
```

- **Demon 两条**：客户端的 `AdjustDamagedElemAttr` 只会因为 **POISON 位 + source id == 2121003/2221003** 触发。
  服务端目前把 `MonsterStatus.POISON` 和 `FREEZE` 一起塞进 `StatEffect.MonsterStatuses`（`StatEffect.cs:717`），
  再由 `RegisterDebuff` → `BroadcastMobStats` 发 `0xF2`，`EncodeForLocalOne` 写的 `BuffSource.GetEncodeId()`
  就是 `getBuffSourceId()` = 技能 id。**所以只要这次 debuff 真的发出去并且 POISON 项存在，客户端就会同步。**
- **属性攻击（Charge）四条**：客户端 `AdjustDamagedElemAttr` 里**没有** 1211003..1211008 / 1221003 / 1221004 的分支，
  这些技能打上来时客户端走的是 `else` 分支 → `sub_78C7E8`（从模板恢复）。
  换句话说：**服务端对 Charge 做的 `setTempEffectiveness`，客户端完全不知道**，
  它只会拿模板抗性算伤害。这一支属于"服务端单方面改动"，会造成服务端校验值（`calcDmgMax` 里的 ×1.05/×1.2 加成）
  与客户端显示的伤害脱节。

### 4.2 顺带发现：Demon 的时长单位是**放大了 1000 倍**

`AbstractDealDamageHandler.cs:264,269`：

```csharp
long duration = 1000 * (attackEffect!.getDuration());
monster.setTempEffectiveness(Element.ICE, ElementalEffectiveness.WEAK, duration);
```

`StatEffect` 构造时 `duration = template.Time * 1000`（技能类，`StatEffect.cs:154`），
`template.Time` 来自 `SkillLevelData.Time` ← WZ `level/N/time`。查 `212.img.xml` / `222.img.xml`：
Fire/Ice Demon 各等级 `time` = 10~15（**秒**），于是 `getDuration()` 已经是 10000~15000（毫秒）。
`setTempEffectiveness` 的 `milli` 直接加在 `Node.getCurrentTime()` 上，所以再 `×1000` 的结果是
**10 000~15 000 秒（约 2.8~4.2 小时）**，而不是 10 秒。

同一文件里 Charge / Hamstring / Blind 用的都是 `Effect.getY() * 1000`（`y` 在 WZ 里是秒，单位正确），
`ToMonsterDebuff` 也直接吃 `getDuration()` 的毫秒值。按 `getDuration()` 的定义，这里应为：

```csharp
long duration = attackEffect!.getDuration();
```

---

## 5. 待确认

1. **Charge 技能客户端是否另有通道**：本 IDB 里 `AdjustDamagedElemAttr` 只认 2121003/2221003。
   理论上 Charge 的 `WK_CHARGE` 是玩家自己的 buff，客户端可能直接在本地把武器元素算进伤害，
   但 `sub_790660` 用的是**怪物**的 `MobStat` 元素值，没有读玩家的 Charge 元素。
   服务端 `PDamage` 里 `arg_17` 附近的代码尚未逐条跟完，无法排除"客户端用 Charge 元素去索引
   `MobStat` 同一下标"的可能。
2. **POISON 是否真的发给客户端**：服务端 `StatEffect.cs:717` 把 FIRE_DEMON/ICE_DEMON 映射成
   `MonsterStatus.POISON + FREEZE`，随后由 `RegisterDebuff` 判定 `Skill.getElement()` 的抗性
   （Fire Demon 的 `elemAttr=f`、Ice Demon 是 `i`）通过后才 `BroadcastMobStats`。
   也就是说"客户端能否同步"还额外依赖这一层 gate，未逐条验证哪些怪会被挡掉。
3. **`+0x10..+0x1C` 四条清零**：只能确认"被清零"，元素归属是类推（§2.4 表中已标注）。
4. **Demon 的 POISON 到期**：客户端 `CMob::Update`（0x6675A8）在 `CMob+0x45C`（POISON 起效时刻）之后
   开始每秒结算，但**没有**看到它按 `MobStat+0x30`（POISON 到期时刻）自行移除 POISON——
   是否需要服务端主动发 `0xF3` 才能让客户端走 `sub_78C7E8` 复位元素抗性，尚未定论。
   目前服务端是自己的本地表恢复，两边状态互相独立。

---

## 6. 本次改动

本次任务没有修改代码，只产出本文档。文档列出的两处候选修改：

- `AbstractDealDamageHandler.cs:264,269` 的 `1000 *` 双重换算（§4.2，已查 WZ 确认是 1000 倍）
- Charge 的 `setTempEffectiveness` 客户端不可见（§4.1，需先确认 §5 第 1 条）
