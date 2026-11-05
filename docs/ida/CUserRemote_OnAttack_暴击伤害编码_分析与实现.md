# 攻击包伤害行的 bit31 = 暴击（CUserRemote::OnAttack）分析与服务端对照

> IDA 数据库：`` `Angel.i64 ``（v83，基址 `0x400000`）
> 服务端：`src/Application.Core/Channel/Net/Handlers/AbstractDealDamageHandler.cs`、
> `src/Application.Core/Tools/PacketCreator.cs`
> 关联文档：`docs/ida/CUserLocal_攻击包_客户端发包结构_分析.md`（同一组包的**发送侧**）、
> `docs/ida/CUserRemote_DecodeForRemote_分析与实现.md`、`docs/ida/CMob_OnStatSet_分析.md`

---

## 1. 结论速览

| 项目 | 客户端实测 |
|------|-----------|
| 攻击包入口 | `CUserRemote::OnAttack`（**0x9803AB**），opcode 186/187/188/189 → `OnMeleeAttack` / `OnShootAttack` / `OnMagicAttack` / `OnBodyAttack` |
| 伤害行解码 | `Decode4()` 后：`crit = v >> 31`、`damage = v & 0x7FFFFFFF`（**0x980617–0x980631**） |
| bit31 语义 | **暴击标记**，不是数值符号 |
| 低 31 位语义 | 该条伤害的数值 |
| 本地发包侧 | `CalcDamage::PDamage`（**0x78DF87**）把伤害写 `target[6..]`、暴击标记写 `target[21]`（= 结构体偏移 0x54 / 84） |
| 本地发包编码 | `TryDoingMeleeAttack`/`TryDoingShootAttack`/`TryDoingMagicAttack` 只 `Encode4` 伤害，暴击标记不出现在线上包中 |
| 显示侧 | `CMob::ShowDamage`（**0x6691D3**）把"是否暴击"传给 `CAnimationDisplayer::Effect_HP`（**0x437D0F**）的第 6 参，走独立的数字图与 `+0x1E` 偏移 |
| `DAMAGE_MONSTER`(246) | 走 `CMob::OnDamaged`（**0x66C6C2**），**负数是治疗**，与攻击包的 bit31 语义冲突 |

**判定：服务端用 `damage + int.MinValue` 表示暴击，语义上是对的，与客户端接收侧完全对应；
但 `AbstractDealDamageHandler.DecodeDamage` 的还原公式与客户端不一致（少 1），且用加法实现有溢出风险。**

---

## 2. 客户端证据

### 2.1 包来源与分发

`CMobPool::OnMobPacket`（0x67936D）只管怪物包；玩家攻击包走 `CUserPool::OnUserRemotePacket`
之后进入 `CUserRemote::OnAttack`（0x9803AB）。其分发尾部：

```c
switch (a2) {
  case 186: CUserRemote::OnMeleeAttack (this, ...); break;   // CLOSE_RANGE_ATTACK
  case 187: CUserRemote::OnShootAttack (this, ...); break;   // RANGED_ATTACK
  case 188: CUserRemote::OnMagicAttack (this, ...); break;   // MAGIC_ATTACK
  case 189: CUserRemote::OnBodyAttack  (this, ...); break;   // BODY_ATTACK
}
```

`OnAttack` 头部按 `numAttackedAndDamage` 拆出 `numAttacked` 与 `numDamage`：

```c
v5 = Decode1();          // numAttackedAndDamage
v6 = v5 >> 4;            // numAttacked
v62 = v5 & 0xF;          // numDamage（每条攻击的伤害行数）
...
v63 = (Decode2() >> 15) & 1;   // 某一标志（stance 高位）
v66 = v66_ & 0x7FFF;            // stance
```

### 2.2 伤害行解码：bit31 = 暴击

每个目标的伤害行在 0x980572–0x980631。注意 `&loc_40413E` 是 IDA 把 32 位立即数
（`0x40413E` = **4211006 = Meso Explosion**）当成代码指针显示的结果，相邻的
`&loc_40413C` = 4211004 = Band of Thieves 也佐证了这一点，所以它**不是**"普通攻击"分支：

```c
if ( v70 == &loc_40413E ) {         // Meso Explosion：每行原样读，无 bit31
    v18 = Decode1();
    v14[5] = v18;                   // 行数
    do {
        *v64 = Decode4();           // 伤害，不解 bit31
        ...
        ++v64;
    } while (...);
}
else {                              // 其它所有攻击（含 skill==0 的普攻）
    v21 = v62;                      // numDamage
    if ( v62 > 0 ) {
        v22 = v14 + 6;              // target 结构的伤害数组起点
        do {
            v23 = Decode4();
            v22[15] = v23 >> 31;            // ← 暴击标记 → target[21]
            *v22++  = v23 & 0x7FFFFFFF;     // ← 伤害值   → target[6..]
            --v21;
        } while ( v21 );
    }
}
```

反汇编（0x980617）：

```
980619  call    Decode4
980620  shr     ecx, 1Fh        ; crit = bit31
980623  and     eax, 7FFFFFFFh  ; damage = low31
980628  mov     [esi+3Ch], ecx  ; target[15+6] = target[21] = crit
98062b  mov     [esi], eax      ; target[6..]   = damage
```

即客户端把**线上 4 字节**解释为 `(crit << 31) | damage`，`crit` 与 `damage` 是两个独立字段。

### 2.3 本地发包侧：同一套"伤害 + 暴击"双数组，暴击标记**不上线**

`CalcDamage::PDamage`（0x78DF87）的签名里 `a16`/`a17` 是两个 `int[]`：
`a16` 每行写伤害、`a17` 每行写"这一行是否暴击"。反编译中的两处写入：

```c
/* 命中暴击率判定后： */
*(a17 + 4 * nItemID) = 1;          // 行 1498（全屏技/普通暴击）
...
*(a17 + 4 * nItemID) = 1;          // 行 1390（另一暴击分支）
```

调用点 `CUserLocal::TryDoingMeleeAttack`（0x950921）第 1159–1180 行传参：

```c
CalcDamage::PDamage(
    ...,
    a4 + 24,        // a16 = target 结构里的伤害数组（target[6..]）
    a4 + 84,        // a17 = target 结构里的暴击数组（target[21..]）
    ...);
```

`a4` 是单目标结构体（152 字节），`+24` = `target[6]`、`+84` = `target[21]`，与 2.2 中接收侧
`v14 + 6` / `v14[15]`（即 `v14[21]`）**完全同一布局**——接收侧把线上 bit31 拆回本地结构里
PDamage 产出的那两个字段。

本地发包时只编码伤害：`TryDoingMeleeAttack` 0x95285F `COutPacket(44)`，0x952B1D 循环
`Encode4(target[6 + i])`；`TryDoingShootAttack` 0x955150 `COutPacket(45)`，0x955423 循环；
`TryDoingMagicAttack` 0x956DA2 `COutPacket(46)`，0x957056 循环。**本地客户端自己的包里不含 bit31**
（自己已经用 `a17` 直接显示了暴击数字），bit31 只用于"转发给其他玩家时让别人的客户端也显示出暴击"。

### 2.3.1 信息流向：暴击标记只存在于"服务端 → 旁观者"方向

把两侧合起来看，暴击标记的生命周期是：

```
攻击者客户端 ──(伤害值, 无暴击标记)──> 服务端
                                     │  只能推断：canCrit && damage > hitDmgMax
                                     │  重新编码：damage | bit31
                                     ▼
                          旁观者客户端 ──(bit31 = 暴击)──> 显示暴击数字
```

- **攻击者 → 服务端**：`target[21]` 从不写入包，全函数位扫描无任何 `or …, 80000000h`
  （`TryDoingMeleeAttack`/`Shoot`/`Magic` 三处均无）。服务端在协议层面**拿不到真实暴击**。
- **服务端 → 旁观者**：这一层才有 bit31，且完全由服务端决定；旁观者客户端无条件相信这个位。

所以服务端用"伤害是否超过非暴击上限"来推断，不是被协议逼出来的解码需求，而是**单方面
为旁观者补上展示信息**；推断错只影响旁观者看到的暴击数字，不影响服务端的伤害结算。

### 2.4 显示侧：暴击数字

`CMob::OnHit`（0x668B83）在 0x668DFA–0x668E0D 调用：

```c
ShowDamage(this, a6 /*damage*/, a8 /*行号*/, a7 /*是否暴击*/, a3 == (&loc_4E23EA+2), ...);
```

`CMob::ShowDamage`（0x6691D3）：

```c
if ( nDamage <= 0 ) {
    if ( nDamage )  Effect_HP(..., -nDamage, 1, 0);   // 蓝色数字（回复）
    else            Effect_Miss(...);                  // MISS
} else {
    Effect_HP(..., nDamage, 0, a4);                    // a4 = 是否暴击
}
```

`CAnimationDisplayer::Effect_HP`（0x437D0F）：

```c
v8 = lY != 0 ? 0x1E : 0;      // 暴击数字整体下移 30px
```

并按 `a5`（0/1/2）选择不同的数字图池（0x437D0F 里 `sub_4027BE(*(this+93))` /
`*(this+95)` / `*(this+97)` / `*(this+99)` 四组）。构造 `CAnimationDisplayer::CAnimationDisplayer`
（0x435444）依次从 `Effect/BasicEff.img` 下取 `NoRed0/NoRed1/NoBlue0/NoBlue1/NoViolet0/
NoViolet1/NoCri0/NoCri1`（SP_969–SP_976）——说明"暴击"确实对应独立的 `NoCri*` 资源分支。

> 未完全确认：`Effect_HP` 里 4 组池与 `NoCri*` 的精确索引对应关系。可确认的是
> **`lY != 0`（即 ShowDamage 的 a4，暴击标记）走的是与普通伤害不同的数字图与位置**。

### 2.5 `DAMAGE_MONSTER`(246) 的负数语义不同

`CMobPool::OnMobPacket`（0x67936D）`case 246 → CMob::OnDamaged`（0x66C6C2）：

```c
CInPacket::Decode1(a2);              // 占位
v3 = CInPacket::Decode4(a2);         // 伤害，按有符号使用
CMob::ShowDamage(this, v3, 0, 0, 0, ...);
```

`ShowDamage` 对 `nDamage < 0` 走 `Effect_HP(..., -nDamage, 1, 0)`（蓝色回复数字）。
服务端 `PacketCreator.healMonster(oid, heal, ...)` 正是发 `-heal`，与之吻合。

**因此：bit31 表示暴击只对 186/187/188/189 攻击包成立；246 包里的负数表示治疗，
两种语义互斥，不能混用。**

### 2.6 服务端包里的两个固定字节：`0x0A` 与 `0x0`

服务端 `PacketCreator.addAttackBody` 写了两个**固定值**，客户端把它们当普通字段读走：

#### `0x0A` —— 写死在“技能段”里，客户端当作 afterimage 编号

服务端顺序（`PacketCreator.cs:2147`）：`display, direction, stance, speed, 0x0A, projectile`。
客户端 `CUserRemote::OnAttack` 0x9804D7 按 **1 + 2 + 1 + 1 + 4** 读同一块：

| 服务端 | 客户端 | 去向 |
|--------|--------|------|
| `display` | `Decode1 & 0x20` | 仅本函数内判断技能特效 |
| `direction` + `stance` | `Decode2`：低 15 bit = stance，bit15 = 标志 | `OnMeleeAttack` 的 `a6`/`a7` |
| `speed` | `Decode1` | `SetAttackAction` / `ShowSkillEffect` |
| **`0x0A`** | `Decode1` → `v61` | `OnMeleeAttack(..., a8 = v61)` |
| `projectile` | `Decode4` | 远程弹道 |

`v61` 进入 `sub_936F34(&v82, a4, a5, v67, v61)`，该函数把参数与字符串池
`SP_2431_CHARACTER_AFTERIMAGE_` 拼成 `Character/Afterimage/<n>`：

```c
StringPool::GetBSTR(Instance, &dword_BF122C, SP_2431_CHARACTER_AFTERIMAGE_);
_itow(a5, &v11, 10);                 // a5 = 该字节
sub_413878(&dword_BF122C);           // "Character/Afterimage/" + 数字
```

即 **`0x0A` = 残影图号 10**，服务端写死，旁观者看到的攻击残影固定取 `Character/Afterimage/10`。
本地客户端在这一位写的是 `v291`（0/1 布尔），两者本就不是同一语义——服务端只需保证
字节位置对得上。

#### `0x0` —— 目标段 oid 之后那个字节，客户端当作“受击动作”

客户端接收（0x98051F）：

```asm
98052a Decode4   ; oid
98053b Decode1   ; ← 这一个字节
98054a mov [esi+4], eax     ; target[1]
```

`target[1]` 随后作为 `sub_66B05E` 的参数传下去（受击动画/伤害数字注册）。
而**客户端自己发包时这一位写的是 `target[1] = sub_668AAD(mob)`**——真实的受击动作 id
（`rand() % *(模板 + 0x220) + 6`，`<= 0` 时为 -1）。

所以服务端的 `0x0` 表示“受击动作 0 / 无动作”：旁观者不会看到与攻击者一致的受击动画。
这是服务端重造包时的简化，属表现层，不影响结算。

---

## 3. 与服务端实现的对照

> **2026-11-02 修订**：服务端已重构为「解析期只保留语义、发包期才编码」：
> `AttackDamage(int Damage, bool IsCrit)` 取代了"用负数承载暴击"的旧做法。
> §3.1–§3.3 记录重构前的问题与最终方案。

### 3.1 重构前：用"负数"承载暴击，语义泄漏到整个链路

旧实现在 `parseDamage` 里就把暴击写进数值：

```csharp
if (ret.skill == Marksman.SNIPE || (canCrit && damage > hitDmgMax))
    damage = damage + int.MinValue;      // 0x7FFFFFFF 取反式编码
```

这个值此后同时充当"数值"和"标记"，导致下游每个消费者都得自己解一遍：

| 位置 | 旧写法 | 问题 |
|------|--------|------|
| `DecodeDamage` | `tempInt += int.MaxValue` | **比客户端 `& 0x7FFFFFFF` 少 1** |
| `applyAttack` 的 `totDamageToOneMonster` | 手写 `if (<0) tempInt += int.MaxValue` | 又一份重复的还原公式 |
| PickPocket | `eachd + int.MaxValue` 后再判负 | 同一公式第三遍，且对正数行会误加偏移 |
| `CloseRangeDamageHandler`（SACRIFICE 反噬） | 直接取 `damageLines.First()` | 暴击行是负数 → 反噬变回血 |
| `PacketCreator.addAttackBody` | 原样 `writeInt` | 只有这里真的需要 bit31 |

对 `0 <= damage <= int.MaxValue`，`damage + int.MinValue` 的位模式确实等于
`(uint)damage | 0x80000000`，客户端 `crit = v >> 31; damage = v & 0x7FFFFFFF` 可逆，
**编码方向本身没错**；错在把"传输编码"当成了"内存表示"。

### 3.2 重构后：语义与编码分离

- `AttackTarget.damageLines` 改为 `List<AttackDamage>`，`AttackDamage` = `(int Damage, bool IsCrit)`；
- 解析期只置 `isCrit = true`，数值保持客户端显示值，**零还原公式**；
- 只有在 `PacketCreator.addAttackBody` 写包时调用一次
  `AttackInfo.EncodeCritDamage` → `IsCrit ? unchecked(Damage + int.MinValue) : Damage`。

这样与服务端实际处理的数据包结构一致（客户端本身就是"伤害 + 暴击"两个独立字段，见 §2.3），
并且 `DecodeDamage`、PickPocket 里的重复公式、SACRIFICE 的负数泄漏一并消失。

**遗留注意**：`PacketCreator.addAttackBody` 是唯一出口，若以后新增别的"把伤害转发给客户端"
的包（例如 `damageMonster`），需要自己判断是否要带 bit31——但按 §2.5，`DAMAGE_MONSTER`
的负数语义是**治疗**，那里**不能**用同一套编码。

### 3.3 暴击行的数值不再被"服务端减 1"

重构后 `totDamageToOneMonster` 直接来自 `AttackDamage.Damage`，与客户端显示值一致。
之前"暴击行少 1 点"的问题随之消失，无需再改 `DecodeDamage`。

### 3.4 服务端的推断条件与客户端不一致（已由客户端补丁解决）

客户端**不根据数值推测暴击**：本地由 `CalcDamage::PDamage` 用暴击率判定，把结果写进
`a17`；接收侧直接读 bit31。原协议下攻击包不带 `target[21]`（见 §2.3.1），
服务端只能用 `canCrit && damage > hitDmgMax` 猜。

> **2026-11-04 更新**：已通过客户端补丁
> （`Client::EnableCritFlagInAttackPacket`）
> 在发包前把 `target[21+i]` OR 进伤害值的 bit31，服务端相应改为拆位解码
> （`AbstractDealDamageHandler.parseDamage`）。现在暴击是**客户端给出的真值**，
> 不再需要任何推断。

#### SNIPE（3221007）的真实行为

`CMob::OnHit`（0x668B83）在显示层有一处特判：

```c
if ( a3 != 1121001 && a3 != 1221001 && a3 != 1321001 )
{
    if ( a3 == 3221007 )      // 0x668DD8
        a7 = 1;               // 强制"暴击"数字图
    CMob::ShowDamage(v20, a6, a8, a7, ...);
}
```

**但这只是旁观者看到的数字图样式，与实际暴击无关。**

`CalcDamage::PDamage` 里 SNIPE 的固定伤害分支会**整段跳过暴击判定**：

```asm
0x78E4DD  cmp [ebp+arg_24], 1     ; a11 == 1
0x78E4E1  jnz 0x78E647            ; 否则走正常流程
0x78E4E7  ... 算 199999 - roll ...
0x78E552  jmp 0x7905F8            ; 直接写伤害，跳出目标循环
```

而 `target[21]` 在循环开头已初始化为 0（`0x78E3E5 mov [ebx], ecx`，`ecx=0`），
所以 **SNIPE 发出去的暴击位恒为 0**。

**结论**：服务端对 SNIPE **不应**无条件置位 bit31——那会与客户端发出的真值矛盾。
当前实现已删除该特判，与客户端保持一致。

### 3.5 `damageLines` 的语义

`attack.targets[...].damageLines` 现在是 `AttackDamage(Damage, IsCrit)`，**数值永远是客户端
显示的非负值**，暴击是独立布尔位。所有拿它做算术的地方都不再需要解码：

- `applyAttack` 的 `totDamage` / 吸血 / PickPocket / 连击统计 — 直接用 `Damage`；
- `CloseRangeDamageHandler`（SACRIFICE 反噬）— 直接用 `Damage`；
- `PacketCreator.addAttackBody` — 唯一需要编码的位置（`EncodeCritDamage`）。

反例仍然成立：**任何直接调用 `PacketCreator.damageMonster(oid, damage)` 的地方都不该塞
bit31**，因为 `DAMAGE_MONSTER` 的负数是治疗（见 §2.5）。

---

## 4. 结论

1. **"用 `damage + int.MinValue` 表示暴击"是正确的**，并且是 v83 客户端唯一能识别的
   暴击传递方式：`CUserRemote::OnAttack` 对每条伤害行做 `>>31` / `& 0x7FFFFFFF` 拆分。
2. 该编码**只应在发包时施加**。服务端内存里保留 `(Damage, IsCrit)` 两个字段，
   不要在解析期把标记揉进数值——否则下游每个消费者都要重复一遍解码（旧实现即如此，
   且 `+ int.MaxValue` 的还原公式比客户端少 1）。
3. 该标记只影响**旁观者**看到的暴击数字，不参与任何伤害结算。
4. **该标志只允许出现在 186/187/188/189 攻击包里**；`DAMAGE_MONSTER(246)` 的负数是
   治疗语义。
5. 用"伤害超过非暴击上限"来推断暴击是服务端单方面的近似：协议里攻击包**不携带**暴击标记
   （§2.3.1），服务端无法得到真值。

> 本次分析**未修改服务端逻辑**，只补充了文档；代码侧的重构由仓库作者完成
> （`AttackDamage` / `EncodeCritDamage`）。

---

## 5. 未确认 / 待验证

- `CAnimationDisplayer::Effect_HP` 里 4 组数字图池（`this+92/93`、`94/95`、`96/97`、
  `98/99`）与 `BasicEff.img/NoRed*`、`NoBlue*`、`NoViolet*`、`NoCri*` 的精确索引对应，
  只确认了构造函数按 SP_969–SP_976 顺序加载这 8 个资源、`lY != 0` 走独立分支且 `+0x1E`。
- `CMob::OnHit` 的 `a7`（暴击）在 `sub_66B05E` 之后的完整传递链（`v20[33]` → OnHit 第 7 参）
  只验证到 `v133[15]`（= `target[21]`）被作为 `a8` 传入 `sub_66B05E` 并存入偏移 0x48。
- 魔法攻击：`CalcDamage::MDamage`（0x791617）中没有对 `a17` 的暴击写入，客户端魔法是否
  存在暴击仅由服务端 `canCrit` 判断（法师不在列表内），未进一步验证。
- 服务端写死的 `0x0`（受击动作）在 `sub_66B05E` → `CMob::OnHit` 链路上最终落到
  “受击动画索引”还是“多段伤害的垂直间距”，只追到 `sub_66B05E` 把它存进偏移 0x48，
  未追到 `ShowDamage` 的第 3 参。
