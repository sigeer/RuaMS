# CTextAnalyzer 文本换行（v83）

> IDA 数据库：`` `Angel.i64 ``（v83 客户端，`` `Angel.exe ``，映像基址 `0x400000`，代码段 `0x401000` 起）
> 服务端：`src/Application.Core/Channel/ClientCulture.cs`、`src/Application.Resources/Messages/*.resx`、
> `src/Application.Core/Channel/Net/Packets/ScriptPackets.cs`
> 关联：`docs/ida/UseItem.md`（同一次会话里的客户端结构说明）

本文只回答一个问题：**NPC 对话文本里，什么字符才会让客户端换行**。
起因是 `ScriptTalk.PartyQuest_Requirement` 等多行文案在 resx 里改成"物理换行"后，
客户端显示成一行（换行丢失）。

---

## 1. 结论速览

| 项目 | 客户端实测 |
|------|-----------|
| 解析函数 | `CTextAnalyzer::AnalyzeText`，`0x99DD54`，大小 `0x47BB` |
| 调用链 | `CScriptMan::OnScriptMessage`（`0x74660A`）→ `CScriptMan::OnSay`（`0x7467AB`）→ `CUtilDlgEx::SetUtilDlgEx`（`0x9AC552`）→ `AnalyzeText`（`0x9AC666` 处调用） |
| 换行字符 | **`0x0D`（CR）** 才是换行。`0x99E187` `cmp al, 0Dh` |
| `0x0A`（LF） | **不是分隔符，也不会换行**。整个 `AnalyzeText` 内没有任何与 `0x0A` 的比较（按字节模式扫描 `3C 0A / 83 F8 0A …` 全函数只命中 `0x99E841` 的 `cmp eax, 0Ah`，那是光标/排版宽度常量，不是字符比较） |
| `0x5C`（`\`） | 与 `0x0D` **走同一分支**：`0x99E18F` `cmp al, 5Ch`，与 `0x99E187` 一样跳到 `loc_9A2115`。因此字面量反斜杠也会触发换行处理，后面的 `r`/`n` 由后续的转义解析消耗 |
| `#` | 不是换行，是颜色/格式指令（`#b`/`#k`/`#L..` 等），由同一分析器处理 |
| 文本解码 | `CInPacket::DecodeStr`（`0x46F30C`）→ `CIOBufferManipulator::DecodeStr`（`0x46F37B`）：**1 字节长度 + 原始字节**，无转义、无编码转换（服务端按 GBK 写入，见 `Application.Utility.GlobalVariable.Encoding`） |

因此：

- **真实 CRLF（`0D 0A`）** → 换行正常（`0D` 被当分隔符，`0A` 作为普通字节被吞在行尾）。
- **裸 LF（`0A`）** → 不换行，直接当作普通字符绘制 → 多行文案粘成一行（本次"换行丢失"）。
- **字面量 `\r\n`（`5C 72 5C 6E`）** → 走 `0x5C` 分支，效果等价于换行；旧版 resx 正是这种写法，所以一直"看起来正常"。

## 2. 关键地址与证据

### 2.1 包结构（`CScriptMan::OnScriptMessage` 0x74660A）

```
Decode1  → v4        // 包内第一个字节
Decode4  → v7        // npc id 等
Decode1  → nMsgType  // 0=Say, 1=YesNo, 4=Menu, 12=AcceptDecline ...
Decode1  → speaker
switch (nMsgType) { case 0: OnSay(...) }
```

`OnSay`（0x7467AB）继续 `DecodeStr` 取正文，再交给 `CUtilDlgEx`。
对应的服务端发包在 `ScriptPackets.CreateNpcTalk`（`writeByte(4); writeInt(npc); writeByte(msgType); writeByte(speaker)`），
字段顺序与客户端解码一致。

### 2.2 换行判定（`AnalyzeText` 0x99DD54）

```
0x99E162  ...
0x99E175  cmp eax, esi        ; token 为空?
0x99E17D  mov al, [eax]
0x99E181  jz  loc_9A2138      ; 结束
0x99E187  cmp al, 0Dh         ; CR
0x99E189  jz  loc_9A2115      ; → 换行
0x99E18F  cmp al, 5Ch         ; '\'
0x99E191  jz  loc_9A2115      ; → 同一"换行/转义"入口
```

`loc_9A2115` 给当前 `CT_INFO` 打上换行标记：

```
0x9A2125  imul eax, 44h
0x9A2128  mov dword ptr [eax+ecx-14h], 1
```

其它相关辅助：

| 地址 | 作用 |
|------|------|
| `0x9A264F` | 取下一个 token；首字符为 `0x0D`（`0x9A269C`）或 `0x5C`（`0x9A26A6`）时走 `loc_9A2707`，为 `#`（`0x9A26A1`）时走 `loc_9A26BB` |
| `0x9A2785` | 用函数指针 `dword_BF048C`（运行时填充）定位下一个特殊字符并切片 |
| `0x9A2A2C` / `0x9A283B` | 普通文本 / 反斜杠序列的消费；`0x9A29EE`、`0x9A29F3`、`0x9A29F8` 分别以 `0x0D`、`#`、`\` 结束当前片段 |
| `0x9A262F` / `0x9A264F` 附近 | 对话框排版宽度（`cmp eax, 0Ah`、`cmp ..., 64h` 等），与字符比较无关 |

> 尚未确认：`dword_BF048C` 指向的具体函数（静态分析里是 0，运行时才写入）。
> 但"`0x0D` 与 `0x5C` 同分支、`0x0A` 无比较"这一结论来自指令本身，不依赖该函数。

## 3. 与服务端 resx 的对应关系

客户端只认 `0x0D`（以及 `0x5C`），而 resx 的值是 **XML 文本原样**，MSBuild 的
`GenerateResource` 不做 C# 字符串转义。两种写法的编译结果（已用最小工程验证）：

| resx 里的写法 | 编译进 .resources 的字节 | 客户端结果 |
|---------------|-------------------------|-----------|
| `<value>a\r\nb</value>`（字面反斜杠） | `61 5C 72 5C 6E 62` | `\` 触发换行分支，能换行（但依赖转义解析） |
| `<value>a&#13;&#10;b</value>` | `61 0D 0A 62` | 正常换行，与文件 EOL 无关 |
| 值里写物理换行（编辑器回车） | 取决于**文件字节**：CRLF 文件 → `0D 0A`；LF 文件 → `0A` | CRLF 正常；**LF 不换行** |
| `<value>a\nb</value>`（字面 `\n`） | `61 5C 6E 62` | 与 `\r\n` 同理，依赖反斜杠分支 |

关键点：仓库 `.gitattributes` 为 `* text=auto eol=lf`，所以**提交进 git 的 resx 是 LF**；
在 Linux/CI 上直接构建时，值里的物理换行就是裸 `0A`，客户端不认。
而本机工作区因 `core.autocrlf=true` 检出为 CRLF，本地构建又"看起来正常"——这就是
"本地好好的、某处换行丢失"的根源。

本次核对到的现状（`git show HEAD:...` 为 LF，工作区为 CRLF）：

- `ScriptTalk.PartyQuest_Requirement`、`HenesysPQ_Description`、`HenesysPQ_Intro`、
  `KerningPQ_Description`、`KerningPQ_Intro` 共 5 个键在 `ScriptTalk*.resx` 三份文件里
  都改成了物理换行（提交 `cbdedacc7` 由字面 `\r\n` 改成物理换行）。
- `ClientMessage` 里 `CPQ_PickRoom`、`CPQ_FindError`、`GachaCommand_Message1`
  （以及 en-US 的 `SearchCommand_Wrong`）仍是字面 `\r\n`，编译产物中确认为 `5C 72 5C 6E`。

## 4. 结论

1. 少换行的直接原因不是 resx 本身，而是**值里出现了物理换行 + git 归一化成 LF**，
   而客户端只把 `0x0D` 当换行。
2. 想要"文件怎么换行都不影响运行时"，resx 值内**不要写物理换行**：
   写成 `&#13;&#10;`（推荐，编译出真实 CRLF），或沿用字面 `\r\n`（靠客户端反斜杠分支，语义依赖客户端实现）。
3. 反过来，如果改成 JSON/YAML 之类的格式，`"\r\n"` 会在解析时变成真实 `0D 0A`，
   天然免疫 git EOL 归一化；这也是换格式最实际的收益之一。

## 5. 本次改动

- 只新增本文档；未修改任何 resx 或代码（方案待定，见下）。

后续：已按结论 3 落地，全部资源改为 JSON（值内统一写 `"\r\n"` 转义），
见 `docs/I18n.md`。
