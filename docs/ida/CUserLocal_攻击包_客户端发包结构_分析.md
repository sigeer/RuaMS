# 客户端 → 服务端攻击包结构（v83，CUserLocal 发包侧）

> IDA 数据库：`` `Angel.i64 ``（v83，基址 `0x400000`）
> 服务端：`src/Application.Core/Channel/Net/Handlers/AbstractDealDamageHandler.cs`（`parseDamage` / `parseMesoExplosion`）
> 关联文档：`docs/ida/CUserRemote_OnAttack_暴击伤害编码_分析与实现.md`（同一组包在**旁观者客户端**的接收侧）

---

## 1. 结论速览

| 项目 | 客户端发包函数 | 服务端入口 |
|------|---------------|-----------|
| 近战 `CLOSE_RANGE_ATTACK = 186 (0x2C)` | `TryDoingMeleeAttack` **0x950921** | `CloseRangeDamageHandler` |
| 远程 `RANGED_ATTACK = 187 (0x2D)` | `TryDoingShootAttack` **0x9537D5** | `RangedAttackHandler` |
| 魔法 `MAGIC_ATTACK = 188 (0x2E)` | `TryDoingMagicAttack` **0x95571F** | `MagicDamageHandler` |
| 身体碰撞 `BODY_ATTACK = 189 (0x2F)` | `TryDoingNormalAttack` **0x95719B** | — |
| 召唤兽 | `CSummoned::TryDoingAttack` **0x7A5C99** | `SummonedAttackHandler` |

**这两个方向不是"同一个包的回环"。** 客户端→服务端是"我打了谁、每行多少伤害"；
服务端→旁观者是"这个伤害带不带暴击"。攻击包**不携带暴击位**，也不携带 `curHP/maxHP`。

---

## 2. 公共包头（三种攻击共用）

以 `TryDoingMeleeAttack` 0x952857 起的实际指令为准
（`TryDoingShootAttack` 0x955150、`TryDoingMagicAttack` 0x956DA2 结构一致）：

```c
COutPacket(0x2C);                                  // 186
Encode1(*(get_field() + 0x134));                   // ① 平台标志
Encode1((numAttacked << 4) | numDamage);           // ② numAttackedAndDamage
Encode4(nAction);                                  // ③ nAction（动作 id）
Encode4(a2 ? sub_75C017(skill) : 0);               // ④ skillID（SKELLENTRY+0x34；无技能=0）
if (is_keydown_skill(skill)) Encode4(keydown);     // ⑤ 仅 keydown 类技能
Encode1(display);                                  // ⑥
Encode1(direction);                                // ⑦
Encode1(stance);                                   // ⑧
Encode1(attack_speed_degree);                      // ⑨
Encode4(...);                                      // ⑩ 远程包此处为弹道坐标×2×short + 投掷物 id
```

服务端 `parseDamage`（`AbstractDealDamageHandler.cs:603` 起）读取顺序：

```csharp
p.readByte();                                  // ①
ret.numAttackedAndDamage = p.ReadSByte();      // ②
ret.numAttacked = (numAttackedAndDamage >> 4) & 0xF;
ret.numDamage  = numAttackedAndDamage & 0xF;
ret.skill = p.readInt();                       // ③④ 合并：只把 nAction 之后那个 int 当 skill
if (skill 在 BIG_BANG/GRENADE/CORKSCREW/ICE_BREATH/FIRE_BREATH/POISON_BOMB) ret.charge = p.readInt();
p.skip(8);                                     // ⑤⑥ 的定长部分
ret.display = p.ReadSByte();                   // ⑦
ret.direction = p.ReadSByte();                 // ⑧
ret.stance = p.ReadSByte();                    // ⑨
```

**这里有一个需要留意的不对称**：客户端在 ③ 写 `nAction`、④ 写 `skillID`，是两个 `int`；
服务端只在 ③ 之后读**一个** `int` 并当作 `skill`。`p.skip(8)` 又把 ⑤⑥ 剩余部分吞掉。
字节数上恰好对得上（4+4 = 服务端 readInt(4) + 另一处 skip），但**语义上服务端拿到的是
`nAction` 还是 `skillID` 取决于两者是否相等**——这一点在 §5 标为待核实。

> 现有服务端能跑，说明 `nAction` 与 `skillID` 在绝大多数情况下同值或服务端容忍；
> 但这是"字节对齐"而非"字段对齐"，改动时不要按服务端字段名去推客户端。

### 2.1 `is_keydown_skill`（0x4FB08F）

```c
return skill == 0x4DD5CC || skill == 0x4F5C6A || skill == 0x4FAA8C ||
       skill == 0xC80EDA || skill == 0xD7511E || skill == 0xE66C4B ||
       skill == 22121000 || skill == 22151001 ||
       skill == 2221001  || skill == 1121001  || skill == 1221001 ||
       skill == 1321001  || skill == 2121001  || skill == 2321001 ||
       skill == 3121004  || skill == 3221001;
```

前几个 `0x4DD5CC` / `0x4F5C6A` 等是 IDA 把立即数当指针渲染的结果（对应 1009、1020、
20001009 一类"姿势/摇杆"技能 id）。服务端 `parseDamage` 的 charge 列表是
`Evan.ICE_BREATH / Evan.FIRE_BREATH / FPArchMage&ILArchMage&Bishop.BIG_BANG /
Gunslinger.GRENADE / Brawler&ThunderBreaker.CORKSCREW_BLOW / NightWalker.POISON_BOMB`。

**两个集合不一致**：客户端还包括 1020/1009 系列、火毒毒雾一类；服务端还包括
`CORKSCREW_BLOW`（客户端 `is_keydown_skill` 里看不到 1321001 以外的对应值）。
差集会让该包在这一段整体错位 4 字节。**这是现存风险点，见 §5。**

---

## 3. 每目标段（`numAttacked` 次）

`TryDoingMeleeAttack` 0x9529D3 起（Shoot 0x9552F8、Magic 0x956F11 同）：

```c
Encode4(_ZtlSecureFuse(mob + 0x17C, mob[0x184]));   // ① oid
Encode1(target[1]);                                  // ② 受击动作编号
Encode1((sub_663FE2(mob) << 7) | target[2] & 0x7F);  // ③ 高1位状态 + 低7位当前动作
Encode1(target[3]);                                  // ④ 当前动作索引
Encode1((v191 << 7) | mob[0x4C4] & 0x7F);            // ⑤ 高1位标志 + 低7位 mob 字段
Encode2(target[4]);                                  // ⑥ 受击动画延迟 (ms)
Encode2(mob.GetPos().x); Encode2(mob.GetPos().y);    // ⑦ 当前位置
Encode2(mob.GetPrevPos().x); Encode2(...y);          // ⑧ 上一位置
Encode2(mob[8] & 0xFFFF);                            // ⑨ 某 mob 字段
for (i = 0; i < numDamage; i++)
    Encode4(target[6 + i]);                          // ⑩ 纯伤害，无 bit31
Encode4(sub_6711AC(mob));                            // ⑪ mob 模板 CRC32
```

target 结构（`CalcDamage::PDamage` 的输入，152 字节 / 38 dword）：

| 偏移 | 内容 |
|------|------|
| `+0x00` | `CMob*` |
| `+0x04` | `sub_668AAD(mob)` — 受击动作编号（`rand() % 模板+0x220 + 6`，`<=0` 时 -1） |
| `+0x08` | `CMob::GetCurrentAction(mob)` — 当前动作 |
| `+0x0C` | `sub_7A5A58(mob)` — 动作索引 |
| `+0x10` | 受击动画延迟（short，`update_time + 延迟` 用于 `sub_66B05E`） |
| `+0x18` | `int damage[numDamage]`（`PDamage` 的 `a16`，`a4+24`） |
| `+0x54` | `int crit[numDamage]`（`PDamage` 的 `a17`，`a4+84`） |

### 3.1 字段来源与置信度

| # | 来源 | 语义 | 置信度 |
|---|------|------|--------|
| ② | `target[1] = sub_668AAD(mob)`（0x668AAD） | 受击动画索引 Hit0/Hit1/…（`rand() % *(模板+0x220) + 6`） | 高 |
| ③ | 高1位 `sub_663FE2(mob)`（0x663FE2）<br>低7位 `target[2] = GetCurrentAction` | `sub_663FE2` 返回 `模板+0x1A0` 或 `mob[237] & 1` 的 bool，字段未命名 | 中 |
| ④ | `target[3] = sub_7A5A58(mob)` → `sub_7A964D(mob[249])` | 当前动作在动作表中的索引（找不到 = -1） | 中高 |
| ⑤ | 高1位 `v191 = mob[0x188] != mob[0x18C]`（受 `mob[0x2A4]` 门控）<br>低7位 `mob[0x4C4] & 0x7F` | 注释：来自 CMob 而非 target | 中 |
| ⑥ | `target[4]`（`[edi+0x10]`，按 short 写出） | 受击动画延迟（`v309 + 70*idx` 一类） | 中高 |
| ⑪ | `sub_6711AC` → `sub_6711D6` → `sub_6814C3` | **mob 模板 CRC32**：对模板的 `+0x1E0`、`+0x1C0/0x210`、`+0x9D0`、攻击信息数组、掉落数组逐段 CRC32，`!= -1` 时 `+1`；带脏标记缓存 | 高 |

服务端对应（`AbstractDealDamageHandler.cs:876`）：

```csharp
int oid = p.readInt();
p.skip(4);              // ②③④⑤ 四个字节
var curPos  = p.readPos();   // ⑦
var nextPos = p.readPos();   // ⑧
short delay = p.readShort(); // ⑥ 之后的 (9)?? 见下
for (j < numDamage) damage = p.readInt();
p.skip(4);              // ⑪ mob 模板 CRC32
```

> **⑥⑨ 的顺序需要服务端实测**：客户端在 4 字节之后写「4 个 short（两组坐标）+ short(target[4]) + short(mob[8])」，
> 而服务端读的是「readPos ×2 + readShort」。若服务端读到的 delay 实际是 `target[4]` 还是第 5 个 short，
> 取决于两者排列；这一段只核对了字节数（4 + 12 = 16，服务端 4 + 8 + 2 = 14 + 后面伤害前还有 2），
> 未逐字段确认，标为待核实。

两个要点：

1. **伤害行是 `target[6+i]`**，由 `CalcDamage::PDamage` 写入的纯数值；暴击标记在
   `target[21+i]`，**从不写入包**（相关文档 §2.3.1）。
2. **Meso Explosion(4211006) 是唯一例外**：oid 之后先读 1 字节，再 `byte 行数`，再逐行
   `Encode4`，没有尾部 CRC。服务端 `parseMesoExplosion`（`AbstractDealDamageHandler.cs:1059`）
   的 `ReadSByte` 行数 + `skip(4)` 与之对应。

### 3.2 与"服务端 → 旁观者"格式不同

接收侧 `CUserRemote::OnAttack`（0x98051F）对每个目标只读 **oid(4) + 1 字节 + 4×numDamage**：

```c
98052a call Decode4            ; oid
98053b call Decode1            ; 只有 1 个字节
980540 cmp [ebp+var_10], loc_40413E   ; Meso Explosion?
98054d jnz loc_98060D
980614 add esi, 18h            ; 跳到伤害数组
980619 call Decode4            ; 伤害（含 bit31）
```

即**两个方向的目标段长度不同**：客户端→服务端带 4 字节 + 坐标 + 延迟 + CRC，
服务端→旁观者只带 `oid + 1 字节 + 伤害`。因此服务端 `PacketCreator.addAttackBody`
是在**重新构造**精简包，而不是转发原始包——这也解释了为什么 ②③④⑤、坐标、⑥⑨、⑪
在服务端都是 `skip` 掉。

---

## 4. 包尾

三种攻击在目标循环后都由 `sub_952B58`/`955454`/`957163` 一带写：

```c
Encode2(chr.x); Encode2(chr.y);
Encode2(?);     Encode2(?);
```

服务端对应 `p.skip(4)`（近距离/远程在 `ret.skill != RAPID_FIRE ...` 时）以及
`POISON_BOMB` 的 `readShort` 坐标。魔法包额外在包头写了 `charge`、包尾写了
`Encode4(...)`（keydown 类），服务端 `magicAttack` 的 `if (charge != -1) writeInt(charge)` 与之对应。

---

## 5. 未确认 / 待核实

1. **③④ 两个 `int` 的语义**：客户端写 `nAction`（动作 id）与 `skillID`；服务端只读一个 int
   当 `skill`。需在 IDA 里确认 `nAction` 与 `sub_75C017(skill)` 在何种情况下不等，
   以及服务端 `p.skip(8)` 是否正好覆盖了多读/少读的 4 字节。
2. **`is_keydown_skill` 集合与服务端 charge 列表的差集**（§2.1）：两者不一致，可能导致
   这一段错位。这是目前最值得实测的一项。
3. **远程包 187 的弹道字段**：客户端 0x955259–0x9552BB 写
   `Encode2(ProperBulletPosition)`、`Encode2(...)`、`Encode4(投掷物)` 等；服务端
   `ranged` 分支用 `skip(7)` + 条件 `skip(4)` 对齐，未逐字段命名。
4. **目标段 ⑥~⑨ 的排列**：客户端是「4 short 坐标 + short 延迟 + short mob[8]」，
   服务端是「readPos ×2 + readShort」，仅核对了总长度，未逐字段确认哪个 short 落到
   `delay` 上（见 §3.1 注）。
5. **BODY_ATTACK(189)**：`TryDoingNormalAttack` 0x95719B 尚未展开。
6. **召唤兽攻击**：`CSummoned::TryDoingAttack`(0x7A5C99) / `ProcessAttack`(0x7A4412) 尚未展开。

> 本文档只记录**客户端发包侧**已经核对过的结构；服务端解码逻辑未做修改。
