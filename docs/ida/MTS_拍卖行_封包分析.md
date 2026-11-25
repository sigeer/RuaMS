# MTS（拍卖行 / ITC）客户端封包分析

> IDB：`D:\walker\code\`Angel\`Angel.idb`（IDA Pro 8.3，模块 `` `Angel.exe ``，ImageBase `0x400000`，v83，自动分析完成，Hex-Rays 可用）
> 客户端类族：`CITC`（拍卖行容器）、`CITCWnd_Char` / `CITCWnd_Tab` / `CITCWnd_List`（窗口与列表）、
> `CRegisterSaleEntryDlg`（上架对话框）、`CRegisterAuctionEntryDlg`（拍卖上架对话框）、`CRegisterWishEntrySearchDlg`（求购登记）
> 服务端：`Application.Core/Channel/Net/Handlers/EnterMTSHandler.cs`、`MTSHandler.cs`、
> `Application.Core/Channel/Net/Packets/MTSPacketCreator.cs`、`Application.Core/Channel/ServerData/MTSManager.cs`、
> `Application.Core.Login/ServerData/MTSManager.cs`、`Application.Core.Login/Servers/MTSGrpcService.cs`
> 协议：`Application.Protos/Model/MTS.proto`、`Application.Protos/Service/MTSOperation.proto`
> 关联文档：`docs/ida/RecvOpcode未定义数据包_客户端发包定位.md`（`0x9C` / `0xFD` 的取号来源）、
> `docs/ida/现金商城_封包分析.md`（同一个 `CStage::OnPacket` 入口的 `0x7F` / `0x143..0x14D`，以及商城专用的 `QUERY_CASH_RESULT`）

---

## 1. 结论速览

| 方向 | 名称 | opcode | 服务端枚举 | 说明 |
|------|------|--------|-----------|------|
| C→S | `ENTER_MTS` | `0x9C` | `RecvOpcode.ENTER_MTS` | 裸包，payload 为空 |
| C→S | `MTS_OPERATION` | `0xFD` | `RecvOpcode.MTS_OPERATION` | 子操作码见 §4 |
| S→C | `SET_ITC` | `0x7E` | `SendOpcode.SET_ITC` | 打开拍卖行界面，见 §3 |
| S→C | `MTS_OPERATION2` | `0x15B` | `SendOpcode.MTS_OPERATION2` | 两个 int：货币显示，见 §5 |
| S→C | `MTS_OPERATION` | `0x15C` | `SendOpcode.MTS_OPERATION` | 子操作码见 §6 |
| C→S | `CHANGE_MAP` | `0x26`(38) | `RecvOpcode.CHANGE_MAP` | **空 payload = 离开拍卖行**，见 §3.3 |

**拍卖行的定价模型（本次由客户端确认，见 §2.4）：**

> 卖家上架时**自己选**模式 —— op 2 = 一口价直售（只填一个价），op 18 = 拍卖
> （要填 **起拍价 + 一口价 + 时长 + 加价幅度** 四个值）。
> **一口价是卖家填的，不是系统按比例算出来的。** 买家实付 = `成交价 + 500 + 成交价×7%`
> （§2.3），卖家的到手价仍是成交价本身。

客户端分发链（`CStage` 子对象 vtable `off_AF58C4[0]`）：

```
sub_5A4205(opcode, CInPacket*)
    switch (opcode):
      case 346 (0x15A): sub_5A4241   → open_web_site(计费网页)，与拍卖行无关
      case 347 (0x15B): sub_5A428C   → 货币显示
      case 348 (0x15C): sub_5A4311   → 再 Decode1 取子操作码，见 §6 的分发表
```

`0x15C` 的子操作码分发表在 **`sub_5A4311`**（完整 21/22/23/24/29/30/31/32/33/34/35/36/37/38/39/40/41/42/43/44/45/46/47/48/49/50/51/52/53/54/55/56/60/61/62），
每个分支读完后都会 `Decode1` 一个字节，该字节为 1 时把 `this[6]`（“请求在途”标志）清零 —— 即**每个响应末尾都有一个 byte 收尾**（下文称“收尾字节”）。

---

## 2. 客户端结构

| 名称 | 作用 |
|------|------|
| `CITC` | 拍卖行主对象，持有 `CITCWnd_Char` / `CITCWnd_Tab` / `CITCWnd_List`；`CITC` 与 CStage 是同一块内存的不同基址视图（分发入口里出现 `a1 - 8`） |
| `CITCWnd_List` | 商品列表。`+104` = 当前 tab，`+108` = 当前 type，`+112` = 当前 page（由 `sub_5A48AF` 从服务端包写入） |
| `CITCWnd_Tab` | tab 控件与搜索框，`OnButtonClicked` 里发 op 5 / op 6 |
| `CRegisterSaleEntryDlg` | 上架对话框，`Confirm` 输出 quantity / 时长等，然后由 `CITC::OnRegisterSaleEntry` 发包 |
| `CRegisterAuctionEntryDlg` | 拍卖上架对话框，同上（走 op 18） |
| `CRegisterWishEntrySearchDlg` | 求购登记，发 op 4 |

IDA 里**没有** `ITCITEM` 这个结构体（`ida_typeinf` 编号类型表里查不到），因此商品条目布局是按
`sub_5A2C0F` 的 `ZAllocEx::Alloc(272)` + 逐字段赋值反推出来的（见 §7）。

### 2.1 tab 语义（`sub_5BC1D5` 的 `v4[26]`）

拍卖行窗口把四个品类挤在同一个列表控件里，用 `当前tab` + `当前type` 两个状态共同定位：

| tab | type | 品类 | 列表里点条目时的按钮行为 |
|-----|------|------|--------------------------|
| 1 | 0 | **出售（商品）** | 右键 `a2==1` → op 9 加入购物车；左键 `a2==0` → YesNo → op 16 直接购买 |
| 2 | 0 | **求购（别人贴的求购单）** | `a2==0` → 与求购单成交（op 12） |
| 3 | 0 | **拍卖** | `a2==1` → YesNo → op 20 按即买价买下；`a2==0` → 出价对话框 → op 19 |
| 4 | 0 | **购物车** | `a2==1` → op 10 移出购物车；`a2==0` → YesNo → op 17 从购物车购买 |
| 4 | ≠0 | **我的求购** | `a2==1` → op 13 撤销求购；`a2==0` → op 11 |

服务端常量与之对应：`SALE_TAB=1` / `WANTED_TAB=2` / `AUCTION_TAB=3` / `MY_WANTED_TAB=4`，
`MY_WANTED_TYPE=1`。`ChangePage` 里 `tab==4 && type==0` 走购物车，`tab==4 && type!=0` 走“我的求购”。

### 2.2 `SET_ITC` 里 5 个 int 的取值（`sub_59EF9D`）

客户端把这 5 个 int 存进 `this[1947..1951]`。**这些是服务端下发的参数，客户端没有写死它们的值**
（`PacketCreator.openCashShop(c, mts: true)` 现在直接写 `MTSConstants.SALE_FEE` / `Fee_Rate` /
`Fee_Base` / `AUCTION_MIN_HOURS` / `AUCTION_MAX_HOURS`）：

| # | 值 | 对应常量 | 用途 |
|---|----|---------|------|
| 1947 | `5000` | `SALE_FEE` | 上架手续费（金币） |
| 1948 | `7` | `Fee_Rate` | 买家实付的加价**百分比** |
| 1949 | `500` | `Fee_Base` | 买家实付的**固定**加价 |
| 1950 | `24` | `AUCTION_MIN_HOURS` | 拍卖时长下限（小时） |
| 1951 | `168` | `AUCTION_MAX_HOURS` | 拍卖时长上限（小时） |

客户端拿 1948/1949 算**买家实付总额**（不是附加费，直接和余额比较）：

```
buyerTotalCost = 1949 + (1948 + 100) * dealPrice / 100
               = 500 + 107 * dealPrice / 100          // 当前取值
```

> **这不是“一口价”**。一口价是卖家上架时在 `CRegisterAuctionEntryDlg` 里单独填的另一个值
> （见 §4.1 / §7 第 15 段 `+0x108`），服务端既不推算也不覆盖它。
> 1948/1949 只决定买家在这笔交易里**多付多少**，卖家仍按成交价入账。
> 早期曾把这个式子当成“服务端自动算出来的一口价”，导致卖家填的一口价被丢弃，已修正。

`OnRegisterSaleEntry` 用 `this[1950] <= 时长 <= this[1951]` 校验拍卖上架时长，服务端
`RegisterAuction` 也按 `Clamp(24, 168)` 再夹一次。

服务端扣款用的是同一个式子（`MTSConstants.BuyerTotalCost` = `价格 + (500 + 价格×7/100)`），
与上面的公式在整数除法下**逐位相等**（`107×p/100 ≡ p + 7×p/100`）。

> ⚠️ **注意有一处客户端自己没走这个参数**：列表上 `SP_4759_BUY_NOW__D` 那一行把
> `一口价 + 一口价/10 + 100` 写死在代码里（§2.3.1），所以在 7 / 500 这套取值下，
> 列表显示的买断价与确认框的数字不同。服务端以**确认框**为准。

### 2.3 全拍卖行只有一套费率：`500 + 价格 × 7%`

买家实付 = **成交价 + 税费**，税费 = `500 + 成交价 × 7%`（`Application.Shared/Items/MTSConstants.cs`
里的 `GetTaxes` / `BuyerTotalCost`，中心服与频道服共用）。卖家永远按成交价入账。

| 场景 | 买家实付 | 卖家实得 | 客户端依据 |
|------|----------|----------|-----------|
| 普通出售（op 16 / 17） | `价格 + 500 + 价格×7%` | `价格` | tooltip 总价 = 第 3 段（税费）+ 第 4 段（售价） |
| 拍卖 · 一口价买下（op 20） | `一口价 + 500 + 一口价×7%` | `一口价` | 确认框 `0x5BC2F6`（列表标签 `0x8F78B0` 不一致，见 §2.3.1） |
| 拍卖 · 出价到期成交 | `最高出价 + 500 + 最高出价×7%` | `最高出价` | `sub_5C3947` 的余额校验 + `SP_4749 ... FOR %d` |
| 求购成交（op 3 / 11 / 12） | `求购价 × 数量`（**未加税费**） | 同左 | ⚠️ 无直接证据，见下 |

> **求购成交那一行是唯一没有客户端证据的**：op 3 / 11 / 12 路上客户端不带金额，
> 也没有 `sub_5C3947` 那样的确认框，所以“要不要在求购价之外再收一次税费”无法从客户端判定。
> 服务端目前**不收**（`FulfillWanted` 里买家付 `价格 × 数量`、卖家得同额）。
> 若以后决定与普通出售对齐（买家付 `总额 + GetTaxes(总额)`），只改 `FulfillWanted` 一处即可 ——
> 但要注意求购人登记时只承诺了 `价格 × 数量`，多收会把“登记价”变成非最终价。

### 2.3.1 列表标签 vs 确认框：同一个概念，客户端给了两个数（以确认框为准）

买家实付这个概念在客户端出现三次，但**只有两处走 SET_ITC 的参数**，第三处是写死的：

| 位置 | 写法 | 数据来源 |
|------|------|----------|
| 列表的 `SP_4759_BUY_NOW__D` 行（`0x8F78B0`~`0x8F78C6`） | `一口价 + 一口价/10 + 100` | `10` 与 `100` **写死在客户端**（`push 0Ah` / `lea eax,[eax+edx+64h]`） |
| `sub_5C3947`（`0x5C3972`，整个函数只有一行 `return`） | `*(this[44]+7796) + (*(this[44]+7792)+100) * (this[39]+this[40]) / 100` | `1949` / `1948`，**SET_ITC 下发** |
| `sub_5BC1D5` `case 3` `a2==1`（`0x5BC2F6` / `0x5BC302`） | `v12 = entry[264]*(this[1948]+100)/100; v13 = this[1949] + v12` | 同上 |

`7792 = 4 × 1948`、`7796 = 4 × 1949`。

**“写死”这一点是确认过的**：`sub_8F6DC9` 全长 4854 条指令，没有任何一处引用
`+0x1E70`（1948）或 `+0x1E74`（1949）—— 画列表行的作者根本没去读 SET_ITC 的参数，
直接把 `10` / `100` 内联了。

因此在当前下发的 `7` / `500` 下，两个数**必然不同**：

| | 买家实付 |
|---|---|
| 列表「BUY NOW」那行 | `一口价 + 一口价/10 + 100` |
| 点下去的确认框（以及余额校验） | `500 + 1.07 × 一口价` |

两者无法同时满足 —— 把 1948/1949 发成 `10`/`100` 能让列表对，但确认框就会跟着变。
**服务端以确认框为准**：它是购买前最后告知玩家的数字，也是 `SP_4759` 那行不参与的那个判断
（`if (余额 < sub_5C3947) → 钱不够`）。列表那行纯粹是画的。

`case 3` 的确认框分支（带地址）：

```
0x5BC2F6  v12 = *(*(v6+4) + 264) * (*(v11 + 7792) + 100) / 100;  // 一口价 × (1948+100)/100
0x5BC302  v13 = *(v11 + 7796) + v12;                             // + 1949
0x5BC318  ZXString::Format(&s, SP_4749_WOULD_YOU_LIKE_TO_MAKE_A_DIRECT_PURCHASE_R_NOF_THIS_ITEM_FOR_D, v13)
0x5BC360  → sub_59F70A  (op 20)
```

式子的语义（`sub_5C373E` 的确定分支）—— 它是**总额**，不是附加费：

```
v8 = 抵用券余额;
if ( v8 < sub_5C3947(this) ) → Notice(SP_535_YOU_DONT_HAVE_ENOUGH_CASH);
else if ( this[39] + this[40] < entry[+0x108] ) → 发 op 19;
else → YesNo(SP_4749 ... FOR sub_5C3947(this)) → op 20;
```

普通出售那条路不需要公式：`case 1` 的确认框 `SP_4803_ARE_YOU_SURE_YOU_WANT_TO_BUY_THIS`
**不带金额**，玩家看到的是物品 tooltip 的“税费 + 售价”，税费就是服务端下发的第 3 段
（`GetTaxes = 500 + 价格×7%`），与 `Buy` 里收的 `价格 + GetTaxes(价格)` 一致。
也就是说：**直售的税费由服务端在条目里直接告诉客户端，拍卖没有这个字段，才退化成
“客户端用 SET_ITC 参数算一遍”** —— 这就是 1948/1949 存在的唯一理由，不是第二套税率。

### 2.4 拍卖上架 = 卖家自己填 4 个值（不是“系统算一口价”）

`CRegisterAuctionEntryDlg::Confirm`（`0x5AD76B`）的输出：

| 参数 | 含义 | 校验 |
|------|------|------|
| `*a4` | **起拍价** | `110 ≤ 起拍价 ≤ 9999999`，越界弹 `SP_5560` |
| `*a5` | **一口价** | 同上，且 `起拍价 + 加价幅度 ≤ 一口价`，否则弹 `SP_4765 THE_BUY_NOW_PRICE_IS_LOWER_THAN_THE_STARTING_BID` |
| `*a9` | **加价幅度** | 来自下拉框（`sub_4C7379` → `*(sub_4C78DF(a1) + 4)`） |
| `*a7` | **时长**（byte） | `24 ≤ 时长 ≤ 168` |
| `*a8` | 常量 `1` | 随 op 18 一起发，服务端忽略 |

所以：**卖家在拍卖行上架时可以选“一口价直售”（op 2）或者“拍卖”（op 18）；
走拍卖时一口价由卖家自己填，系统不推算。** 这与“全部走拍卖、系统自动算一口价”是两种设计，
本客户端明确是前者。

---

## 3. 进入 / 退出

### 3.1 进入：C→S `ENTER_MTS = 0x9C`

裸包、无 payload。服务端 `EnterMTSHandler` 收到后：

1. 复用商城的进入校验（`cannotEnterCashShop` / 事件地图 / 迷你副本）；
2. `EnterExtralWorld`（与商城同一套“额外世界”流程，把角色移出当前地图但保留频道）；
3. `SendPacket(MTSPacketCreator.OpenMTS(...))` → `PacketCreator.openCashShop(c, true)`，即 `SET_ITC`；
4. `getCashShop().open(true)` 打上“已在商城界面”标记，这样客户端发 `CHANGE_MAP`(空包) 时
   走 `ChangeMapHandler` 的 `p.available() == 0` → `LeaveCashShop()` 分支；
5. 主动拉一次第 1 页（tab=1）。

### 3.2 `SET_ITC = 0x7E` 布局（`sub_59EF9D`，`CITC` 的“由包构造”）

```
addCharacterInfo(角色信息)
writeString(AccountName)      // DecodeStr → this[15]，再 Mid(0,2) 当作区号
writeInt x5                                  // this[1947..1951]
writeLong / DecodeBuffer(8)                  // 8 字节 FILETIME（拍卖行开放时间）
```

其中 `this[1947..1951]` 就是 op 2 / op 18 里用到的“上架时长可选区间”等参数
（`OnRegisterSaleEntry` 里用 `this[1950] <= dlg 时长 <= this[1951]` 做拍卖时长校验）。

服务端现有实现 `PacketCreator.openCashShop(c, mts: true)` 写的 28 字节常量块
（`0x88,19,0,0 | 7,0,0,0 | 0xF4,1,0,0 | 0x18,0,0,0 | 0xA8,0,0,0 | 0x70,0xAA,0xA7,0xC5,0x4E,0xC1,0xCA,0x01`）
= 5 个 int + 8 字节 FILETIME，**与客户端解码顺序一致，未改动**。

### 3.3 退出：`CITC::SendTransferFieldPacket`（`0x59EF2B`）

客户端“离开”按钮最终调用 `CITC::SendTransferFieldPacket`，它构造 `COutPacket(38)`，
**不带任何字段**。38 = `0x26` = `CHANGE_MAP`。服务端 `ChangeMapHandler`：

```csharp
bool enteringMapFromCashShop = p.available() == 0;
if (enteringMapFromCashShop) { await c.LeaveCashShop(); return; }
```

因此**不需要**为拍卖行单独加退出包。

注意：op 34（`sub_5A4C2A`）与 op 36（`sub_5A4CE7`）在错误码为 **73** 时也会调用
`SendTransferFieldPacket` —— 即“列表加载失败”会把玩家踢回原地图。

### 3.4 主窗口的三个按钮：`0xFB` / `0xFC` / `CHANGE_MAP`

`sub_5B8453` 在 `CITC` 主窗口上创建三个 `CCtrlButton`，按钮 id 即下发时的参数：

| 按钮 id | 文案 | 按钮创建地址 | 点击后发出的包 |
|---|---|---|---|
| 1000 | `SP_4736_UI_ITCIMG_BTCHARGE`（充值） | `0x5B84E7` | `0xFB`（`sub_59EBDA`） |
| 1001 | `SP_4737_UI_ITCIMG_BTCHECK`（查询） | `0x5B85A1` | `0xFC`（`sub_59EECE`） |
| 1002 | `SP_1230_UI_CASHSHOPIMG_CSSTATUS_BTEXIT`（离开） | `0x5B864A` | `COutPacket(38)` = `CHANGE_MAP` |

`sub_5B8688`（安装在该窗口 vtable 槽 `0xAF5C24`）按按钮 id 分发：

```c
if (a2 == 1000) return sub_59EBDA(this[33]);   // 0xFB，裸包
if (a2 == 1001) return sub_59EECE();           // 0xFC，裸包（sub_59EC2C 只是 thunk）
if (a2 == 1002) return CITC::SendTransferFieldPacket();
```

`0xFB` / `0xFC` 都是**裸包、无 payload**，且在发包前用 `this[6]`（控件对象 `+0x18`）做
「只发一次」保护：`sub_59EECE` 读到非 0 就直接 return 0，不再发包。也就是说**同一次打开拍卖行
期间，每个按钮最多只会发一次请求，收到能清标志的回包之前再点也发不出去**。

- `0xFB`（充值）服务端未实现处理，也不在 `RecvOpcode` 里；
- `0xFC`（查询）对应 `RecvOpcode.MTS_Check`，服务端回 `MTS_OPERATION2`（`ShowMTSCash`）。
  `sub_5A428C` 在余额非负时会把 `this[6] = 0`，正好清掉「请求在途」；
  余额为负则弹 `SP_557` 并 `SendTransferFieldPacket()` 把玩家踢出拍卖行。

---

## 4. C→S `MTS_OPERATION = 0xFD`

客户端所有该类包都由 `COutPacket(253)` 构造。用 `push 0FDh` + `call COutPacket::COutPacket` 特征
扫全二进制，命中 19 处构造点，去掉重复后得到下表（**全部为客户端实测构造点**）。

| op | builder | payload（按发送顺序） | 语义 |
|----|---------|----------------------|------|
| 2 | `CITC::OnRegisterSaleEntry` `0x59EC36` | `GW_ItemSlotBase::Encode` + int slot + int quantity + int price + byte + byte | 普通上架 |
| 18 | 同上（`a2 != 0` 分支） | `GW_ItemSlotBase::Encode` + int slot + int quantity + int 1 + int 时长 + byte + byte + int | 拍卖上架 |
| 3 | `sub_59EE3F` `0x59EE3F` | byte + int + `GW_ItemSlotBase::Encode` + int | 对求购物品出价 |
| 4 | `CITC::OnRegisterWishEntry` `0x59F542` | int itemid + int price + int quantity + byte + byte + str | 登记求购 |
| 5 | `sub_59F297` / `sub_59F376` / `sub_59F465` | int tab + int type + int page + byte + byte + int searchType + str search | 首次加载 / 翻页 / 切换类型 |
| 6 | `CITCWnd_Tab::OnButtonClicked`（`0x5B719D`、`0x5B7266`） | int tab + int type + int 0 + int searchType + str keyword | 提交搜索 |
| 7 | `sub_59FB7A` | int 商品id | 取消上架（先弹 YesNo） |
| 8 | `sub_59FC23` | int 商品id | 从“待领取”取出物品 |
| 9 | `sub_59F89D` | int 商品id | 加入购物车（tab 1 的 `a2==1` 按钮） |
| 10 | `sub_59F9AE` | int 商品id | 从购物车移除（tab 4 的 `a2==1` 按钮） |
| 11 | `sub_59FA21` | int 商品id | 求购列表：点击条目 |
| 12 | `sub_59FA94` | int 商品id | 求购相关 |
| 13 | `sub_59FB07` | int 商品id | 求购列表：移除 |
| 16 | `sub_59F697` | int 商品id | **在商品列表内直接购买** |
| 17 | `sub_59F910` | int 商品id | **从购物车购买** |
| 19 | `sub_5C373E`（`case 1`, `a2==0` 分支上游） | int 商品id + int 出价 + int ... | 拍卖出价 |
| 20 | `sub_59F70A` | int 商品id | 拍卖直接购买（buy-now） |

### 4.1 op 2 / op 18 的精确字段序（`CITC::OnRegisterSaleEntry` `0x59EC36`）

两者是同一个函数，`a2 == 0` 走普通上架、`a2 == 1` 走拍卖上架：

```
a2 == 0（普通上架）                                   a2 == 1（拍卖上架）
  Encode1(2)                                            Encode1(18)
  GW_ItemSlotBase::Encode                               GW_ItemSlotBase::Encode
  Encode4(a4)            // slot                        Encode4(a4)   // slot
  Encode4(v22)           // quantity                    Encode4(v22)  // quantity
  Encode4(a3)            // price                       Encode4(a2)   // 起拍价
  Encode1(v23)           // byte                        Encode4(v20)  // 即买价
  Encode1(v21)           // byte                        Encode1(v23)  // 时长(小时, byte)
                                                        Encode1(v21)  // byte
                                                        Encode4(v19)  // int
尾长 14                                              尾长 22
```

注意两个差异，**都曾经写错过**：

- 拍卖的**时长只有 1 个 byte**，不是 int。客户端用 `this[1950] <= v23 && this[1951] >= v23`
  校验它落在 `24..168`（`this[1950]` / `this[1951]` 就是 §2.2 的 24 / 168）；
- 拍卖有**两个价格 int**：`a2` 是起拍价、`v20` 是一口价（卖家填的）。
  `CRegisterAuctionEntryDlg::Confirm(a3, a5, this, &a2, &v20, &v22, &v23, &v21, &v19)`
  一共 6 个输出，而普通上架的 `CRegisterSaleEntryDlg::Confirm` 只有 4 个
  （`&a3`=价格、`&v22`=数量、`&v23`、`&v21`），这就是差异的来源。

op 18 的包尾固定 22 字节，**逐字段**是：

```
int  slot        // 背包格子
int  quantity
int  起拍价       // Confirm 的 *a4
int  一口价       // Confirm 的 *a5 —— 卖家填的即买价，服务端必须存下来
byte 时长(小时)   // Confirm 的 *a7
byte 常量 1       // Confirm 的 *a8
int  加价幅度     // Confirm 的 *a9（下拉框）
```

> **服务端早期错误地忽略了第 4 个 int**（当成“没用”跳过），并把一口价改成用
> `成交价 × 1.07 + 500` 现算。正确做法是：一口价原样保存（条目第 15 段 `+0x108`），
> 税费只是**买家实付**的一部分（§2.3），两者不要混。

服务端 `MTSHandler.HandleSaleEntry(p, c, tail, auction)` 用同一个函数按 `tail` 区分
（op 2 = 14，op 18 = 22），都从包尾反推 `slot/quantity/price`，只从包首取物品id。

### 4.2 op 19（拍卖出价）是 **3 个 int**

`sub_5C373E` 的 `a2 == 1` 分支：

```
Encode1(19)
Encode4(*(this[47] + 32))   // 商品id —— 条目 +0x20，与 op 7/8/9/10 同一个字段
Encode4(this[40])           // 基准价 = 条目 +0x10（对话框打开时的当前价）
Encode4(this[39])           // 加价部分 = 条目 +0x0E 的整数倍
```

**真正的出价 = `this[39] + this[40]`**。这一点由三个地方交叉确认：

1. 出价对话框构造（`sub_5C28F9`）：`this[39] = this[42] = entry[+0x0E]`（一个加价幅度），
   `this[40] = this[41] = entry[+0x10]`（当前价）；
2. 上下箭头（`sub_5C373E` 的 `1003` / `1004` 分支）：只加减 `*(entry+0x0E)`，
   即 `this[39]` 是“加价部分”，`this[40]` 不动；
3. 余额校验与买断判断都用和：`sub_5C3947(this)` 里的 `(this[39] + this[40])`，
   以及 `if (this[39] + this[40] < *(this[47] + 264))` 发 op 19、否则弹“是否直接买下”。

发出去之前客户端已经校验过余额：
`balance >= sub_5C3947(this) = this[1949] + (this[1948]+100)*(this[39]+this[40])/100`，
即 **`balance >= 出价 + 500 + 出价×7%`**（1948/1949 = 7/500，见 §2.2），
不够就弹 `SP_535_YOU_DONT_HAVE_ENOUGH_CASH`。

> 服务端 `MTSHandler` 读 **3 个 int** 并把前两个相加作为出价。
> 只读第 2 个 int 会漏掉加价部分，出价永远等于“对话框打开时的当前价”。
> 服务端最低出价按 `当前价 + 加价幅度` 校验（与对话框初始值一致）。

### 4.3 op 3（对求购单出价）

`sub_59EE3F(this, char a2, int a3, void* a4, int a5)`：

```
Encode1(3)
Encode1(a2)          // 1 个 byte
Encode4(a3)          // 商品id
GW_ItemSlotBase::Encode
Encode4(a5)          // 1 个 int
```

服务端只取 `a3`（商品id），交货时按求购单的物品id 从背包里取货，不解析那两段
（`a2` / `a5` 的语义仍未展开，见 §10）。

### 4.4 tab 2 的 `a2==0` 不是发包，是开对话框

`sub_59F811`（`sub_5BC1D5` 的 `case 2`）并不发 `0xFD`，而是
`ZAllocEx::Alloc(1480)` 造一个对话框再 `DoModal`（构造在 `sub_5C1384`，
用 `itemid / 1000000` 判断装备/消耗/设置/其他/现金，落进 `this[363]`）。
所以“在求购列表里点条目”最终是通过这个对话框再走到 op 3 的。

> **op 14 / op 15 不存在**：对全二进制扫 `push 0FDh` + `call COutPacket::COutPacket` 共命中 19 个
> 构造点，逐个反编译后覆盖的子操作码是 **2,3,4,5,6,7,8,9,10,11,12,13,16,17,18,19,20**
> —— 没有 14，也没有 15。旧 `modules` 实现里的 `case 14` 是死代码。

`tab 1 / tab 4` 的按钮分发在 `sub_5BC1D5`（`v4[26]` 为当前 tab，`a2` 为 0/1 两个按钮）：

```
tab 1: a2==1 → op 9（加入购物车）      a2==0 → YesNo("确定购买?") → op 16
tab 2: a2==0 → sub_59F811（求购相关）
tab 3: a2==1 → YesNo → op 20（直接购买） a2==0 → sub_59F77D（出价对话框）
tab 4: 非求购模式 a2==1 → op 10 / a2==0 → YesNo → op 17
       求购模式   a2==1 → op 13 / a2==0 → op 11
```

**注意 op 16 与 op 17 的区别**：16 = 列表直接购买，17 = 购物车购买。服务端
`MTSHandler` 的 `Buy(c, id, fromCart:false / true)` 与之对应。

### 4.1 op 2 的定价校验（客户端侧）

`CRegisterSaleEntryDlg::Confirm` 之后，`OnRegisterSaleEntry` 校验价格：

```
price > 109 && price <= 9999999
```

否则弹 `SP_5560_PLEASE_ENTER_THE_PRICE_OF_OVER_110_NX`。服务端 `MTSHandler.HandleSaleItem`
已按 `price < 110 || price > 9999999` 拒绝。

---

## 5. S→C `MTS_OPERATION2 = 0x15B`（货币显示）

`sub_5A428C`（0x15B 分支）：

```
writeInt NxPrepaid   → this[9]
writeInt MaplePoint  → this[12]
InvalidateRect(货币区域)
```

两个 int 任一为负 → 弹 `SP_557_DUE_TO_AN_UNKNOWN_ERROR...CASH_SHOP_HAS_FAILED` 并把玩家送出拍卖行。
服务端 `MTSPacketCreator.ShowMTSCash(nxPrepaid, maplePoint)` 与此一致。

---

## 6. S→C `MTS_OPERATION = 0x15C`

| op | 客户端处理函数 | payload | 语义 |
|----|---------------|---------|------|
| 21 | `sub_5A48AF` | int 总容量 + int 条数 + int tab + int type + int page + byte + byte + N×条目 + 收尾字节 | 商品列表 |
| 22 | `sub_5A4882` | — | 单条更新（未使用） |
| 23 | `sub_5A4A02` | 同 21 的条目段 | 列表整体刷新（未使用，改用 op 21 更稳） |
| 24 | `sub_5A49E3` | — | 未使用 |
| 29 | `sub_5A4674` | 无 | **上架成功**（“Successfully registered the item for sale”） |
| 30 | `sub_5A4581` | byte errorCode + int 上限 | 上架限制/错误分发（未使用） |
| 31 | `sub_5A46B2` | 无 | **对求购单出价成功** |
| 32 | `sub_5A46F0` | 无 | **对求购单出价失败** |
| 33 | `sub_5A4AF3` | int 条数 + N×条目 + int 未显示数量 + 收尾字节 | **待领取列表** |
| 34 | `sub_5A4C2A` | byte errorCode | 错误提示；code==73 时踢出拍卖行 |
| 35 | `sub_5A4C57` | int 条数 + N×条目 | **我上架中的商品** |
| 36 | `sub_5A4CE7` | byte errorCode | 错误提示；code==73 时踢出拍卖行 |
| 37 | `sub_5A4D14` | 无 | **取消出售成功** |
| 38 | `sub_5A4D49` | byte errorCode | **只弹提示、不踢人**（服务端通用错误出口） |
| 39 | `sub_5A4D68` | int tab + int slot | **物品已放入背包**：`CCtrlTab::SetTab(tab-1)` + `sub_5B3B00` 选中 slot / 滚动（不放物品，§6.1.1） |
| 40 | `sub_5A4DCF` | — | 未使用 |
| 41 | `sub_5A4DFC` | 无 | **加入购物车成功** |
| 42 | `sub_5A4E31` | 无 | 加入购物车失败 |
| 43 | `sub_5A4E66` | 无 | **移出购物车成功** |
| 44 | `sub_5A4E91` | 无 | 移出购物车失败 |
| 45 | `sub_5A4EC6` | int 条数 + N×条目 | 我的上架列表（与 op 35 重复，未使用） |
| 46 | `sub_5A4FDC` | — | 未使用 |
| 47 | `sub_5A5011` | 无 | **与求购单成交成功**（“Successfully requested to buy the item”） |
| 48 | `sub_5A503C` | 无 | **与求购单成交失败**（“Request to buy the item has failed”） |
| 49 | `sub_5A5071` | 无 | **撤销求购成功**（“The request for purchase has been canceled”） |
| 50 | `sub_5A50DF` | 无 | 撤销求购失败（“Failed to cancel the request for purchase”） |
| 51 | `sub_5A5114` | 无 | **购买成功**（“You have successfully purchased the item”） |
| 52 | `sub_5A513F` | 无 | **购买失败**（“Failed to purchase the item”） |
| 53 | `sub_5A5174` | — | 未使用（购买成功变体） |
| 54 | `sub_5A519F` | — | 未使用（购买失败变体） |
| 55 | `sub_5A51D4` | 无 | **求购登记成功**（“Successfully registered under [buy]”） |
| 56 | `sub_5A5209` | 无 | 求购登记失败（“Failed to register the item under [buy]”） |
| 60 | `sub_5A5444` | 无 | **不能连续出价**（“You cannot make a consecutive bid”） |
| 61 | `sub_5A523E` | int A + int B | 求购到期提示；`A>=0 && B>0` 时才弹，文案含 B（NX/物品数） |
| 62 | `sub_5A52DE` | byte 方向 + int itemid + int NX + 8 字节 FILETIME | 卖出/买入通知（“%s is sold/bought for %d NX”） |

> **31/32 与 47/48 的分工**：两条路径都是“把东西卖给求购单”，但入口不同 —— op 3
> （客户端带物品编码的那条）走 31/32，op 11 / op 12（只带商品id）走 47/48。
>
> **53/54 与 51/52、45 与 35 语义重复**，服务端统一用已验证的那一个（51/52、35），
> 53/54/45 保留在文档里备查但不下发。

### 6.1 `op 39` 的 tab 是 **1 基**背包页

`CCtrlTab::SetTab(*(dword_BED8A4 + 112), tab - 1)` —— 传 `1..5` 分别对应
装备 / 消耗 / 设置 / 其他 / 现金，与 `(int)InventoryType` 完全一致。
服务端 `MTSConfirmTransfer((int)item.getInventoryType(), pos)` 正确。

### 6.1.1 `op 39` **不往背包里放物品**（`sub_5B3B00` 的语义）

`sub_5A4D68` 第三句是 `sub_5B3B00(v4)`，直接把格子号传进去。这个函数（`0x5B3B00`，`size=0xE8`）
通篇没有任何写背包的代码，它做的是**界面层的「选中 + 滚动」**：

```c
// this = CITC 的背包面板（CCITC 内的子控件），a2 = 格子号（服务端下发的 slot）
*(this + 1444) = -1;                       // 先清掉「当前选中格」
if (a2 >= 0)
{
    v4 = *(this + 1440);                   // 当前背包页（tab）
    v5 = CharacterData 背包页 v4 的条目数组长度;
    if (a2 < v5 - 1)                       // 比 CharacterData::GetItem 的 `a4 <= len-1` 严格一格
    {
        v6 = CharacterData::GetItem(chrData, v4, a2)->ptr;
        if (v6)                            // ← 只有那一格真有物品才记选中
            *(this + 1444) = a2;
    }
}
if (*(this + 1444) != -1)
{
    v8 = 滚动条当前页;
    v9 = (slot - 1) / 6;                   // 每行 6 格，算出目标行页
    ...                                    // 目标页超出可视区就夹到 near 边界
    if (v8 != dword_BED908[tab])           // 滚动位置变了才动
    {
        dword_BED908[tab] = v8;
        CCtrlScrollBar::SetCurPos(scrollBar, v8);
        *(this + 1436) = 6 * v8 + 1;       // 记下可视区首格
    }
}
CWnd::InvalidateRect(this, 0);             // 重画
```

结论：

- 它读的是 **客户端本地缓存的 `CharacterData`**（`CWvsContext::GetCharacterData`），
  用 `CharacterData::GetItem(tab, slot)` 判断这一格有没有东西；有才把格子号存进
  `this+1444`（选中）并调整滚动条 —— 没有就直接保持 `-1`，界面上什么都不会选中。
- 所以 **`op 39` 只是「去背包页 N 选中格子 M 并滚到它」的指令**，物品必须已经在客户端背包里。
  服务端如果只发 `op 39` 而不发 `INVENTORY_OPERATION` 的 add，客户端会去查一个空槽，
  选中状态不生效、提示照弹但背包里没有东西。
- 物品数据来自哪里由服务端决定：旧实现（模块版）在频道用 `InventoryManipulator.addFromDrop`
  塞进 `Player.Bag` 再靠 `InventoryOperation` 同步，现在改为中心服持有背包、
  频道只按 `TakeItemResponse.Item` 重建 `Item` 后发一次 `InventoryAdd`（见 §12.2.2）。

> 同一族还有 `sub_5B34A9`（`CITCTabWnd` 的鼠标处理）：`a2 == 513`（单击）时也调
> `sub_5B3B00` 完成「点哪格选哪格」，`a2 == 515`（双击）才发 `MTS_OPERATION` op 8 向服务端要物品。
> 两侧走的是同一个函数，进一步说明它只是选中/滚动，与背包数据无关。

### 6.2 `op 33` / `op 35` / `op 39` 的收尾差异

- `op 21`：条目后跟 **1 个字节**；
- `op 33`：条目后跟 **int + 1 个字节**（`sub_5A4AF3` 里 `Decode4` 后 `Decode1`）；
- `op 35`：条目后**什么都不跟**（`sub_5A4C57` 里没有尾部 `Decode1`）；
- `op 39 / 51 / 52 / 29`：没有条目。

服务端 `MTSPacketCreator` 三种写法已分别对上（`TransferInventory` 写 `int 0 + byte 1`，
`NotYetSoldInv` 不写尾部）。

---

## 7. 商品条目（ITCITEM）布局

解析函数 **`sub_5A2C0F`**（`ZAllocEx::Alloc(272)`，即条目结构体 272 字节）。
按解码顺序，共 17 段：

| # | 类型 | 结构体落点 | 说明 |
|---|------|-----------|------|
| 1 | `GW_ItemSlotBase::Decode` | `+0x14`（ZRef，物品指针在 `+0x18`） | 物品本体（`PacketCreator.addItemInfo(p, item, zeroPosition: true)` 的镜像） |
| 2 | int | `+0x20` | **商品 id**（op 7/8/9/10/16/17 里客户端回传的就是这个值） |
| 3 | int | `+0x24` | **税费** |
| 4 | int | `+0xFC` | **售价**。注意客户端在拍卖 tab 里把它当作**当前最高出价的加数**（见 §7.2），所以拍卖条目必须填 0 |
| 5 | str | `+0x100` | 出售者**账号名** |
| 6 | str | `+0x28` | 出售者**角色名** |
| 7 | 8 字节 | `+0x2C` | **上架结束时间**（FILETIME，`DecodeBuffer(…, 8)`） |
| 8 | str | `+0x34` | **出售者名字**（50 字节定长缓冲）。客户端四个 tab 的按钮处理都用它判断“这条是不是我自己挂的”，**不能留空** |
| 9 | str | `+0x66` | 预留串（同上） |
| 10 | str | `+0x98` | 预留串（同上） |
| 11 | int | `+0x0C` | **拍卖：出价人数**。客户端只保留低 16 位（`*(this+12)` 与 `*(this+14)` 交叠，实际是 WORD 存储），`sub_8F6DC9` 把它拼进 `SP_4766_BID_COUNT__D` |
| 12 | int | `+0x0E` | **拍卖：加价幅度**（出价对话框上下箭头按它步进，`sub_5C373E` 的 `p3+0x0E`） |
| 13 | int | `+0x10` | **价格本体**：出售/求购是售价（`SP_4805_SALE_PRICE__D`），拍卖是**当前最高出价**（出价对话框的基准价，`sub_5C28F9` 的 `p3+0x10`） |
| 14 | int | `+0x104` | 预留 |
| 15 | int | `+0x108` | **拍卖：一口价**（`+264`。卖家上架时自己填的价，`sub_5BC1D5` case 3 与 `sub_5C373E` 都从这里取） |
| 16 | int | `+0x10C` | **拍卖：单价组成项**（`SP_4763_PRICE_PER_UNIT__D` = 第 4 段/数量 + 本段） |
| 17 | short | `+0x1C` | 预留 |

条目在内存里是**两级结构**：列表里存 8 字节 `ZRef`（`*(元素+4)` 才是 272 字节对象本体），
`sub_5A2C0F` 分配的 272 字节就是它。`sub_5B219C` / `sub_5C28F9` / `sub_59F70A` 里的
`*(a3+4)`、`*(v10+4)`、`*(a2+4)` 都是这个本体，所以 `+0x20` / `+0x24` / `+0xFC` / `+0x108`
这些偏移要对本体（而不是那 8 字节的 ZRef）来读。

逐条解码（`sub_5A2C0F`，带地址）：

```
0x5A2C70  GW_ItemSlotBase::Decode          → entry+0x14 (ZRef，指针落在 +0x18)
0x5A2C90  *(v6+4) = *(v5+4)                → +0x18 = 物品指针
0x5A2CA6  Decode4 → *(v24+32)   = +0x20    商品 id
0x5A2CB3  Decode4 → *(v24+36)   = +0x24    税费
0x5A2CC0  Decode4 → *(v24+252)  = +0xFC    售价
0x5A2CCC  DecodeStr                        账号名 / 角色名
0x5A2CFF  DecodeStr
0x5A2D34  DecodeBuffer(entry+0x2C, 8)      8 字节时间
0x5A2D3F  DecodeStr → lstrcpy(entry+0x34)  50 字节定长缓冲
0x5A2D6C  DecodeStr → lstrcpy(entry+0x66)
0x5A2D93  DecodeStr → lstrcpy(entry+0x98)
0x5A2DC0  Decode4 → *(v24+12)   = +0x0C
0x5A2DCE  Decode4 → *(v24+14)   = +0x0E    加价幅度
0x5A2DDC  Decode4 → *(v24+16)   = +0x10    当前价
0x5A2DE9  Decode4 → *(v24+260)  = +0x104
0x5A2DF9  Decode4 → *(v24+264)  = +0x108   一口价
0x5A2E09  Decode4 → *(v24+268)  = +0x10C
0x5A2E1C  Decode2 → *(v24+28)   = +0x1C    short
```

> **注意 `+0x0C` / `+0x0E` / `+0x10` 三处都是 `Decode4`，落点只隔 2 字节，会互相覆盖。**
> 最终内存是：`[0x0C..0x0D]` = 第 11 段低 16 位、`[0x0E..0x0F]` = 第 12 段低 16 位、
> `[0x10..0x13]` = 第 13 段完整的 32 位。所以客户端在 `+0x0E` 上是按 **WORD** 读加价幅度、
> 在 `+0x10` 上按 **DWORD** 读当前价。服务端照常写三个 int 即可，只要加价幅度 ≤ 65535。

语义佐证：

- `sub_5B219C`（列表里的物品 tooltip，`0x5B221D`）读 `*(条目+4)+36` 与 `*(条目+4)+252`
  拼成总价 —— 证实 `+0x24` 是税费、`+0xFC` 是售价，同时说明条目是
  「8 字节 ZRef → 272 字节对象」的两级结构，`*(a3+4)` 才是 272 字节对象本体；
- **第 8 段（`+0x34`）是出售者名字**：`sub_5BC1D5` 里 `v7 = *(v6+4)`（条目本体）之后，
  四个 tab 分支都先做 `dword_BF03C4(v7 + 52, *(*dword_BE7918 + 8268))`（`52` 就是 `+0x34`），
  再决定是走正常操作还是弹
  `SP_4743_THE_ITEM_YOU_HAVE_REGISTERED_R_NCANNOT_BE_IN_TRANSACTION` /
  `SP_4812_YOU_CANNOT_PLACE_A_BID_R_NON_YOUR_OWN_ITEM`。
  四个分支的走向（tab1 能买 / tab3 不能出价 / tab2 能开求购框 / tab4 能进购物车）
  只有把它的返回值理解成「两个名字**不相等**」时才全部自洽。
  `dword_BF03C4` 是 IAT 里的导入函数指针（静态时为 0），无法直接确定是哪个 API，
  但语义只能是字符串比较。

  > 这条以前被当成“预留串”写了空串，结果客户端永远认为“所有挂单都是别人的”：
  > 自己的挂单也能加购物车、点买断，只是被服务端拦下（`Buy_CannotBuyOwn`）。
  > 现在第 8 段已经改填 `SellerName`。
- `sub_5A4752`（错误码 → 文案）与 `sub_59FB7A`（取消上架）都从 `+0x20` 取商品 id。

**第 12 / 13 / 15 段为什么是“加价幅度 / 当前价 / 一口价”**（而不是“当前最高出价”）：

`sub_5C373E`（出价对话框）的 `a2 == 1`（确定）分支：

```
if (this[39] + this[40] < *(this[47] + 264))   // 264 = +0x108
    → 发 op 19（普通出价）
else
    → YesNo("WOULD_YOU_LIKE_TO_MAKE_A_DIRECT_PURCHASE ... FOR %d")
      → sub_59F70A() 发 op 20（一口价买下）
```

对话框一打开时 `this[39]+this[40]` 就等于 `条目[+0x10] + 条目[+0x0E]`，
也就是“当前价 + 一个加价幅度”。如果 `+0x108` 是“当前最高出价”，那么在没有任何人出价时
这个和就已经 ≥ `+0x108`，**每次出价都会变成立即买断**，客户端不可能这么写。
反过来，`+0x108` 是卖家填的一口价时逻辑完全自洽：
出价没够到一口价就正常竞价，够到就提示改为一口气买下。
再加上 §2.4 的 `CRegisterAuctionEntryDlg` 本来就要求卖家填两个价、并校验
`起拍价 + 加价幅度 ≤ 一口价`，因此 `+0x108` = 一口价。**

`+0x10` 是“当前价”而非“起拍价”：它是可变的，服务端每次接受出价后要把它推上去，
否则出价对话框的基准价永远不会动。**出售 / 求购条目**的第 4 段（`+0xFC`）填售价即可
（物品 tooltip 的总价 = 税费 + 本段）；**拍卖条目必须填 0**，原因见 §7.2。

**这一部分与本仓库曾参考的 Cosmic 实现差别很大**：Cosmic 的 `MTSPacketCreator` 写的是
`id, taxes, price, 0, long, str, str, 28×0`，而本客户端实际是
`item, id, taxes, price, str, str, 8字节时间, 3×str, 2×int, 4×int, short`。
服务端 `MTSPacketCreator.writeMTSItemEntry` 已按上表重写。

第 8~10 / 11~16 / 17 段在普通出售条目里固定填空串与 0 —— 客户端会把它们写进定长缓冲，
不读也不会崩，但长度必须一致，否则后续条目错位。
**第 8 段例外**：它是出售者名字，留空会让客户端认不出自己的挂单（见上）。

### 7.1 出价对话框只支持「按加价幅度步进」

`sub_5C373E`（出价对话框的 `OnCommand`）只处理四个消息，**没有可编辑的价格输入框**：

| 消息 | 行为 |
|------|------|
| `1003`（加号） | `if (this[39] + this[40] >= 条目[+0x108]) return;` 否则 `this[39] += 条目[+0x0E]` |
| `1004`（减号） | `if (条目[+0x0E] < this[39]) this[39] -= 条目[+0x0E]` |
| `1`（确定） | 余额校验 → `this[39]+this[40] < 条目[+0x108]` 就发 op 19，否则弹“是否直接买下” |
| `2`（取消） | 直接关闭 |

所以**买家不能输入任意金额**：每次点击恰好加/减**一个加价幅度**，出价必然是
`当前价 + k × 加价幅度`（`k ≥ 1`，初始就是 `k = 1`），且不会达到或超过一口价
（到了就改成“直接买下”）。

服务端的对应校验只要保证 `出价 >= 当前价 + 加价幅度`；**不强制要求是加价幅度的整数倍** ——
真实客户端本来就只会发整数倍，放宽不会放松任何真实可达的作弊面，还能避免服务端规则
比客户端更严导致合法出价被拒。

### 7.2 列表行的绘制（`sub_8F6DC9`）—— 第 11 / 13 / 4 / 15 / 16 段的真正用途

`sub_8F6DC9` 是拍卖行列表的**行绘制函数**：参数 `a7` 就是那个 272 字节条目本体，
`a8` 是 tab。拍卖分支（`a8 == 2`）逐项取 `IWzCanvas::DrawTextA` 的格式化参数，
把上表里几个“语义未展开”的偏移一次性钉死了：

| 地址 | 客户端文案 | 表达式 | 对应条目偏移 | 结论 |
|------|-----------|--------|-------------|------|
| `0x8F7775` | `SP_4766_BID_COUNT__D` | `LOWORD(*(v88 + 12))` | `+0x0C`（**WORD**） | 第 11 段 = **出价人数** |
| `0x8F7A4C` | `SP_4756_CURRENT_TOP_BID__D` | `*(v120 + 16) + *(v120 + 252)` | `+0x10` + `+0xFC` | 当前最高出价 = **第 13 段 + 第 4 段** |
| `0x8F78BA` | `SP_4759_BUY_NOW__D` | `*(v101+264)/10 + *(v102+264) + 100` | `+0x108` | 一口价 = 第 15 段，**税费由客户端自己加** |
| `0x8F7629` | `SP_4763_PRICE_PER_UNIT__D` | `*(v73+252) / 数量 + *(v74+268)` | `+0xFC`/数量 + `+0x10C` | 单价 = 第 4 段/数量 + 第 16 段 |
| `0x8F7D31` | `SP_4805_SALE_PRICE__D`（出售 tab） | `*(v153 + 16)` | `+0x10` | 出售 tab 显示的是第 13 段 |
| — | 剩余时间 | `sub_59FD85` → `SYSTEMTIME` → `SystemTimeToFileTime` 与当前时间求差，`ZXString::Format("%d:%d")`，除数为 `0x861C46800`（天）/ `0x23C34600`（时） | `+0x2C` | 第 7 段是**结束时间**（不是上架时间） |

由此得到两条必须遵守的写包规则：

1. **第 11 段（`+0x0C`）填出价人数**，不是 0。它是按 `WORD` 读的，
   所以超过 65535 的部分客户端会截掉（实际拍卖行不可能到这个数量）。
   服务端在每次接受出价时累加（`MTSItemProto.bidCount` → `mts_items.bid_count`）。
2. **第 4 段（`+0xFC`）在拍卖条目里必须填 0。** 客户端算
   “当前最高出价 = 第 13 段 + 第 4 段”，而第 13 段本身就是当前最高出价
   （出价对话框也拿它当基准，见 §7.1）。如果两段都填同一个价，
   拍卖行列表里的“当前最高出价”会显示成**两倍**。
   出售 / 求购条目不受影响，第 4 段照旧填售价（tooltip 的总价 = 税费 + 第 4 段）。

第 16 段（`+0x10C`）只在“单价”这一行用到，与第 4 段相加；第 4 段为 0 时它就是单价本身，
服务端填 `当前价 / 物品数量`。第 15 段（`+0x108`）填卖家的一口价原值 —— 客户端显示
`一口价 + 税费`，所以服务端**不要**预先加上税费。

> 第 13 段 = “当前价”这一点，与“第 4 段填 0”并不冲突：另一种读法
> （第 13 段填 `当前价 - 起拍价`、第 4 段填起拍价）也能让加法成立，但那样出价对话框的
> 基准价（`this[40] = 条目[+0x10]`）就会退化成“起拍价 + 一个加价幅度”，
> 每次出价都要被服务端以“低于最低出价”拒绝。把第 13 段放当前价、第 4 段放 0，
> 加法、对话框、单价三处同时自洽。

---

## 8. 错误码（op 34 / 36 / 38）

`sub_5A4752(code)` 的映射（`code` 为 byte）：

| code | 字符 | 客户端文案 |
|------|------|-----------|
| 66 | `'B'` | You do not have enough NX |
| 67 | `'C'` | You do not have enough mesos |
| 68 | `'D'` | An error has occurred while using NX |
| 71 | `'G'` | Your inventory is full |
| 73 | — | Failed to load the list（**op 34 / 36 会同时把玩家踢出拍卖行**） |
| 78 | `'N'` | Unable to pull out of the sale - you have at least 1 bid on the item |
| 79 | `'O'` | This item may not be sold |
| 81 | `'Q'` | The item has been sold - you cannot cancel the sale of a purchased item |
| 82 | `'R'` | Throwing stars are not tradable |
| 83 | `'S'` | You must be over level 10 to sell items on MapleStory Trading System |
| 其它 | — | Due to an unknown error the request for MTS has failed |

对应服务端枚举 `Application.Shared/Items/MTSClientErrorCode.cs`，发包函数
`MTSPacketCreator.MTSFailError(code)`（用 op 38，不会踢人）。

顺带得到一条客户端规则：**上架要求角色 > 10 级**（code `'S'` 由服务端下发，客户端只负责显示）。

---

## 9. 与 `src/` 的映射

| 客户端 | 服务端 |
|--------|--------|
| `ENTER_MTS 0x9C` | `Channel/Net/Handlers/EnterMTSHandler.cs`（注册在 `ChannelPacketProcessor`） |
| `0xFD` 子操作码 | `Channel/Net/Handlers/MTSHandler.cs` |
| `0xFC`（主窗口「查询」按钮） | `Channel/Net/Handlers/MtsCheckHandler.cs` → `MTSManager.ShowCash`（只回 `MTS_OPERATION2`） |
| `SET_ITC 0x7E` | `PacketCreator.openCashShop(c, mts: true)` |
| `0x15C` 各子操作码 | `Channel/Net/Packets/MTSPacketCreator.cs` |
| 商品条目 / 分页拼装 | `Channel/ServerData/MTSManager.cs`（`Query` / `Search` / `SendPageData`） |
| 商品数据、税费、购物车 | `Core.Login/ServerData/MTSManager.cs`（`DataStorageBase<int, MTSItemProto, MtsItemEntity>`） |
| gRPC | `Application.Protos/Service/MTSOperation.proto` + `Core.Login/Servers/MTSGrpcService.cs` |
| 频道 ↔ 中心 | `ServerTransports/IChannelServerTransport.cs` 的 `#region MTS`（`DefaultChannelServerTransport` 走 gRPC，`LocalChannelServerTransport` 直连） |
| `CHANGE_MAP 0x26` 空包 | `Channel/Net/Handlers/ChangeMapHandler.cs`（`p.available() == 0` → `LeaveCashShop`） |

税费公式 `GetTaxes(price) = 100 + price / 10`（即 `sub_5B219C` 里的 `售价 + 税费`）。

---

## 10. 客户端已验证 / 仍未确认

**客户端已验证（本次逐函数反编译）**

- `0x15C` 全部子操作码的分发表与各自读取字段（`sub_5A4311` 及各分支）；
- 商品条目 17 段布局与落点（`sub_5A2C0F`）；
- op 39 的 `tab - 1` 语义（`sub_5A4D68`）；
- op 34/36/38 的错误码表与 73 的踢人行为（`sub_5A4752` / `sub_5A4C2A` / `sub_5A4CE7` / `sub_5A4D49`）；
- op 21 / 33 / 35 的尾部差异；
- `0xFD` 各子操作码的完整构造点（19 处 `COutPacket(253)` 全扫）；
- op 2 / 18 / 4 的完整字段顺序与价格区间 `110..9999999`；
- op 16 = 列表购买、op 17 = 购物车购买（`sub_5BC1D5` 的 tab 分发）；
- op 5 与 op 6 的区别（op 6 多带一串搜索关键字、少两个 byte）；
- `SET_ITC` 的 5×Decode4 + 8 字节 FILETIME（`sub_59EF9D`），含 5000/7/500/24/168 的取值与用途；
- 退出走 `CHANGE_MAP(38)` 空包（`CITC::SendTransferFieldPacket` 0x59EF2B）；
- 主窗口三个按钮 `1000/1001/1002` 与 `0xFB` / `0xFC` / `CHANGE_MAP` 的对应关系，
  以及 `0xFC` 的回包 `MTS_OPERATION2` 在余额非负时清「请求在途」标志（§3.4）；
- 四个 tab 与 type 的组合语义（`sub_5BC1D5` 的 `v4[26]` / `v4[27]`，见 §2.1）；
- 条目 `+0x108` 是拍卖的**一口价**（卖家上架时自己填的），不是“当前最高出价”：
  `sub_5C373E` 用 `this[39]+this[40] < *(this[47]+264)` 二选一发 op 19 / op 20，
  而 `this[39]+this[40]` 初值就是 `条目[+0x10] + 条目[+0x0E]`，只有“`+0x108` 是一口价”
  这个读法自洽（§7 语义佐证）；
- `CRegisterAuctionEntryDlg::Confirm`（`0x5AD76B`）要求卖家填 **起拍价 + 一口价 + 时长 + 加价幅度**
  四个值，并校验 `起拍价 + 加价幅度 ≤ 一口价`（否则 `SP_4765
  THE_BUY_NOW_PRICE_IS_LOWER_THAN_THE_STARTING_BID`）—— 即“卖家自选一口价/拍卖”，
  不是“全部走拍卖、系统算一口价”（§2.4）；
- 拍卖买家实付 = `成交价 + 500 + 成交价×7%`，与普通出售同一套费率；
  客户端的确认框/余额校验按 `1949 + (1948+100)*价格/100` 计算（`sub_5C3947` `0x5C3972`、
  `sub_5BC1D5` `case 3` `0x5BC2F6`），服务端下发的 1948/1949 = `7`/`500` 使两者逐位相等；
  `sub_5C373E` 拿它**直接和余额比较**，所以它是总额而非附加费；
  列表的 `SP_4759_BUY_NOW__D` 行（`0x8F78B0`~`0x8F78C6`）另有一套写死的
  `一口价 + 一口价/10 + 100`，且该函数 4854 条指令内**没有**读 1948/1949
  （`+0x1E70` / `+0x1E74`），所以它与确认框必然不一致，服务端以确认框为准（§2.3.1）；
- op 2 与 op 18 的精确字段序、op 18 的时长是 **byte**、op 18 的第 4 个 int 是**一口价**（§4.1）；
- op 19 是 3 个 int 且**出价 = 第 2 + 第 3 个 int**，客户端发包前已校验
  `balance >= 出价 + 500 + 出价×7%`（§4.2）；
- op 3 的确切布局 `byte + int + 物品 + int`（§4.3）；
- tab 2 的 `a2==0` 是开对话框（`sub_59F811` → `sub_5C1384`），不是直接发包（§4.4）；
- op 11（`sub_59FA21`）与 op 13（`sub_59FB07`）结构完全相同，都只是 `Encode1(op) + Encode4(商品id)`；
- `0x15C` 的 31/32、37、41~44、47~50、55/56、60 各自的成功/失败文案（见 §6 表）；
- 条目第 11 段是**出价人数**（`sub_8F6DC9` 的 `0x8F7775`，`LOWORD(entry+12)` 与
  `SP_4766_BID_COUNT__D` 一起）、第 13 段 + 第 4 段 = **当前最高出价**
  （同一函数 `0x8F7A4C`）、第 15 段是**一口价**（`0x8F78BA`，客户端显示时自己加税费）、
  第 16 段是**单价**组成项（`0x8F7629`）、第 7 段是**结束时间**（剩余时间计算），
  由此推出“拍卖条目第 4 段必须填 0”（§7.2）。

**仍未确认**

1. 条目第 9~10 段（`+0x66` / `+0x98` 两个 50 字节串）、14 段、17 段（short）的真实语义。
   普通出售条目填空串/0 不会出错（客户端只做 `lstrcpy` 进定长缓冲）。求购人名字该落在
   哪一段仍未追，求购条目暂时把求购人名写进 `SellerName`（第 5~6 段）以保证名字能显示。
   第 11 / 13 / 16 段已由 `sub_8F6DC9` 确认（§7.2），第 8 段已确认为出售者名字。
1.1 条目第 7 段（`+0x2C`，8 字节）**已确认为结束时间**（§7.2 的剩余时间计算）：
   `sub_59FD85` 把它转成 `SYSTEMTIME` 后与当前时间作差，得到“剩余天:时”。
   服务端填 `sellEnds` 正确；`releaseDate` 只用于列表排序，不下发。
2. **op 11 的语义**。它和 op 13 字节级完全相同，只在“tab 4 且 type!=0”（我的求购）里
   作为 `a2==0`（主动作）出现，而 op 13 是 `a2==1`。面对自己的求购单谈不上“成交”，
   客户端也没给这条路准备专用的成功/失败操作码，因此服务端只回一次列表刷新
   （带收尾字节，能清掉客户端“请求在途”标志），不做业务动作。
3. op 3 里 `a2`（byte）与 `a5`（int）的真实语义没有展开。
4. op 5 与 op 6 的 tab 编号细节：op 6 在 `CITCWnd_Tab::OnButtonClicked` 里发的是
   `tab 控件索引 + 1`，op 5 直接发列表窗口 `+104`。两者在“出售 tab”上都应该是 1，但没有找到
   直接证明两者一定同域的证据；服务端目前按“线上就是 1..4”处理（`SALE_TAB = 1`、`CART_TAB = 4`），
   进入拍卖行时主动发的第 1 页也用 1，实测三方自洽。
5. §6 表里标“未使用”的 22 / 23 / 24 / 30 / 40 / 45 / 46 / 53 / 54 只确认了存在，
   没有逐字段展开各自的 UI 语义。
6. op 61（求购到期提示）与 op 62（“%s is sold for %d NX” 卖出/买入通知）的 payload 已知，
   但服务端还没有“主动向不在线/其他频道的玩家推送封包”的通道，故未实现。

---

## 11. 与 `src/modules` 旧实现的差异

频道侧大部分代码最初来自 `src/modules/Application.Module.MTS.*`。逐条对比如下
（“实测”指本文 §3~§8 的反编译结论）。

### 11.1 下发封包

| 项目 | modules 旧实现 | 客户端实测 | 结论 |
|------|---------------|-----------|------|
| `SET_ITC` 头 | `addCharacterInfo` + 账号名 + 28 字节常量块 | 同 | ✅ |
| op 21 头部 | `0x15, pages*16, count, tab, type, page, 1, 1` | 同（`sub_5A48AF` 的 5×Decode4 + 2×Decode1） | ✅ |
| op 21 条目 | `item, id, taxes, price, int 0, long 时间, str, str, 28×0` | `item, id, taxes, price, str, str, 8字节时间, str×3, int×6, short` | ❌ 字段顺序与长度都错：`int 0` 与时间被放到了两个字符串**之前**；尾部 28 个 0 实际应是 3 个串 + 6 个 int + 1 个 short |
| op 33 尾部 | `byte (0xD0+count)` + `{-1,-1,-1,0}` | `int + byte`（`sub_5A4AF3`） | ❌ 字节数与顺序都不对 |
| op 35 空列表 | 多写一个 `int 0` | `int count` 后直接结束（`sub_5A4C57`） | ❌ |
| op 39 | `0x27` + `int quantity` + `int pos` | `0x27` + `int tab(1基)` + `int slot` | ⚠️ 形状对、语义错（把背包页当成数量） |
| op 52 失败 | `0x34` + `0x42`（=66 `'B'`） | op 52 **无 payload**；错误码走 op 34/36/38 | ❌ 多写的 byte 客户端根本不读，等于没提示 |
| op 29 / 51 / 61 | `0x1D` / `0x33` / `0x3D`+2int | 一致 | ✅ |
| 货币包 | `MTS_OPERATION2` + 2 int | 一致（`sub_5A428C`） | ✅ |

### 11.2 上传封包解析

| op | modules 旧实现 | 客户端实测 | 结论 |
|----|---------------|-----------|------|
| 2 | 手写游走 `GW_ItemSlotBase`（`itemtype`/`stars`/owner/skip 32…）后读 `slot, quantity, price` | `GW_ItemSlotBase::Encode` + `int slot` + `int quantity` + `int price` + `byte` + `byte` | ⚠️ 末尾三段的顺序是对的（`OnRegisterSaleEntry(this, mode, invType, **slot**, item)`，包里先写 `a4`=slot 再写 `v22`=quantity），但前半段手写游走极其脆弱 |
| 3 | 空分支，不读 payload | `byte + int + 物品 + int`（`sub_59EE3F`） | 客户端**会发**，只是服务端不做求购 |
| 4 | `int,int,int,short,str` | `int itemid + int price + int qty + byte + byte + str`（`OnRegisterWishEntry`） | ✅ 字节级等价（`short` 恰好吃掉那两个 byte） |
| 5 | 只读 3 个 int | 还要读 `byte, byte, int searchType, str` | ❌ 丢掉搜索状态（不崩，但翻页/切类型后搜索态与服务端不一致） |
| 6 | `int,int,int,int,str` | 同（`CITCWnd_Tab::OnButtonClicked`） | ✅ |
| 7 / 8 / 9 / 10 | 各读 1 个 int | 都是 `int 商品id` | ✅ |
| 11 / 19 / 20 | 没有分支 | 都会发包 | 漏了（求购/拍卖，服务端不产出故不可达） |
| 12 / 13 | 空分支 | `int 商品id` | 客户端**会发**，求购类 |
| 14 | 空分支 | **客户端从不构造** | 死代码 |
| 16 / 17 | 列表购买 / 购物车购买 | 同（`sub_5BC1D5` 按 tab 分发） | ✅ 语义正确 |

### 11.3 其他

- `MTSItemInfo.getTaxes()` = `100 + price / 10` —— ✅ 与客户端一致（`sub_5B219C` 显示 `税费 + 售价`）。
- `MTSItemInfo.Map()` 用 `rs.Type != 1` 判断装备/普通物品 —— ⚠️ `Type` 是上架分类，真正的装备标记是
  `MtsItemEntity.Isequip`；`rs.Position` 也只对装备有意义。该类未纳入核心实现。
- `getEndingDate()` 把时间截断到“年月日”再转毫秒 —— ⚠️ 会让客户端显示的剩余时间出现最多一天的误差。

---

## 12. 本次改动

按上述结论修改/新增：

| 文件 | 改动 |
|------|------|
| `Channel/Net/Packets/MTSPacketCreator.cs` | 条目按 §7 的 17 段重写；`TransferInventory` 补 `int + byte` 尾；`NotYetSoldInv` 去掉尾部；`MTSConfirmTransfer` 改为 `(背包页1基, 格子)`；新增 `MTSFailError(byte)`（op 38）；条目第 11 段（`+0x0C`）填**出价人数**、第 12 段（`+0x0E`）填加价幅度、第 13 段（`+0x10`）填当前价、第 15 段（`+0x108`）填**一口价**、第 16 段（`+0x10C`）填单价；**第 4 段（`+0xFC`）在拍卖条目里固定填 0**（客户端算“当前最高出价 = 第 13 段 + 第 4 段”，见 §7.2），出售/求购仍填售价；新增 `MTSWantedRegistered`(55/56) / `MTSWantedFulfilled`(47/48) / `MTSWantedCanceled`(49/50) / `MTSWantedOffered`(31/32) / `MTSCartAdded`(41/42) / `MTSCartRemoved`(43/44) / `MTSCancelSell`(37) / `MTSConsecutiveBid`(60) |
| `Channel/Net/Handlers/MTSHandler.cs` | op 5 按 §4 完整读取（原来只读 3 个 int，会丢掉搜索条件）；op 2 价格上界补 `9999999`；`HandleSaleItem` 泛化为 `HandleSaleEntry(p, c, tail, auction)` 同时服务 op 2（tail 14）与 op 18（tail 22），op 18 按 §4.1 逐字段读 `slot/quantity/起拍价/一口价/时长/常量/加价幅度`；op 19 读 **3 个 int** 并把前两个相加作为出价；op 3/4/11/12/13/14/19/20 全部落地，其中 14 标注为客户端从不构造的死分支 |
| `Channel/ServerData/MTSManager.cs` | `Query` 增加“带客户端搜索状态”的重载；上架失败改用错误码（`'O'` / `'R'` / `'S'`）；上架数量超限改为提示框（客户端没有对应包，`op 61` 是求购到期文案，不能借用）；**领取 / 上架 / 拍卖上架 / 求购成交四处改走 `ItemTransferRunner` 统一流程**（§12.2），背包放不下回错误码 `'G'`；取消上架失败回 `'Q'`；购买 NX 不足回 `'B'`；新增 `RegisterWanted` / `CancelWanted` / `FulfillWanted` / `RegisterAuction` / `BidAuction` |
| `Channel/ServerData/ItemTransferRunner.cs`、`ItemTransfer.cs`、`TransferOutcome.cs` | 新增。物品转移的统一调度：占位 → 本地背包 → 提交，失败回滚；提交抛异常时用同一 token 重试，靠中心服幂等把“结果未知”消成“确定”（§12.2.3） |
| `Core.Login/ServerData/ItemTransferJournal.cs` | 新增。按 token 记录在途物品转移（5 分钟过期），提供幂等与“同一件商品只允许一个请求在途”（§12.2.2） |
| `Core.EF/Entities/MtsItemEntity.cs`、`Core.EF/DBContext.cs` | 商品表改成「结构化列 + 物品整块序列化」：只保留 `tab/type/seller/price/buy_now/bid_incre/bidder/sell_ends/release_date/transfer/item_blob`，物品本体（含装备属性、潜能、宠物、现金道具字段）按 `ProtoModel.ItemProto` 序列化进 `item_blob`，不再拆成 30 多个列；`sell_ends` 由 `varchar(16)` 改为 `bigint`；新增 `release_date`（上架时间，unix 毫秒）；最高出价者单独用 `bidder` 列（此前是借 `level` 列）；新增 `bid_count`（拍卖出价人数，§7.2） |
| `Core.Login/ServerData/MTSManager.cs` | `TakeItem` / `SaleItem` / `RegisterAuction` / `FulfillWanted` 四个入口一律按 `TransferStage` 分 `PREPARE` / `COMMIT` / `ABORT` 三段，并新增 `ParkItem` 把物品暂存进玩家的待领取列表作为兜底（§12.2）；`ChangePage` 增加 `tab==4 && type!=0` 的“我的求购”；新增 `RegisterWanted` / `CancelWanted` / `FulfillWanted` / `LoadWanted` / `RegisterAuction` / `BidAuction`；`Buy` 按 §2.3 用统一费率（`成交价 + 100 + 成交价/10`）并允许卖家离线；新增 `SettleExpired`（拍卖到期成交结算）；新增 `ResolveAccountId` / `ResolveName` / `TryChangeNx`（离线也能结算）；`MapModel`/`MapEntity`/`MapExsitedEntity` 打通 `price`(当前价)/`buyNow`(一口价)/`bidIncrement`(加价幅度)/`bidderId`/`bidCount`（分别落 `price` / `buy_now` / `bid_incre` / `bidder` / `bid_count` 五列），`BidAuction` 每接受一次出价累加 `BidCount`；`CalcBuyNow` 更名为 `BuyerTotalCost`（= `dealPrice + GetTaxes(dealPrice)`，是买家实付总额，不是一口价）；`RegisterAuction` 保存卖家填的一口价与加价幅度并校验 `一口价 ≥ 起拍价 + 加价幅度`；`BidAuction` 最低出价改为 `当前价 + 加价幅度` |
| `Core.Login/Servers/MTSGrpcService.cs` | 补 6 个新 rpc 的实现 |
| `ServerTransports/IChannelServerTransport.cs`、`DefaultChannelServerTransport.cs`、`Channel.InProgress/LocalChannelServerTransport.cs` | 补 6 个新方法（gRPC 与直连两种实现） |
| `Application.Protos/{Model/MTS.proto, Service/MTSOperation.proto}` | `MTSItemProto` 增 `buyNow`(一口价) / `bidIncrement`(加价幅度) / `durationHours` / `buyerName` / `bidderId` / `bidCount`(出价人数) / `releaseDate`，`price` 明确为“当前价”，原 `currentBid` 字段 `reserved 10`；新增 rpc `RegisterWanted`/`CancelWanted`/`FulfillWanted`/`LoadWanted`/`RegisterAuction`/`BidAuction` 与消息 `MTSOperationResponse`/`WantedRequest`/`ProductRequest`/`FulfillWantedRequest`/`BidRequest`；`SaleItemRequest` 增 `durationHours` / `buyNow` / `bidIncrement`；`TakeItemRequest` / `SaleItemRequest` / `FulfillWantedRequest` 各增 `token` + `stage`（取代原来的 `commit` / `abort` 两个 bool，§12.2.1） |
| `Application.Protos/Model/ItemTransfer.proto` | 新增。`enum TransferStage { PREPARE; COMMIT; ABORT; }` —— 物品跨进程搬运的通用阶段，独立于拍卖行（§12.2.1） |
| `Application.Shared/Items/MTSConstants.cs` | 新增。tab 号（`SALE_TAB`/`WANTED_TAB`/`AUCTION_TAB`/`CART_TAB`/`MY_WANTED_TAB`/`MY_WANTED_TYPE`）、`PAGE_SIZE`、`MAX_SALE_ITEMS`、`SALE_FEE`、`AUCTION_MIN_HOURS`/`AUCTION_MAX_HOURS`、`Fee_Rate`/`Fee_Base`（SET_ITC 下发的 1948/1949）、`SALE_DURATION` 以及 `GetTaxes` / `BuyerTotalCost` —— 原先中心服 `MTSManager`、频道服 `MTSManager`、`MTSPacketCreator` 各存一份副本（`AuctionTab`、`GetTaxes` 甚至有三份），现统一到一处 |
| `Application.Shared/Items/MTSClientErrorCode.cs` | 新增，§8 的错误码枚举 |
| `Application.Shared/Items/MTSResponseCode.cs` | 新增 `Bid_TooLow`、`Bid_Consecutive`、`InProgress`（领取占位被占用） |
| `Application.Resources/i18n/ClientMessage/{en-US,zh-CN}.json` | 新增 `MTS_NotEnoughMeso`、`MTS_ListingLimit` |

### 12.1 求购 / 拍卖的实现取舍

这两个品类在旧实现里完全没有，本次按 §2.1 的 tab 语义补齐。有几处是**有意做的近似**，
都记在这里：

1. **求购单不预扣抵用券。** 求购人登记时只校验参数与数量上限，成交那一刻才真正扣款；
   求购人当时钱不够就成交失败（op 48），卖方物品原样退回。
2. **求购单的 `SellerId` 存的是求购人**（买家）。成交后物品落进**买家的“待领取”列表**
   （买家可能不在线，不能直接进背包），卖家拿走抵用券并立刻同步余额。
3. **拍卖出价不扣款。** 出价只是“承诺”，真正的扣款发生在按一口价成交（op 20）或者到期结算时。
   —— 这样就不存在“被更高价顶掉之后要退款给一个可能已下线的玩家”的问题；
   代价是出价人可以超额出价，但成交时会再校验一遍余额。
4. **不能连续出价**用 `BidderId == 当前玩家` 判断，命中时下发 op 60
   （`YOU_CANNOT_MAKE_A_CONSECUTIVE_BID`）；出价低于“当前价 + 加价幅度”时用 op 38 的
   “未知错误”兜底（客户端没有“出价太低”的专用文案）。
5. **商品表的存储模型改成了「结构化列 + 物品整块序列化」**（`mts_items`）：
   `price` = **当前价**（起拍价，出价后被抬高成最高出价）、`buy_now` = **一口价**（卖家填的）、
   `bid_incre` = **加价幅度**、`bidder` = 当前最高出价者、`bid_count` = **出价人数**、
   `release_date` = 上架时间、`sell_ends` = 上架结束时间（改为 `bigint` 毫秒时间戳）。
   物品本体按 `ProtoModel.ItemProto` 序列化后整块存进 `item_blob`，不再拆成 30 多个列 ——
   这样装备属性 / 潜能 / 宠物 / 现金道具这些字段可以原样往返，不用每加一个字段就改一次表。
   出售者名字不再冗余存表，改由 `CharacterManager` 按 sellerId 取（`CharacterManager` 比
   `MTSManager` 先注册、先初始化，离线角色也在内存里）。
   > ⚠️ **数据库迁移尚未生成**（按当前约定只做编译校验）。`DBContextModelSnapshot` 与
   > `mts_items` 的实际结构会在下一次 `dotnet ef migrations add` 时对齐；在那之前对已有库
   > 直接启动会因为缺 `release_date` / `item_blob` / `bid_count`、`sell_ends` 类型不符而报错。
6. **手续费全拍卖行只有一套**（§2.3）：买家实付 = `成交价 + 500 + 成交价×7%`，
   公式在 `MTSConstants.BuyerTotalCost`（= `dealPrice + GetTaxes(dealPrice)`），卖家按成交价入账。
   SET_ITC 下发的 1948/1949 直接取 `MTSConstants.Fee_Rate` / `Fee_Base`，
   保证客户端按 `1949 + (1948+100)*价格/100` 算出的确认框金额与扣款逐位相等（§2.2）。
   列表「BUY NOW」那行是客户端写死的另一套数，服务端不去对齐它（§2.3.1）。
   **税费不是一口价**，一口价只来自卖家上架。
7. **拍卖到期会结算**（`SettleExpired`）：有出价时物品转给最高出价者、从他账上扣
   `最高出价 + 500 + 最高出价×7%`、卖家入账最高出价；出价者付不出钱则商品退回卖家并记日志。
   没有出价的拍卖、以及普通出售/求购单，到期后进登记人的“待领取”列表。
8. **op 3 / op 12 都走同一条“按求购单物品id 从背包取货”的路径**，不解析
   `GW_ItemSlotBase` 的可变长度编码 —— 与 op 2 的处理思路一致，避免再写一份脆弱的手工游走。
9. **op 11 只回列表刷新**，不做业务动作（原因见 §10 第 2 条）。
10. **结算用 `ResolveAccountId` 走“在线查内存、离线查数据库”**，所以卖家/求购人下线也能结算。
    原有的 `Buy` 也一并改成这个方式（原来要求买卖双方都在线）。

**刻意没改**

- `PacketCreator.openCashShop(c, mts: true)` 的 28 字节常量块：已与 §3.2 一致；
- 退出流程：`CHANGE_MAP` 空包已由 `ChangeMapHandler` 正确处理，不需要新的退出包；
- op 22 / 23 / 24 / 30 / 40 / 45 / 46 / 53 / 54：语义与已验证的操作码重复或未展开，
  服务端不下发（列表刷新统一用 op 21，错误统一用 op 38，成功统一用 29 / 51）；
- **到期通知**（op 61 求购到期、op 62 “%s is sold for %d NX”）：payload 已知，但服务端还没有
  “由中心服主动向某个频道的玩家推封包”的现成通道（现有 `BroadcastMessage` 是全服的），
  为此单独造一条通路不属于本次范围，先不做；
- `src/modules/Application.Module.MTS.*`：按项目约定暂不纳入范围（只在 §11 做了对照）。

### 12.2 MTS 的背包归属：中心服持有

进入 MTS 之后，频道侧的 `Player` 已经脱离世界（`WorldChannel.EnterExtralWorld` 把它移进
`PlayersAway`，不再参与任何世界内逻辑）。而 **MTS 和现金商城互斥** —— 两者都检查
`Player.IsInExtraServer()`（即 `IChannelClient.TradeServer` 非空），进入时各自把它设成
`new CashShopServer()` / `new MtsServer()`，离开时由 `ChannelClient.LeaveCashShop` 清空 ——
所以 MTS 期间不会发生现金仓存取（`CashOperationHandler` 的 `0x0D` / `0x0E`）这类频道侧改背包的操作。

因此 MTS 期间的背包所有者只有一个：**中心服缓存的 `CharacterLiveObject.Character.Data.Bag`**。
所有上架、领取、成交都直接改它；玩家离开 MTS 时频道会重新向中心服请求角色数据
（`ChannelClient.LeaveCashShop` → 重连 → `PlayerLoggedinHandler` → `LoginService.PlayerLogin`
返回带 `Character.Data.Bag` 的 `PlayerGetterProto`），背包自然是最新的。

这条链路让四个操作都变成**单进程内的内存操作 + 一次落库**，不再有跨进程的两阶段提交。

#### 12.2.1 中心服：`MtsBagStorage`

`Core.Login/ServerData/MtsBagStorage.cs` 提供背包页读写：

| 方法 | 用途 |
|------|------|
| `TakeBySlot(chr, invType, slot, quantity)` | 上架 / 拍卖上架：按格子取出（数量可拆） |
| `TakeById(chr, invType, itemId, quantity)` | 求购成交：按物品 id 取出卖方的交付物 |
| `Add(chr, invType, item)` | 领取 / 成交：放进第一个空格，返回落点；`0` = 放不下 |
| `GetSlotLimit(chr, invType)` | 格子上限取 `CharacterProto.Equipslots/Useslots/Setupslots/Etcslots`，现金页取 `DefaultConfigs.BagCashSize` |

**这里刻意不做叠加合并**：中心服没有 `ItemInformationProvider`（`slotMax` 在 WZ 模板里，只有频道加载），
猜一个上限可能造出超过上限的堆，反而会让客户端显示异常。所以进包一律找空格新建一摞；
背包满了就失败 —— 靠 `MTSResponseCode.InventoryFull` 告诉客户端，物品仍留在待领取列表里。

四个操作：

| 操作 | 中心服做的事 |
|------|-------------|
| 上架 / 拍卖上架 | `TakeBySlot` 从卖家背包取出 → 直接生成商品行（`InTransfer = false`） |
| 领取（op 39） | 商品行 → `Add` 进持有者背包 → 删行，返回落点 `Position` |
| 求购成交 | `TakeById` 扣卖方交付物 → 扣买家/付卖家 → 买家在 MTS 里就 `Add` 进背包，离线就 `ParkItem` 进待领取 |
| 购买 | 商品行转进买家待领取列表（客户端本来就走「购物车 → 领取」） |

`ParkItem` 仍然保留，只用于两个地方：买家离线（成交物进待领取），以及回滚时物品放不回背包的兜底。

#### 12.2.2 频道服：只发包，不碰背包

频道侧不再 `addFromDrop` / `removeFromSlot`，只负责把「客户端视图」同步过去：

- 上架 / 拍卖上架成功 → `SyncClientInventory(new InventoryRemove(invType, slot), true)`
- 领取成功 → 先用返回的 `ItemProto` 重建 `Item` 发 `InventoryAdd`（只是让**客户端视图**里有这件物品，
  频道那份 `Player.Bag` 仍然不动），再发 `MTSConfirmTransfer(背包页, Position)`（op 39）
- 求购成交成功 → 把本地那一格 `InventoryRemove` 掉

`InventoryAdd` 这一步不能省：`op 39` 的 `sub_5B3B00` 只在客户端已有的背包数据里选中格子
（§6.1.1），没有前置的 add，客户端选中的是空格。

频道本地那份 `Player.Bag` 在整个 MTS 期间是**陈旧且无权威**的，只用于查格子号发显示包，
以及给上架前的「能不能卖 / 等级 / 手续费」这类校验读物品属性。

#### 12.2.3 离开 MTS 时的同步

**正常离开不同步角色数据。** 出 MTS 走 `ChannelClient.LeaveCashShop`：清 `TradeServer` 后直接把
`PlayerPreEnterChannelCommand` 发下去，由命令在发 `CHANGE_CHANNEL` 之前设置账号切服状态
（`UpdateAccountState(..., PlayerServerTransition)` + `SetCharacteridInTransition`）。
重连时 `PlayerLoggedinHandler` → `LoginService.PlayerLogin` 本来就返回中心服那份
`PlayerGetterProto`，背包自然是最新的，频道那份旧镜像根本不需要上传。

理由：`LeaveMts` / `LeaveCashShop` 原来在 `CharacterManager.Update` 里命中的是 `keepMasterState`
分支 —— `origin.Character` 不赋值、`MergeAccountGame` 跳过、频道号不变也不触发 module 回调，
整个 `PlayerSaveProto` 传过去后被整份丢弃，只起到「设置状态位」的作用。状态位现在由命令直接设置，
这两个 trigger 已经删除（枚举里留空位，4 / 6）。

**在 MTS 里直接断线仍然要同步**（没有重连流程可依赖）：`Character.logOff` 按
`TradeServer is MtsServer` 选 `SyncCharacterTrigger.LogoffMts`，在 `CharacterManager.Update` 里命中
`keepMasterState`，保留中心服自己的 `Character.Data.Bag`，其余字段（地图、经验）取频道那份，
同时把账号置为 `LOGIN_NOTLOGGEDIN`。

> 没有采用「在角色模型上挂一个 MtsBagOwned 标记」的做法：会话类型本来就该由会话本身表达，
> 也不会在角色数据里留下需要清理的状态。

#### 12.2.4 这套模型的边界

- 跨进程原子性不再是问题：扣物品、付钱、移交都在中心服一个方法里完成；
- 但**频道本地背包在 MTS 期间是陈旧的**。任何新增的、会在 MTS 期间读 `Player.Bag` 的代码都会读到旧值 ——
  这是这套模型唯一的约束，改动那部分代码时要记得（MTS 内一切以中心服为准）；
- 物品进背包不做堆叠合并（见 §12.2.1），背包满时会提示而不是合并进已有摞；
- `MTSConfirmTransfer` 只带「背包页 + 格子」，**它本身不往客户端背包里放任何东西**
  （`sub_5B3B00` 只做选中 + 滚动，见 §6.1.1）。领取成功时频道必须先用
  `TakeItemResponse.Item` 重建 `Item` 并发一次 `INVENTORY_OPERATION` 的 `InventoryAdd`，
  再发 `op 39`，客户端那一格才有东西可选。

#### 12.2.5 奖励领取仍然走两阶段

CDK 兑换和 NPC 9010000（`RewardManager` + `ItemService.ClaimReward`）**不在 MTS 窗口里**：
那时玩家就在频道里，背包归频道所有。所以这两条仍然用
`PREPARE / COMMIT / ABORT` + `ItemTransferJournal` + `ItemTransferRunner`：
中心服先占位返回奖励包，频道预检空间并发放，发放成功才提交核销；
提交被拒就把已经发下去的道具/金币/点券按 `RewardGrant` 原样收回。

## 13. S→C 操作码分发表（`sub_5A4311`）逐个核对

- **IDB**：`` `Angel.exe ``，ImageBase `0x400000`，IDA Pro 8.3，Hex-Rays
- **分发表**：`sub_5A4311` @ `0x5A4311`，`size=0x1C8`
- **入口**：`MTS_OPERATION (0x15C)` 收包后先 `CInPacket::Decode1` 取操作码，再按表分发
- **本节的结论全部来自客户端反编译**；与 `src/` 的对应关系在 §13.2

### 13.1 分发表

| op | handler | 地址 |
|----|---------|------|
| 21 | `sub_5A48AF` | `0x5A48AF` |
| 22 | `sub_5A4882` | `0x5A4882` |
| 23 | `sub_5A4A02` | `0x5A4A02` |
| 24 | `sub_5A49E3` | `0x5A49E3` |
| 29 | `sub_5A4674` | `0x5A4674` |
| 30 | `sub_5A4581` | `0x5A4581` |
| 31 | `sub_5A46B2` | `0x5A46B2` |
| 32 | `sub_5A46F0` | `0x5A46F0` |
| 33 | `sub_5A4AF3` | `0x5A4AF3` |
| 34 | `sub_5A4C2A` | `0x5A4C2A` |
| 35 | `sub_5A4C57` | `0x5A4C57` |
| 36 | `sub_5A4CE7` | `0x5A4CE7` |
| 37 | `sub_5A4D14` | `0x5A4D14` |
| 38 | `sub_5A4D49` | `0x5A4D49` |
| 39 | `sub_5A4D68` | `0x5A4D68` |
| 40 | `sub_5A4DCF` | `0x5A4DCF` |
| 41 | `sub_5A4DFC` | `0x5A4DFC` |
| 42 | `sub_5A4E31` | `0x5A4E31` |
| 43 | `sub_5A4E66` | `0x5A4E66` |
| 44 | `sub_5A4E91` | `0x5A4E91` |
| 45 | `sub_5A4EC6` | `0x5A4EC6` |
| 46 | `sub_5A4FDC` | `0x5A4FDC` |
| 47 | `sub_5A5011` | `0x5A5011` |
| 48 | `sub_5A503C` | `0x5A503C` |
| 49 | `sub_5A5071` | `0x5A5071` |
| 50 | `sub_5A50DF` | `0x5A50DF` |
| 51 | `sub_5A5114` | `0x5A5114` |
| 52 | `sub_5A513F` | `0x5A513F` |
| 53 | `sub_5A5174` | `0x5A5174` |
| 54 | `sub_5A519F` | `0x5A519F` |
| 55 | `sub_5A51D4` | `0x5A51D4` |
| 56 | `sub_5A5209` | `0x5A5209` |
| 60 | `sub_5A5444` | `0x5A5444` |
| 61 | `sub_5A523E` | `0x5A523E` |
| 62 | `sub_5A52DE` | `0x5A52DE` |

op **25–28、57–59 不在表里**，客户端不会处理；服务端也不应下发。

### 13.2 已核对的字段序（按 Decode 顺序）

**op 39 — 物品已进背包（`sub_5A4D68`）**

```c
v3 = Decode4();          // 背包页，1 基
v4 = Decode4();          // 落点格子
CCtrlTab::SetTab(ctrlTab, v3 - 1);
sub_5B3B00(v4);          // 只在本地已缓存的背包里「选中 + 滚动」，不放物品（见 §6.1.1）
Notice(SP_4787_THE_ITEM_HAS_BEEN_TRANSFERRED);
```

即「**int 背包页(1 基) + int 格子**」。旧实现按 Java 版的 `item.getQuantity()` 填第一个 int
是错的；现在 `MTSPacketCreator.MTSConfirmTransfer(int page, int slot)` 由调用方传
`(int)ItemConstants.getInventoryType(itemId)`（装备=1…现金=5）与中心服返回的落点。

**op 35 — 我的上架中（`sub_5A4C57`）**

```c
v4 = Decode4();          // 条数
sub_59FF5F(list);        // 清空
for (i = 0; i < v4; i++) sub_5A2C0F(entry, packet);   // 17 段条目
```

无尾部字节。

**op 33 — 待领取列表（`sub_5A4AF3`）**

```c
v3 = Decode4();                  // 条数
sub_59FF5F(this + 19);
for (i = 0; i < v3; i++) entry;
v4 = Decode4();                  // 超出显示上限、未能画出来的数量
if (this[1957] && v4 > 0)
    Notice(Format(SP_4807_THERE_ARE_D_ITEMS_IN_TRANSFER_INVENTORY..., v3 + v4));
result = Decode1();              // 收尾字节
if (result) this[6] = 0;         // 清「请求在途」标志
```

**op 38 — 带原因的失败（`sub_5A4D49`）**

```c
v3 = Decode1();          // error code
this[6] = 0;
sub_5A4752(this, v3);    // 弹对应提示
```

**op 30 — 通用结果码（`sub_5A4581`）**

```c
v3 = Decode1();      // 结果码
this[6] = 0;         // 清「请求在途」
if (v3 == 72)        // 0x48
{
    v4 = Decode2();  // short：上限
    Notice(Format(SP_4778_LIMIT_TO_D_UNITS_OR_LESS, v4));
}
else if (v3 == 63 || v3 == 68)   // 0x3F / 0x44
{
    sub_5A4752(this, v3);        // 弹提示
    CITC::SendTransferFieldPacket();   // 强制离场
}
else if (v3 == 66 || v3 == 67 || v3 == 71 || v3 == 79 || v3 == 82 || v3 == 83)
    sub_5A4752(this, v3);        // 只弹提示
// 64 / 65 / 69 / 70：什么都不做
```

即「**byte 结果码**」，**码 72 时再跟一个 short**。

注意码 72 的文案是「最多 N 个**单位**」（单次交易的数量上限），
**不是**「最多同时上架 N 件」——上架件数超限客户端没有专用包，只能走提示框。

**错误码表（`sub_5A4752` @ `0x5A4752`）**，op 30 / 34 / 36 / 38 共用：

| 码 | 字符 | 文案 |
|----|------|------|
| 66 | `'B'` | `SP_4783_YOU_DO_NOT_HAVE_ENOUGH_NX` |
| 67 | `'C'` | `SP_5599_YOU_DO_NOT_HAVE_ENOUGH_MESOS` |
| 68 | `'D'` | `SP_4784_AN_ERROR_HAS_OCCURRED_WHILE_USING_NX` |
| 71 | `'G'` | `SP_5603_YOUR_INVENTORY_IS_FULL` |
| 73 | | `SP_4785_FAILED_TO_LOAD_THE_LIST` |
| 78 | `'N'` | `SP_4768_UNABLE_TO_PULL_OUT_OF_THE_SALE_R_NYOU_HAVE_AT_LEAST_1_BID` |
| 79 | `'O'` | `SP_4810_THIS_ITEM_MAY_NOT_BE_SOLD` |
| 81 | `'Q'` | `SP_4814_THE_ITEM_HAS_BEEN_SOLD_R_NYOU_CANNOT_CANCEL_THE_SALE` |
| 82 | `'R'` | `SP_4816_THROWING_STARS_ARE_NOT_TRADABLE` |
| 83 | `'S'` | `SP_4837_YOU_MUST_BE_OVER_LEVEL_10_TO_SELL` |
| 其它 | | `SP_4808_DUE_TO_AN_UNKNOWN_ERROR_R_NTHE_REQUEST_FOR_MTS_HAS_FAILED` |

响应码即 ASCII 字符本身（`'B'`=66 等），与 `Application.Shared/Items/MTSClientErrorCode.cs` 一一对应。

**op 34 / op 36 — 失败，且 code==73 时强制离场（`sub_5A4C2A` / `sub_5A4CE7`）**

```c
v3 = Decode1();          // error code
this[6] = 0;
sub_5A4752(this, v3);
if (v3 == 73) CITC::SendTransferFieldPacket();   // 把玩家送出拍卖行
```

需要「强制离场」的失败场景必须用 op 34/36，op 38 不会踢人。

**op 21 — 商品列表（`sub_5A48AF`）**

```c
v4  = Decode4();   // this[22] / this[1493]
v15 = Decode4();   // this[23] = 条目数（循环上限）
v12 = Decode4();   // this[26]
v11 = Decode4();   // this[27]
v13 = Decode4();   // this[28]
v10 = Decode1();   // this[1463]
v9  = Decode1();   // this[1464]
for (i = 0; i < v15; i++) entry;
result = Decode1();  // 收尾字节
```

即「**5×int + 2×byte + 条目 + 1×byte**」；第 2 个 int 才是条目数。
`MTSPacketCreator.SendMTS` 的顺序与此一致（第 1 个 int 目前写的是 `pages * 16`，
语义仍未确认，见 §13.4）。

**无负载的操作码（客户端只弹提示）**

| op | handler | 提示 |
|----|---------|------|
| 29 | `sub_5A4674` | `SP_4779_SUCCESSFULLY_REGISTERED_THE_ITEM_FOR_SALE` |
| 31 | `sub_5A46B2` | `SP_4781_SUCCESSFULLY_REQUESTED_THE_ITEM_FOR_SALE` |
| 37 | `sub_5A4D14` | `SP_4786_SUCCESSFULLY_CANCELED_THE_SALE` |
| 41 | `sub_5A4DFC` | `SP_4788_THE_ITEM_HAS_BEEN_SUCCESSFULLY_PLACED_IN_YOUR_CART` |
| 51 | `sub_5A5114` | `SP_5602_YOU_HAVE_SUCCESSFULLY_PURCHASED_THE_ITEM` |

**op 32 — 对求购单出价失败（`sub_5A46F0`）**

```c
this[6] = 0;
v2 = Decode1();                    // 原因码
if (v2 == 80 /*0x50*/) Notice(SP_4806_THE_ITEM_IS_CANCELLED_OR_SOLD_OUT);
else if (v2 == 82 /*0x52*/) Notice(SP_4816_THROWING_STARS_ARE_NOT_TRADABLE);
else Notice(SP_4782_REQUESTING_THE_ITEM_FOR_SALE_FAILED);
```

即「**byte 原因码**」：`80` = 求购单已被取消/售出，`82` = 飞镖类不可交易，其它 = 通用失败。
**之前按空包发是错的**，客户端会去读一个不存在的字节。

**op 61 — 挂单/求购到期（`sub_5A523E`）**

```c
v1 = Decode4();
v2 = Decode4();
if (v1 >= 0 && v2 > 0)
    Notice(Format(SP_4815_THE_LISTING_PERIOD_ON__WANTED__ITEMS..., v2));   // 只代入 v2
```

两个 int；文案只用到第二个（到期条数），第一个只参与「要不要提示」的判断。

**op 62 — 成交提示（`sub_5A52DE`）**

```c
flag = Decode1();       // 1 = 卖出，其它 = 买入
itemId = Decode4();
GetItemName(itemId);
nx = Decode4();
DecodeBuffer(time, 8);  // FILETIME
Format("%d/%d/%d %d:%d:%d", time);
flag == 1 ? SP_4761_S_OF_S_R_NIS_SOLD_FOR_D_NX : SP_4762_S_OF_S_R_NIS_BOUGHT_FOR_D_NX;
```

即「**byte + int itemId + int nx + 8 字节时间**」。

### 13.3 与 `src/` 的对应

`src/Application.Core/Channel/Net/Packets/MTSPacketCreator.cs`（本次已按上表修正）：

| op | 方法 | 本次改动 |
|----|------|---------|
| 39 | `MTSConfirmTransfer(int page, int slot)` | 原为 `(int quantity, int pos)`；按 `sub_5A4D68` 改为「1 基背包页 + 格子」，补文档 |
| 38 | `MTSFailError(code)` | 注释补充：op 34/36 在 code 73 时会强制离场，op 38 不会 |
| 61 | `MTSWantedListingOver(int reserved, int expiredCount)` | 原参数名 `(nx, items)` 语义反了；按 `sub_5A523E` 改名并补文档 |
| 62 | `MTSItemSold(bool sold, int itemId, int nx, DateTimeOffset time)` | 新增，布局按 `sub_5A52DE` |
| 30 | `MTSOperationResult(MTSClientErrorCode code)` / `MTSLimitUnits(short limit)` | 新增；码表按 `sub_5A4752` 补全，码 72 跟 short（单位上限，**不是**上架件数） |
| 32 | `MTSWantedOfferedFailed(byte reason)` | 新增：op 32 带原因码（80 已取消/售出、82 飞镖不可交易），原先发空包是 bug |
| 21 / 33 / 35 | `SendMTS` / `TransferInventory` / `NotYetSoldInv` | 核对无改动（结构一致） |
| 29 / 31 / 37 / 41 / 51 | `MTSConfirmSell` / `MTSWantedOffered(true)` / `MTSCancelSell` / `MTSCartAdded(true)` / `MTSConfirmBuy` | 核对无改动（无负载） |

### 13.4 仍未确认

- **op 21 的第 1 个 int**：客户端存进 `this[22]` / `this[1493]`，没有直接用来渲染，
  语义（总条数？总页数？）未定。现实现写 `pages * 16`，属于按经验填的值；
- **op 21 的第 3–5 个 int 与两个 byte 的确切用途**：顺序与现实现一致（tab/type/page + 两个 flag），
  但没有找到把它们直接喂给控件的代码路径，只能确认「顺序不错」；
- **op 22 / 23 / 24 / 40 / 43 / 45 / 46 / 47 / 48 / 50 / 53 / 54 / 55 / 56**：只确认了
  「分发表收尾 / 无尾部」这类结构信息，字段序未逐个展开（服务端目前不发这些包，或只发空包）。
- 已经逐个核对过的：`30`（含码表）、`32`（原因码）、`42`/`44`/`49`/`52`/`60`（确认无载荷）。

### 13.5 两个错误码枚举的 IDA 校验

| 枚举 | 客户端依据 | 结论 |
|------|-----------|------|
| `MTSClientErrorCode`（`Application.Shared/Items`） | `sub_5A4752` @ `0x5A4752` | 与客户端逐条一致 |
| `MTSOperationCode`（`Application.Shared/Items`） | `sub_5A4581` @ `0x5A4581` | 与客户端逐条一致 |

**op 34 / 36 / 38 —— `MTSClientErrorCode`**

`sub_5A4752` 对**任意** byte 都会弹提示；有专属文案的只有：

`66 'B'` NX 不足、`67 'C'` 金币不足、`68 'D'` 使用 NX 出错、`71 'G'` 背包已满、
`73` 加载列表失败、`78 'N'` 有出价不能撤单、`79 'O'` 不可出售、
`81 'Q'` 已售出不能取消、`82 'R'` 飞镖不可交易、`83 'S'` 未满 10 级；

其余（含 `0`）一律走 `SP_4808_DUE_TO_AN_UNKNOWN_ERROR…`。
其中只有 op 34 / 36 会在 **73** 时调 `CITC::SendTransferFieldPacket()` 把玩家送出拍卖行，op 38 不会。

**op 30 —— `MTSOperationCode`**

`sub_5A4581` 只认下面这些码，其余**完全静默**（只清「请求在途」标志）：

| 码 | 行为 |
|----|------|
| `63` | 弹兜底提示（`sub_5A4752` 的 SP_4808）+ 强制离场 |
| `66` / `67` / `71` | 弹对应提示（NX 不足 / 金币不足 / 背包已满） |
| `68` | 弹提示（NX 异常）+ 强制离场 |
| `72` | 再读一个 `short`，提示 `SP_4778_LIMIT_TO_D_UNITS_OR_LESS`（最多 N 个单位） |
| `79` / `82` / `83` | 弹对应提示（不可出售 / 飞镖不可交易 / 未满 10 级） |
| 其它（含 `0`、`64`、`65`、`69`、`70`、`73`–`78`、`80`、`81`、`≥84`） | 静默 |

**两处容易误用的不对称**：

- `78 'N'`（有出价，不能撤单）和 `81 'Q'`（已售出，不能取消）在 `MTSClientErrorCode` 里有文案，
  但 **op 30 不认这两个码** —— 「撤回出售失败」只能走 `MTSFailError`（op 38），
  否则玩家什么提示都看不到；
- 反过来 `72`（数量上限）只有 op 30 支持，op 38 发 72 只会得到「未知错误」兜底文案。
