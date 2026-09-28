# MobBuffPackets ↔ CMob::OnStatSet 数据包分析

> IDA 数据库: `Angel.idb`（v83，基址 `0x400000`）  
> 服务端: `src/Application.Core/Channel/Net/Packets/MobBuffPackets.cs`（原 `tools/PacketCreator.cs` 内联实现）  
> 关联: `src/Application.Shared/GameProps/MonsterStatus.cs`、`src/Application.Shared/Net/SendOpcode.cs`  
> 关联文档: `docs/CWvsContext_OnTemporaryStatSet_分析.md`、`src/Application.Core/Channel/Net/Packets/BuffPackets.cs`（角色侧同类实现）  
>  
> **2026-09-27 修订**：§2.3 / §2.4 / §3.4 / §3.5 / §3.6 / §4 / §5 原本按“掩码是大端/低半区”的错误前提写成，
> 已按 `UINT128` dword 逆序（`sub_873B22`）重写；旧结论会让所有 mob buff 静默失效。  
> **2026-09-27 修订二**：`MonsterStatus` 由 `EnumClass`（引用相等、value = 单 bit 0x1..0x40000000）
> 改为 `enum`，**枚举值 = 客户端位号（0..30）**，与 `BuffStat` 同一约定（见 §2.6）；
> `NEUTRALISE` 随之改取位号范围之外的服务端专用 id，组掩码时跳过、不发包。

---

## 1. 结论速览

| 项目 | 值 |
|------|-----|
| 服务端发送 | `APPLY_MONSTER_STATUS = 0xF2`（242） |
| 客户端入口 | `CMobPool::OnPacket` → `CMobPool::OnMobPacket` (**0x67936D**) |
| 分发 | `Decode4(oid)` 后 `switch(a2)` case **242** → `CMob::OnStatSet` |
| `CMob::OnStatSet` | **0x66C301**，`void __thiscall OnStatSet(CMob*, CInPacket*)` |
| `CMob::ProcessStatSet` | **0x671B71**（私有，OnStatSet 或 `CMob::Update` 延迟调用） |
| `MobStat::DecodeTemporary` | **0x78B0B1**（按 mask 位读字段，顺序≠bit 序） |
| `MobStat::Reset` | **0x78C187**（ProcessStatReset 经 `sub_78C70A` 调用） |
| 对应取消包 | `CANCEL_MONSTER_STATUS = 0xF3` → `CMob::OnStatReset` (**0x66C424**) |

调用链：

```
服务端 Monster.applyMonsterBuff / broadcastStatusEffect
  → MobBuffPackets.ApplyMonsterStatus(oid, mse)                // 0xF2
客户端 CMobPool::OnPacket
  → CMobPool::OnMobPacket(0x67936D)
      Decode4 → oid；CMobPool::GetMob(oid)
      switch：case 242 → CMob::OnStatSet(mob, pkt)           // 0x66C301
  → OnStatSet
      DecodeBuffer(&mask, 0x10)           // 16 字节 UINT128
      if (IsMovementAffectingStat(mask) && *(CMob+330))
          延迟：ReservedPacket{type=1, mask, packet} 入队   // CMob+1328 (0x530)
      else
          ProcessStatSet(mask, packet)                        // 0x671B71
  → ProcessStatSet
      now = get_update_time()
      MobStat::DecodeTemporary(CMob+416 /*0x1A0*/, mask, pkt, now)
      按 mask 副作用（POISON/Venom/Ninja/Inert/Speed/Alpha/…）
      Decode2 + Decode1 → UpdateAffectedSkillList + size
      若 movement-affecting：再 Decode1 → SetStatChangedPoint
```

说明：

- `CMob+416`（0x1A0）为 `MobStat`；`DecodeTemporary` 的 `this[n]` 均相对该基址（`_DWORD` 下标）。
- `MobStat` 经 `SetFrom` 可见总大小 `0x208`（520 字节 = 130 个 DWORD）。
- mask 单 bit 常量由 `sub_873B8C(UINT128(1), bit)` 生成，地址规律 **`0xBEFAB0 - bit*0x10`**（初始化：`sub_78973C` bit0 … `sub_789D9C` bit34）。

---

## 2. MonsterStatus ↔ 客户端 mask / 字段对照表

### 2.1 DecodeTemporary 内的实际检查顺序（= 包体字段读取顺序）

**重要：DecodeTemporary 不是按 bit 升序扫描。** 服务端写 per-status 字段的顺序必须与下表一致，否则错位。

| # | 检查的 mask 常量 | bit# | 标准字段读取 | 写入 `MobStat`（DWORD 下标） |
|---|---|---|---|---|
| 1 | dword_BEFAB0 | 0 | D2, D4, D2 | [10],[11],[12] |
| 2 | dword_BEFAA0 | 1 | D2, D4, D2 | [14],[15],[16] |
| 3 | dword_BEFA90 | 2 | D2, D4, D2 | [18],[19],[20] |
| 4 | dword_BEFA80 | 3 | D2, D4, D2 | [22],[23],[24] |
| 5 | dword_BEFA70 | 4 | D2, D4, D2 | [26],[27],[28] |
| 6 | dword_BEFA60 | 5 | D2, D4, D2 | [30],[31],[32] |
| 7 | dword_BEFA50 | 6 | D2, D4, D2 | [34],[35],[36] |
| 8 | dword_BEFA40 | 7 | D2, D4, D2 | [37],[38],[39] |
| 9 | dword_BEFA30 | 8 | D2, D4, D2 | [40],[41],[42] |
| 10 | dword_BEFA20 | 9 | D2, D4, D2 | [43],[44],[45] |
| 11 | dword_BEFA10 | 10 | D2, D4, D2 | [47],[48],[49] |
| 12 | dword_BEFA00 | 11 | D2, D4, D2 | [50],[51],[52] |
| 13 | dword_BEF9F0 | 12 | D2, D4, D2 | [53],[54],[55] |
| 14 | dword_BEF9E0 | 13 | D2, D4, D2 | [56],[57],[58] |
| 15 | dword_BEF9D0 | 14 | D2, D4, D2 | [59],[60],[61] |
| 16 | dword_BEF9C0 | 15 | D2, D4, D2 | [62],[63],[64] |
| 17 | **dword_BEF990** | **18** | D2, D4, D2 | [72],[73],[74] |
| 18 | **dword_BEF980** | **19** | D2, D4, D2 | [75],[76],[77] |
| 19 | **dword_BEF9B0** | **16** | D2, D4, D2 | [65],[66],[67] |
| 20 | **dword_BEF9A0** | **17** | D2, D4, D2 | [68],[69],[70] |
| 21 | dword_BEF960 | 21 | D2, D4, D2 | [81],[82],[83] |
| 22 | dword_BEF950 | 22 | D2, D4, D2 | [84],[85],[86] |
| 23 | dword_BEF930 | 24 | D2, D4, D2 | [88],[89],[90] |
| 24 | dword_BEF920 | 25 | D2, D4, D2 | [92],[93],[94] |
| 25 | dword_BEF910 | 26 | D2, D4, D2 | [95],[96],[97] |
| 26 | dword_BEF8F0 | 28 | D2, D4, D2 | [98],[99],[100] |
| 27 | dword_BEF8E0 | 29 | D2, D4, D2 | [104],[105],[106] |
| 28 | dword_BEF8D0 | 30 | D2, D4, D2 | [108],[109],[110] |
| 29 | dword_BEF8B0 | 32 | D2, D4, D2 | [101],[102],[103] |
| 30 | dword_BEF8A0 | 33 | D2, D4, D2 | [113],[114],[115] |
| 31 | dword_BEF890 | 34 | D2, D4, D2 | [116],[117],[118] |
| 32 | **dword_BEF900** | **27** | **D4 count + 循环** | list @ +124（见 §2.3） |
| 33 | dword_BEF8E0（再查） | 29 | D4 | [107] |
| 34 | dword_BEF8D0（再查） | 30 | D4 | [111] |
| 35 | BEF8E0 \|\| BEF8D0 | 29\|30 | D4 | [112] |
| 36 | dword_BEF8C0 | 31 | D1, D1 | [122],[123] |

**未在 DecodeTemporary 中检查的位**（存在 mask 常量但不读包体）：

| bit | 常量 | 说明 |
|---|---|---|
| 20 | dword_BEF970 (0xBEF970) | 服务端 MonsterStatus 亦未占用 |
| 23 | dword_BEF940 (0xBEF940) | 对应 `NUELEMENTAL_ATTRIBUTELL=0x800000` — **仅 mask 位，DecodeTemporary 不读字段** |

标准三元组（每位置位读 8 字节）：

```text
Decode2 → nValue    → this[n]
Decode4 → nReason   → this[n+1]    // skillId 或 writeMobSkillId 的 4 字节
Decode2 → nDur      → this[n+2] = now + 500 * nDur
```

### 2.2 MonsterStatus 全表（旧服务端 value → bit → dw 地址 → 行为）

> 下表 `bit#` 一列现在就是 `MonsterStatus` 的枚举值（见 §2.6）；`服务端 value` 一列是重构前的 `getValue()`（`1 << bit`），留作对照。

| MonsterStatus | 服务端 value | bit# | mask 常量 (dw) | 虚地址 | MobStat `this[]` | Decode 序 | ProcessStatSet / 相关行为 |
|---|---|---|---|---|---|---|---|
| WATK | 0x00000001 | 0 | dword_BEFAB0 | 0xBEFAB0 | [10],[11],[12] | 1 | 仅更新数值 |
| WDEF | 0x00000002 | 1 | dword_BEFAA0 | 0xBEFAA0 | [14],[15],[16] | 2 | 仅更新数值 |
| NEUTRALISE | — | — | — | — | — | — | **无客户端位**：抗压标记，见 §2.6；旧实现借 WDEF 的 bit1（`0x2`） |
| MATK | 0x00000004 | 2 | dword_BEFA90 | 0xBEFA90 | [18],[19],[20] | 3 | 仅更新数值 |
| PHANTOM_IMPRINT *(first)* | 0x00000004 | 2 | dword_BEFA90 | 0xBEFA90 | [18],[19],[20] | 3 | 与 MATK 共 bit2，进 firstmask |
| MDEF | 0x00000008 | 3 | dword_BEFA80 | 0xBEFA80 | [22],[23],[24] | 4 | 仅更新数值 |
| ACC | 0x00000010 | 4 | dword_BEFA70 | 0xBEFA70 | [26],[27],[28] | 5 | 仅更新数值 |
| AVOID | 0x00000020 | 5 | dword_BEFA60 | 0xBEFA60 | [30],[31],[32] | 6 | 仅更新数值 |
| SPEED | 0x00000040 | 6 | dword_BEFA50 | 0xBEFA50 | [34],[35],[36] | 7 | **SetShoeAttr**；movement |
| STUN | 0x00000080 | 7 | dword_BEFA40 | 0xBEFA40 | [37],[38],[39] | 8 | movement |
| FREEZE | 0x00000100 | 8 | dword_BEFA30 | 0xBEFA30 | [40],[41],[42] | 9 | movement |
| POISON | 0x00000200 | 9 | dword_BEFA20 | 0xBEFA20 | [43],[44],[45] | 10 | **CMob+1116=now**；map 2121003/2221003→AdjustDamagedElemAttr，否则 `sub_78C7E8` |
| SEAL | 0x00000400 | 10 | dword_BEFA10 | 0xBEFA10 | [47],[48],[49] | 11 | 仅更新数值 |
| SHOWDOWN | 0x00000800 | 11 | dword_BEFA00 | 0xBEFA00 | [50],[51],[52] | 12 | 仅更新数值 |
| WEAPON_ATTACK_UP | 0x00001000 | 12 | dword_BEF9F0 | 0xBEF9F0 | [53],[54],[55] | 13 | 仅更新数值 |
| WEAPON_DEFENSE_UP | 0x00002000 | 13 | dword_BEF9E0 | 0xBEF9E0 | [56],[57],[58] | 14 | 仅更新数值 |
| MAGIC_ATTACK_UP | 0x00004000 | 14 | dword_BEF9D0 | 0xBEF9D0 | [59],[60],[61] | 15 | 仅更新数值 |
| MAGIC_DEFENSE_UP | 0x00008000 | 15 | dword_BEF9C0 | 0xBEF9C0 | [62],[63],[64] | 16 | 仅更新数值 |
| DOOM | 0x00010000 | 16 | dword_BEF9B0 | 0xBEF9B0 | [65],[66],[67] | **19** | movement；Reset 时 `SetFrom`+`OnDoomed(0)` |
| SHADOW_WEB | 0x00020000 | 17 | dword_BEF9A0 | 0xBEF9A0 | [68],[69],[70] | **20** | 仅更新数值 |
| WEAPON_IMMUNITY | 0x00040000 | 18 | dword_BEF990 | 0xBEF990 | [72],[73],[74] | **17** | 仅更新数值 |
| MAGIC_IMMUNITY | 0x00080000 | 19 | dword_BEF980 | 0xBEF980 | [75],[76],[77] | **18** | 仅更新数值 |
| *(未占用)* | 0x00100000 | 20 | dword_BEF970 | 0xBEF970 | — | **不读** | 服务端无定义 |
| HARD_SKIN | 0x00200000 | 21 | dword_BEF960 | 0xBEF960 | [81],[82],[83] | 21 | 仅更新数值 |
| NINJA_AMBUSH | 0x00400000 | 22 | dword_BEF950 | 0xBEF950 | [84],[85],[86] | 22 | **CMob+1124=now** |
| NUELEMENTAL_ATTRIBUTELL | 0x00800000 | 23 | dword_BEF940 | 0xBEF940 | — | **不读** | 仅存在于服务端枚举；客户端 DecodeTemporary 不处理 |
| VENOMOUS_WEAPON | 0x01000000 | 24 | dword_BEF930 | 0xBEF930 | [88],[89],[90] | 23 | **CMob+1120=now** |
| BLIND | 0x02000000 | 25 | dword_BEF920 | 0xBEF920 | [92],[93],[94] | 24 | 仅更新数值 |
| SEAL_SKILL | 0x04000000 | 26 | dword_BEF910 | 0xBEF910 | [95],[96],[97] | 25 | 仅更新数值 |
| *(未占用)* | 0x08000000 | 27 | dword_BEF900 | 0xBEF900 | list @+124 | 32 | **reflection：D4 count + 每个条目 3×D4**（`sub_78BB91`） |
| INERTMOB | 0x10000000 | 28 | dword_BEF8F0 | 0xBEF8F0 | [98],[99],[100] | 26 | `sub_66B562(0,0,0)` + `sub_672305`；movement |
| WEAPON_REFLECT *(first)* | 0x20000000 | 29 | dword_BEF8E0 | 0xBEF8E0 | [104..106],[107],[112] | 27,33,35 | 尾部额外 D4→[107]、\|\|时 D4→[112] |
| MAGIC_REFLECT *(first)* | 0x40000000 | 30 | dword_BEF8D0 | 0xBEF8D0 | [108..110],[111],[112] | 28,34,35 | 尾部额外 D4→[111]、\|\|时 D4→[112] |
| *(视觉位)* | — | 31 | dword_BEF8C0 | 0xBEF8C0 | [122],[123] | 36 | D1×2；ProcessStatSet 大段 **Alpha** 逻辑 |
| Toss | — | 32 | dword_BEF8B0 | 0xBEF8B0 | [101],[102],[103] | 29 | IsMovementAffectingStat 组成位 |
| NEUTRALISE | — | 33 | dword_BEF8A0 | 0xBEF8A0 | [113],[114],[115] | 30 | DecodeTemporary 读取 |
| PHANTOM_IMPRINT | — | 34 | dword_BEF890 | 0xBEF890 | [116],[117],[118] | 31 | DecodeTemporary 读取 |

**IsMovementAffectingStat（0x78C803）**：

```
缓存 dword_BEF878 = BEFA50 | BEFA40 | BEFA30 | BEF9B0 | BEF8B0
                 = SPEED | STUN | FREEZE | DOOM | bit32
```

任一位命中且 `*(CMob+330)` 非 0 → OnStatSet/OnStatReset 延迟入 `ReservedPacket` 队列。

### 2.3 bit27 reflection 列表（DecodeTemporary 尾段）

```c
if (mask & dword_BEF900) {
    int count = packet->Decode4();
    clear_list(MobStat + 124);          // sub_672A87
    while (count--) {
        node = list_alloc(MobStat + 124); // sub_7944BA
        sub_78BB91(packet);              // 3 × Decode4 = 12 字节/条目
        node[3] = now;                   // *(node+12) = update_time
    }
}
```

`sub_78BB91`（0x78BB91）读的是 **3 个 Decode4**，不是单个 OID：
```c
*this = Decode4(a2); *(this+1) = Decode4(a2); *(this+2) = Decode4(a2);
```

旧服务端 `applyMonsterStatus` 在 per-status 循环后写 `reflection` 数组（**无 count 前缀**，且条目是单个 int），与客户端 `Decode4 count` + 12 字节/条目两处都不对齐——当前枚举无 `0x08000000` 成员，该路径不触发；若未来启用 bit27，必须同时补 count 前缀与 3 dword 条目。
**现状：`MobBuffPackets` 已彻底删掉这段写出**（多写 1 个 dword 会把紧随其后的反射计数整体顶掉）。

### 2.4 16 字节掩码的字节布局（已验证；本节旧结论写反了）

关键前提：客户端 `UINT128` 是 **dword 逆序** 存放的。

- `UINT128::UINT128(unsigned long)`（0x873A2F → `sub_873B22`）做的是 `this[0]=this[1]=this[2]=0; this[3]=value`，
  即 **32 位字 3 持有数值的低 32 位**。
- 单 bit 常量由 `UINT128(1) << n` 生成（`sub_873B8C`），位 n 的常量地址规律 `0xBEFAB0 - n*0x10`
  （`sub_78973C` 初始化位 0 … `sub_789D9C` 初始化位 34）。
- `UINT128::operator&`（0x874049）是逐 dword 的普通 AND。
- `CInPacket::DecodeBuffer`（0x432257）是纯 `memcpy`，**不做字节交换**。

于是 16 字节包体的偏移与客户端位号的对应是：

| 包体偏移 | 客户端位 |
|---|---|
| 0..7   | 64..127 |
| 8..11  | 32..63  |
| 12..15 | 0..31   |

`MonsterStatus` 的每个状态都对应客户端的一个位，且位号都在 0..30
（WATK→dword_BEFAB0=位 0、SPEED→dword_BEFA50=位 6、MAGIC_REFLECT→dword_BEF8D0=位 30）。
所以**所有状态位都必须 OR 进包体偏移 12..15 的那一个 dword**：

```text
offset 0..7   : 0                        → 客户端位 64..127（恒不命中）
offset 8..11  : 0                        → 客户端位 32..63 （恒不命中）
offset 12..15 : WATK|WDEF|…|MAGIC_REFLECT→ 客户端位 0..31  ✓
```

## 3. OnStatSet 如何接收这些状态

### 3.1 服务端写出布局（旧 `applyMonsterStatus`，`PacketCreator.cs:3382`；现状见 §4.1）

```text
<c>APPLY_MONSTER_STATUS (0xF2)</c>，对应 <c>CMob::OnStatSet</c>（0x66C301）→ <c>ProcessStatSet</c>（0x671B71）。
int32  oid
16B    掩码（WriteStatMask）> 服务端 per-status **遍历顺序 = `mse.getStati()` 的 Dictionary 插入序**；必须与 §2.1 的 DecodeTemporary 检查序一致（多状态同时生效时）。
每个置位状态（ClientFieldOrder 序）：8 字节（见 EncodeForLocalOne）
[位 29] int32、[位 30] int32、[位 29|30] int32   ← EncodeReflectCounter### 3.2 客户端 OnMobPacket 分发（0x67936D）
int16  skillRelated                              ← UpdateAffectedSkillList 的到期偏移
uint8  size                                      ← CMob+1220
[movement 命中时] uint8 statChangedPoint          ← CMovePath::SetStatChangedPointvoid CMobPool::OnMobPacket(CMobPool* this, int op, CInPacket* p) {
</code>    int oid = p->Decode4();
<para>    CMob* mob = CMobPool::GetMob(this, oid);
<paramref name="mse"/> 里若只有纯服务端状态（<see cref="MonsterStatus.NEUTRALISE"/>），    if (!mob) return;
掩码会写 0：客户端那边这只怪不带任何状态，效果全在服务端（如抗压让它不打人）。    switch (op) {
包体仍然完整，客户端读完三个尾部字段就结束。    case 242: CMob::OnStatSet(mob, p);   break;  // APPLY_MONSTER_STATUS
</para>    case 243: CMob::OnStatReset(mob, p); break;  // CANCEL_MONSTER_STATUS
    // 239 OnMove, 240 OnCtrlAck, 246 OnDamaged, ...
    }
}
```

```
<c>DecodeTemporary</c> 尾段的反射计数（在 28 个状态字段之后）：
位 29 WEAPON_REFLECT 额外 Decode4 → <c>MobStat[107]</c>、
位 30 MAGIC_REFLECT 额外 Decode4 → <c>MobStat[111]</c>、
两者任一置位再 Decode4 → <c>MobStat[112]</c>（counter prob，写 100）。
<para>
只要掩码位置位客户端就会读，所以这里必须补齐，否则包体错位。
三段之间没有别的字段：位 27/32/33/34 的读法都排在这三段之前，而服务端置不起那些位。
</para>
<para>
客户端算这个数字的是 <c>sub_7937E3</c>（<c>TryDoingMeleeAttack</c> / <c>TryDoingShootAttack</c> /
<c>TryDoingMagicAttack</c> 打中带反击的怪之后各调一次，第 1 个参数 2 表示魔法）：
</para>
<code>
if (!mobStat || (mobStat[112] &amp;&amp; rand() % 100 &gt;= mobStat[112])) return 0;
base = (type == 2 ? mobStat[108] : mobStat[104]);   // 魔法 / 物理
var  = (type == 2 ? mobStat[111] : mobStat[107]);
return var ? base + rand() % var - var / 2 : base;
</code>
<para>
也就是 <c>[104]</c>/<c>[108]</c> 是固定反伤值——来自 per-status 字段里那个 D2
（<see cref="EncodeForLocalOne"/> 写的 <c>stati</c> 值，即 <see cref="MobSkill.applyEffect"/>
放进 WEAPON_REFLECT 的 x / MAGIC_REFLECT 的 y），
<c>[107]</c>/<c>[111]</c> 是上下浮动幅度（WZ 的 143/144/145 没有这个字段，所以写 0，
客户端于是走 <c>var == 0</c> 那条分支直接返回 base，弹出的数字正好是 x / y，L1 即 10000），
<c>[112]</c> 是触发概率（写 100 = 必触发）。
</para>
<para>
客户端只弹数字、不回包：算完经 <c>sub_953595</c> 调 <c>CUserLocal::SetDamaged</c>
（活体 vtable 基址 0xB3D20C，槽 18 即 <c>[vptr+0x48]</c> → 0x9581A9），
那个 thunk 把第 5 个参数 <c>CMob*</c> 和其余 8 个参数全填 0，
而 SetDamaged 只在第 8 个参数非 0 时才发 TAKE_DAMAGE——
所以 <c>TakeDamageHandler</c> 收不到反击包，反伤只能由服务端自己扣
（<c>AbstractDealDamageHandler</c> 里那段，扣的就是这里发出去的 stati 值）。
</para>
```

### 3.3 OnStatSet 主体（0x66C301）

```c
void CMob::OnStatSet(CMob* this, CInPacket* p) {
    UINT128 mask;
    p->DecodeBuffer(&mask, 0x10);              // ① 16 字节

    if (MobStat::IsMovementAffectingStat(mask)
        && *(DWORD*)((char*)this + 330)) {     // ② 本地激活 (CMob+0x14A)
        ReservedPacket* r = alloc();
        r->type = 1;                           // 1=StatSet, 0=StatReset (OnStatReset 写 0)
        r->mask = mask;
        r->packet = *p;                        // 拷贝剩余包体
        ZList<ZRef<ReservedPacket>>::AddTail(this + 1328, r);  // CMob+0x530
        return;
    }
    ProcessStatSet(this, mask, p);             // ③ 立即处理
}
```

`ReservedPacket` 由 `CMob::Update`（**0x6675A8** 附近，xref: 0x6683F7 调 ProcessStatSet）在移动/控制时机取出，避免打断寻路。

### 3.4 ProcessStatSet 读包顺序（0x671B71）

```c
void CMob::ProcessStatSet(CMob* this, UINT128 mask, CInPacket* p) {
    DWORD now = get_update_time();

    // A. 按 §2.1 顺序读 per-status 字段
    MobStat::DecodeTemporary(this + 416, mask, p, now);

    // B. 副作用（不再读包）
    if (mask & dword_BEFA20) {                 // POISON
        *(this + 1116) = now;                  // 0x45C
        int mapid = *(this + 592);             // 0x250 template/map 相关
        if (mapid == 0x205D2B /*2121003*/ || mapid == 0x21E3CB /*2221003*/)
            MobStat::AdjustDamagedElemAttr(this+416, mapid, ...);
        else
            sub_78C7E8(elemAttr_ptr);          // 恢复受击元素属性
    }
    if (mask & dword_BEF930) *(this + 1120) = now;  // VENOMOUS_WEAPON 0x460
    if (mask & dword_BEF950) *(this + 1124) = now;  // NINJA_AMBUSH   0x464
    if (mask & dword_BEF8F0) {                 // INERTMOB
        sub_66B562(this, 0, 0);
        sub_672305(g_pMobPool+0x64, this);     // 追击/控制相关
    }

    // C. 公共尾部：无条件 Decode2 + Decode1
    //    （旧服务端写的是 writeByte(size)+writeInt(0)，字节数够但语义错位，靠封包长度前缀幸免）
    WORD skillRelated = p->Decode2();          // → UpdateAffectedSkillList 的到期偏移
    BYTE size = p->Decode1();                  // → *(this + 1220) / 0x4C4
    CMob::UpdateAffectedSkillList(this, skillRelated);

    if (mask & dword_BEFA50)                   // SPEED
        CMob::SetShoeAttr(this);

    if (mask & dword_BEF8C0) {                 // 视觉位：层 Alpha
        // IWzGr2DLayer::GetAlpha …（大段 COM）
        if (!*(this + 0x38C) && !*(this + 0x388))
            CMob::PrepareActionLayer(this);
    }

    // D. movement 尾包
    if (MobStat::IsMovementAffectingStat(mask)) {
        BYTE point = p->Decode1();
        if ((mask & dword_BEF9B0) == 0 || *(this + 1320) == 0) {
            // DOOM 未挂起或已有 pending → 立即写入 MovePath
            CMovePath::SetStatChangedPoint(this->movePath, point);
        } else {
            *(this + 1320) = 1;                // pending 标志 0x528
            *(this + 1324) = point;            // 0x52C
        }
    }
}
```

**尾部字节对照（现状 = `MobBuffPackets`）：**

| 客户端读取 | 字节 | 服务端写出 |
|---|---|---|
| Decode2 → skillRelated | 2 | `writeShort(0)` |
| Decode1 → size@+1220 | 1 | `writeByte(GetStatCount(stati.Keys))`（只数有客户端位的状态） |
| Decode1 → movement point（movement 命中时） | 1 | `writeByte(0)` |

旧服务端写的是 `writeByte(size) + writeInt(0)`（5 字节）：字节数比上述 3~4 字节多，靠封包自带长度前缀没触发越界，
但**语义错位**——客户端的 `skillRelated` 读到的是 `size | (0<<8)`，`size@+1220` 读到 0。
movement 位命中时旧写法刚好剩下 1 个未读字节可吃，所以症状不易暴露；漏写才会抛 `ZException` 断线。

### 3.5 取消路径（对照）

`CANCEL_MONSTER_STATUS (0xF3)` → `OnStatReset (0x66C424)`：

- 同样 `DecodeBuffer(16)`；
- movement-affecting 且激活 → `ReservedPacket.type = 0` 入队；
- 否则 `ProcessStatReset (0x672111)`：
  1. `sub_78C70A` → `MobStat::Reset(mask)`（0x78C187，按 mask 清零对应 `this[n]`）；
  2. POISON：`sub_78C7E8` 恢复 elemAttr；
  3. DOOM：`MobStat::SetFrom(template)` + `CMob::OnDoomed(0)`；
  4. INERTMOB：`sub_66B562` + 可能 `CMobPool::CancelChaseTarget`；
  5. **仅 `Decode1`** → `a1[305]`（即 +1220），`UpdateAffectedSkillList(..., 0)` — **无 Decode2**；
  6. SPEED → `SetShoeAttr`；
  7. 若 `CMob::IsActive` 且 movement：`Decode1` → `CMovePath::SetStatChangedPoint`。

服务端 `cancelMonsterStatus`（旧 `PacketCreator.cs:3419`）：`oid + writeLong(0) + writeIntMask + writeInt(0)` →
掩码布局同 §2.4（secondmask 碰巧命中位 0..31，`isFirst` 那 4 个状态落空）；尾部 `Decode1 + [Decode1]` 字节数对得上但语义错位。
现状 `MobBuffPackets.CancelMonsterStatus`：`oid + writeLong(0) + writeInt(0) + writeInt(mask)`，
再 `writeByte(0)`，movement 命中时补 `writeByte(0)`（客户端还会用它那个 `IsActive()` 门控再判一次，
我们无条件多写 1 字节，多出来的尾巴读不到、无害）。

### 3.6 出生带 buff 路径（对照，非 0xF2）

`CMob::SetTemporaryStat`（**0x66FA1D**）用于 `OnMobEnterField` / `SetLocalMob`：

- `Reset(~0)` + `SetFrom(template)`；
- `DecodeBuffer(16)` → `DecodeTemporary`；
- DOOM → `SetFromWhenDoom`；`AdjustDamagedElemAttr`；
- `UpdateAffectedSkillList` / `SetShoeAttr` / bit32 → `*(this+329)=0`。

该路径 mask 旧由服务端 **`writeLongEncodeTemporaryMask`**（4×int，`pos=isFirst?0:2`）编码，
旧布局下 first→位 96..127、second→位 32..63，客户端**一位都测不到**（见 §2.4）。
现状由 `MobBuffPackets.EncodeTemporary` 编码：`writeLong(0)+writeInt(0)+writeInt(mask)` 后再写 per-status 字段；
**没有** 0xF2 那条 `Decode2+Decode1` 尾巴——`SetTemporaryStat` 读完 `DecodeTemporary` 就返回，
`*(this+305)=a3` 的 size 值来自封包里更早的那个字节（出生包的 `controller==null?5:1`）。

---

## 4. 摘要

1. **表格**：§2.2 覆盖全部 `MonsterStatus` → bit# → `dword_BEFxxx`（虚地址 `0xBEFxxx`）→ `MobStat this[]` → 行为；标出服务端未占用的 bit20/27、客户端 **不读字段** 的 bit20/23（含 `NUELEMENTAL_ATTRIBUTELL`）。§2.1 给出 **DecodeTemporary 真实检查序**（bit18/19 在 bit16/17 之前，等）。  
2. **收包**：`OnMobPacket` case **242** → `OnStatSet` 读 **16 字节 UINT128**；movement-affecting 且激活则挂 `ReservedPacket(type=1)`；否则 `ProcessStatSet` → `DecodeTemporary` 按 **§2.1 序** 以 `D2+D4+D2`（8 字节/位）读入，再处理副作用与公共尾部 `Decode2+Decode1`（+可选 movement `Decode1`）。  
3. **Mask 布局（已验证）**：客户端唯一读法 = 16B mask，且 `UINT128` dword 逆序 ⇒ 状态位必须全部落在 **偏移 12..15**（客户端位 0..31）。旧 `applyMonsterStatus` 只有 secondmask 碰巧落在位 0..31（非 `isFirst` 状态因此能生效），4 个 `isFirst` 状态与整条出生路径都失效；`isFirst` 只是旧枚举的标记（enum 重构后已删除，见 §2.6）。详见 §2.4。  
4. **对齐要求**：服务端 per-status **遍历顺序必须等于 §2.1**（= `MobBuffPackets.ClientFieldOrder`，不是字典插入序）；每位宽 8 字节，位 29/30 尾部另有 D4、任一置位再 D4、位 27 路径是 count + 3×D4（当前枚举不触发）；0xF2 尾巴是 `Decode2+Decode1(+movement Decode1)`，0xF3 是 `Decode1(+movement Decode1)`，出生包无尾巴；掩码位必须落在偏移 12..15，否则字段永远不被读取。  
5. **枚举（§2.6）**：`MonsterStatus` = `enum`，值即客户端位号（0..30）；`PHANTOM_IMPRINT = MATK = 2` 共位别名（值语义下不可并存）；`NEUTRALISE` 取 `MonsterStatusUtils.MinServerOnlyId`（100）——纯服务端状态，不进掩码、不计入 `size`，抗压因此不再顺带往客户端塞一个 WDEF。
