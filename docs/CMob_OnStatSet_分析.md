# PacketCreator.applyMonsterStatus ↔ CMob::OnStatSet 数据包分析

> IDA 数据库: `Angel.idb`（v83，基址 `0x400000`）  
> 服务端: `src/Application.Core/tools/PacketCreator.cs`  
> 关联: `src/Application.Shared/GameProps/MonsterStatus.cs`、`src/Application.Shared/Net/SendOpcode.cs`  
> 关联文档: `docs/CWvsContext_OnTemporaryStatSet_分析.md`

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
服务端 Monster.applyStatus / broadcastStatusEffect
  → PacketCreator.applyMonsterStatus(oid, mse, reflection)   // 0xF2
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

### 2.2 MonsterStatus 全表（服务端 value → bit → dw 地址 → 行为）

| MonsterStatus | 服务端 value | bit# | mask 常量 (dw) | 虚地址 | MobStat `this[]` | Decode 序 | ProcessStatSet / 相关行为 |
|---|---|---|---|---|---|---|---|
| WATK | 0x00000001 | 0 | dword_BEFAB0 | 0xBEFAB0 | [10],[11],[12] | 1 | 仅更新数值 |
| WDEF | 0x00000002 | 1 | dword_BEFAA0 | 0xBEFAA0 | [14],[15],[16] | 2 | 仅更新数值 |
| NEUTRALISE *(first)* | 0x00000002 | 1 | dword_BEFAA0 | 0xBEFAA0 | [14],[15],[16] | 2 | 与 WDEF 共 bit1，进 firstmask |
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
| *(未占用)* | 0x08000000 | 27 | dword_BEF900 | 0xBEF900 | list @+124 | 32 | **reflection：D4 count + 循环读 OID** |
| INERTMOB | 0x10000000 | 28 | dword_BEF8F0 | 0xBEF8F0 | [98],[99],[100] | 26 | `sub_66B562(0,0,0)` + `sub_672305`；movement |
| WEAPON_REFLECT *(first)* | 0x20000000 | 29 | dword_BEF8E0 | 0xBEF8E0 | [104..106],[107],[112] | 27,33,35 | 尾部额外 D4→[107]、\|\|时 D4→[112] |
| MAGIC_REFLECT *(first)* | 0x40000000 | 30 | dword_BEF8D0 | 0xBEF8D0 | [108..110],[111],[112] | 28,34,35 | 尾部额外 D4→[111]、\|\|时 D4→[112] |
| *(视觉位)* | — | 31 | dword_BEF8C0 | 0xBEF8C0 | [122],[123] | 36 | D1×2；ProcessStatSet 大段 **Alpha** 逻辑 |
| *(movement 扩展)* | — | 32 | dword_BEF8B0 | 0xBEF8B0 | [101],[102],[103] | 29 | IsMovementAffectingStat 组成位 |
| *(保留)* | — | 33 | dword_BEF8A0 | 0xBEF8A0 | [113],[114],[115] | 30 | DecodeTemporary 读取 |
| *(保留)* | — | 34 | dword_BEF890 | 0xBEF890 | [116],[117],[118] | 31 | DecodeTemporary 读取 |

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
        sub_78BB91(packet);              // Decode4 → 读入一个 OID
        node[3] = now;                   // *(node+12) = update_time
    }
}
```

服务端 `applyMonsterStatus` 在 per-status 循环后写 `reflection` OID 数组（**无 count 前缀**），与客户端 `Decode4 count` 不对齐——当前枚举无 `0x08000000` 成员，该路径通常不触发；若未来启用 bit27 需同时改服务端编码。

### 2.4 first / second mask 与 16 字节布局（确定结论）

**客户端格式唯一，不存在两套兼容读法。**

- `OnStatSet` 与 `SetTemporaryStat` 都是 `DecodeBuffer(16)` → 同一个 `DecodeTemporary` / `IsMovementAffectingStat` / `Reset`。
- 位测试常量全部是 `UINT128(1)<<n`（n=0..34），**有效状态位必须落在 16 字节 LE mask 的低半区（bytes 0..7 = UINT128 bit 0..63，实际只用到 bit 0..34）**。
- 高半区（bytes 8..15 / bit 64..127）的 1 在 `DecodeTemporary` 中**不会**命中任何 `dword_BEFxxx`。

#### 服务端两种写法 vs 客户端

| 写法 | 16 字节 LE 布局 | 客户端 bit0..34 命中 |
|------|-----------------|----------------------|
| **`applyMonsterStatus` / `cancelMonsterStatus`**：`writeLong(0)` + `writeIntMask` | `[00×8][first u32][second u32]` → 状态位全在 **bit 64..127** | **全否** → `DecodeTemporary` 一位都不读；尾部 `Decode2+Decode1` 会错位吃 status 数据 |
| **`encodeTemporary`**：`writeLongEncodeTemporaryMask`（`pos = isFirst?0:2`） | `[first u32][00×4][second u32][00×4]` → first→bit0..31，second→bit64..95 | **仅 isFirst 命中**：NEUTRALISE / PHANTOM / \*REFLECT ✓；POISON/SPEED/… ✗；first/second 混用时字段序还可能与 mask 命中集错位 |
| **客户端预期** | 状态位须在 **bytes 0..7**（且 DecodeTemporary 只用 bit 0..34） | — |

```text
applyMonsterStatus 线上字节序（little-endian）:
  offset 0..7   : writeLong(0)       → UINT128 低 64 恒 0
  offset 8..11  : firstmask (int32)  → UINT128 bit 64..95
  offset 12..15 : secondmask (int32) → UINT128 bit 96..127

writeLongEncodeTemporaryMask 线上字节序:
  offset 0..3   : masks[0] = isFirst 低 32   → bit 0..31   ✓ 与客户端一致
  offset 4..7   : masks[1] = isFirst 高 32=0 → bit 32..63
  offset 8..11  : masks[2] = !isFirst 低 32  → bit 64..95  ✗ 客户端不测
  offset 12..15 : masks[3] = !isFirst 高 32=0→ bit 96..127
```

#### 结论（以客户端读取为准）

1. **`applyMonsterStatus` 的 `writeLong(0)+writeIntMask` 相对本客户端是错误布局**：低 64 恒 0 → `mask & dword_BEFxxx` 恒假 → `DecodeTemporary` 不读任何 per-status 字段。0xF2 路径可能表现为状态静默不生效 / 尾部错位，而不是“另一套正确协议”。
2. **`writeLongEncodeTemporaryMask` 只对一半**：`isFirst` 进 masks[0] 碰巧正确；`!isFirst`（绝大多数状态）进 masks[2]=bit64+，客户端测不到。`MonsterStatus` 的 value（0x1..0x40000000）与客户端 bit0..30 同一值域——若 first/second 都表示“本 32 位图内的位”，则 **两者都应 OR 进 masks[0]**（或等价地写入低 32 位），不能按 64 位 high/low 半区拆分。
3. **角色 `WriteStatMask` 的 first/second ↔ bit64+ 映射不能套到 Mob**：角色高位 bit 由另一套常量测试；Mob 的 `DecodeTemporary` 只用低 bit0..34。
4. **`encodeTemporary` 注释 “to patch some status crashing players”（过滤 WATK/WDEF）** 更像是在绕开错误 mask/字段序导致的错位崩溃，根因是布局而非 WATK/WDEF 本身。
5. **修复方向（仅记录，本次不改代码）**：`applyMonsterStatus` / `cancelMonsterStatus` / `writeLongEncodeTemporaryMask` 都应把参与 `DecodeTemporary` 的状态位写入 **mask 低 32/64 位**；去掉或重解释 `writeLong(0)` 的前导零。改完后 per-status 遍历序仍须等于 §2.1。

### 2.5 服务端每状态字段宽度

```csharp
p.writeShort(value);                          // 2 → Decode2
if (mse.isMonsterSkill())
    writeMobSkillId(p, msId);                 // writeShort(type)+writeShort(level) = 4 → Decode4
else
    p.writeInt(mse.getSkill()!.getId());      // 4 → Decode4
p.writeShort(-1);                             // 2 → Decode2；now + 500*(-1)
```

`writeMobSkillId` 为 4 字节，与客户端 `Decode4` 对齐。

---

## 3. OnStatSet 如何接收这些状态

### 3.1 服务端写出布局（`applyMonsterStatus`，`PacketCreator.cs:3382`）

```text
OutPacket = opcode 0xF2 (short LE)
+ int32  oid                         // OnMobPacket 已 Decode4 消费
+ int64  0                           // writeLong(0)
+ int32  firstmask                   // isFirst=true
+ int32  secondmask                  // isFirst=false
  ---- 以上 16 字节 = DecodeBuffer(&mask, 0x10) ----
+ 对 stati 中每个状态（Dictionary 插入序）：
    int16  value
    int32  skillId  或  (int16 type + int16 level)  // writeMobSkillId
    int16  -1                        // buffTime 占位
+ [可选] 每个 reflection 玩家：int32 objectId
+ uint8  size                        // stati.Count；有 reflection 且 count>0 时 size/2
+ int32  0                           // 尾部
```

> 服务端 per-status **遍历顺序 = `mse.getStati()` 的 Dictionary 插入序**；必须与 §2.1 的 DecodeTemporary 检查序一致（多状态同时生效时）。

### 3.2 客户端 OnMobPacket 分发（0x67936D）

```c
void CMobPool::OnMobPacket(CMobPool* this, int op, CInPacket* p) {
    int oid = p->Decode4();
    CMob* mob = CMobPool::GetMob(this, oid);
    if (!mob) return;
    switch (op) {
    case 242: CMob::OnStatSet(mob, p);   break;  // APPLY_MONSTER_STATUS
    case 243: CMob::OnStatReset(mob, p); break;  // CANCEL_MONSTER_STATUS
    // 239 OnMove, 240 OnCtrlAck, 246 OnDamaged, ...
    }
}
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

    // C. 公共尾部（服务端 writeByte(size)+writeInt(0)）
    WORD skillRelated = p->Decode2();          // → UpdateAffectedSkillList
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

**尾部字节对照：**

| 客户端读取 | 字节 | 服务端写出 |
|---|---|---|
| Decode2 → skillRelated | 2 | `writeByte(size)` + `writeInt(0)` 低 2 字节（size \| 0<<8） |
| Decode1 → size@+1220 | 1 | `writeInt(0)` 下 1 字节 |
| Decode1 → movement point（可选） | 1 | `writeInt(0)` 再 1 字节 |

即服务端 `writeByte(size)+writeInt(0)` 共 5 字节，覆盖 Decode2+Decode1(+可选 Decode1)；多出的字节在非 movement 时可能残留未读（包边界允许）。

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

服务端 `cancelMonsterStatus`（`PacketCreator.cs:3419`）：`oid + writeLong(0) + writeIntMask + writeInt(0)`。

### 3.6 出生带 buff 路径（对照，非 0xF2）

`CMob::SetTemporaryStat`（**0x66FA1D**）用于 `OnMobEnterField` / `SetLocalMob`：

- `Reset(~0)` + `SetFrom(template)`；
- `DecodeBuffer(16)` → `DecodeTemporary`；
- DOOM → `SetFromWhenDoom`；`AdjustDamagedElemAttr`；
- `UpdateAffectedSkillList` / `SetShoeAttr` / bit32 → `*(this+329)=0`。

该路径 mask 由服务端 **`writeLongEncodeTemporaryMask`**（4×int，`pos=isFirst?0:2`）编码。相对客户端：仅 **isFirst**（NEUTRAISE/PHANTOM/\*REFLECT）落在 bit0..31 能被测到；**!isFirst 被放进 bit64+，客户端不读**。不是 `applyMonsterStatus` 的正确参照——见 §2.4 确定结论。

---

## 4. 服务端 / 客户端片段对照

### 4.1 服务端发送

```csharp
// PacketCreator.cs:3382
public static Packet applyMonsterStatus(int oid, MonsterStatusEffect mse, List<int>? reflection)
{
    Dictionary<MonsterStatus, int> stati = mse.getStati();
    OutPacket p = OutPacket.create(SendOpcode.APPLY_MONSTER_STATUS); // 0xF2
    p.writeInt(oid);
    p.writeLong(0);
    writeIntMask(p, stati);                 // firstmask + secondmask
    foreach (var stat in stati)             // 须匹配 DecodeTemporary 检查序
    {
        p.writeShort(stat.Value);
        if (mse.isMonsterSkill())
            writeMobSkillId(p, mse.getMobSkill()!.getId()); // 2+2
        else
            p.writeInt(mse.getSkill()!.getId());
        p.writeShort(-1);
    }
    // reflection OID[] …
    p.writeByte(size);
    p.writeInt(0);
    return p;
}
```

### 4.2 客户端接收（伪代码）

```c
// OnMobPacket 0x67936D — 已消费 oid
// OnStatSet 0x66C301
UINT128 mask = pkt.DecodeBuffer(16);
if (IsMovementAffectingStat(mask) && cmob->flag_330)
    cmob->reserved.Add({ type=1, mask, pkt });
else
    ProcessStatSet(cmob, mask, pkt);

// ProcessStatSet 0x671B71
MobStat::DecodeTemporary(&cmob->mobStat /*+416*/, mask, pkt, now);
// §2.1 顺序 + §2.2 副作用 …
WORD w = pkt.Decode2();
BYTE n = pkt.Decode1();                    // → +1220
UpdateAffectedSkillList(cmob, w);
if (mask & SPEED) SetShoeAttr(cmob);
if (IsMovementAffectingStat(mask))
    MovePath.SetStatChangedPoint(..., pkt.Decode1());
```

---

## 5. 摘要

1. **表格**：§2.2 覆盖全部 `MonsterStatus` → bit# → `dword_BEFxxx`（虚地址 `0xBEFxxx`）→ `MobStat this[]` → 行为；标出服务端未占用的 bit20/27、客户端 **不读字段** 的 bit20/23（含 `NUELEMENTAL_ATTRIBUTELL`）。§2.1 给出 **DecodeTemporary 真实检查序**（bit18/19 在 bit16/17 之前，等）。  
2. **收包**：`OnMobPacket` case **242** → `OnStatSet` 读 **16 字节 UINT128**；movement-affecting 且激活则挂 `ReservedPacket(type=1)`；否则 `ProcessStatSet` → `DecodeTemporary` 按 **§2.1 序** 以 `D2+D4+D2`（8 字节/位）读入，再处理副作用与公共尾部 `Decode2+Decode1`（+可选 movement `Decode1`）。  
3. **Mask 布局（确定）**：客户端唯一读法 = 16B LE mask，状态位须在 **bit0..34（低半区）**。`applyMonsterStatus` 的 `writeLong(0)+writeIntMask` 使状态全在 bit64+ → **与客户端完全不兼容**（`DecodeTemporary` 不命中）；`writeLongEncodeTemporaryMask` 仅 isFirst 进低 32 正确，`!isFirst` 进 masks[2]=bit64+ 同样测不到。两者均不能套用角色 first/second 半区约定；详见 §2.4。  
4. **对齐要求**：服务端 per-status **遍历顺序 = Dictionary 插入序** 必须等于 §2.1；每位字节宽度须含 reflect 尾部 D4、视觉位双 D1、reflection 路径 count+OID（当前枚举未触发 bit27）；mask 位必须落在低半区，否则字段永远不被读取。
