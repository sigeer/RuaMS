---
name: refactor-from-js
description: "实现`/src/Application.Plugin.Script/` 下的 尚未实现的方法（含有`// TODO` 注释）"
---

## npc

`/srcApplication.Plugin.Script/Npc`下的文件

### 映射关系

该目录下虽然有多个文件，但都是一个类拆分成了多个文件。
类中有多个方法，每个方法对应一个npc。方法上有注释，包含了NpcId: `Npc: {npc_id}`。
那他对应的js脚本就是 `/src/Application.Resources/scripts/npc/{npc_id}.js`。

比如：`/src/Application.Plugin.Script/Npc/Salo.cs`下的`hair_mureung1`方法，对应的js脚本就是 `/src/Application.Resources/scripts/npc/2090100.js`。

### 对话逻辑

js脚本中，会有2个方法: `start`和`action(mode, type, selection)`
其中`start`方法对应npc对话开始时的逻辑，然后玩家在对话中的选择/回复则是调用`action(mode, type, selection)`方法，传入不同的参数以达到连续对话的效果

而C#中，通过`await`等待玩家操作，实现对话的连续性（这里玩家是如何回复的与当前技能无关。

|js|c#|
|--|--|
|`sendOk`|`await SayOK`|
|`sendNext`|`await SayNext`|
|`sendYesNo`|`await SayYesNo`|
|`sendAcceptDecline`|`await SayAcceptDecline`|
|`sendSimple`|`await SayOption`|
|`sendStyle`|`await SayStyle`|
|`sendGetNumber`|`await SayInputNumber`|
|`sendGetText`|`await SayInputText`|
|`sendNext` + `sendNextPrev`|`await SaySpeech`|

这样用一个方法达到了原先js脚本中2个方法的效果。

## quest

任务，也是对话形式，所以对话逻辑与npc一样

### 映射关系

`/src/Application.Plugin.Script/Quest`下虽然有多个文件，但都是一个类拆分成了多个文件。
类中有多个方法，方法上有注释，包含了QuestId: `Quest: {quest_id}`。
那么他对应的js脚本就是 `/src/Application.Resources/scripts/quest/{quest_id}.js`。
但是js中可能会有2个方法: `start` 和 `end`。
start 方法对应 quest 开始时的逻辑，end 方法对应 quest 结束时的逻辑。
这里C#中也会用2个方法对应：`q{quest_id}s`和`q{quest_id}e`。

比如：`/src/Application.Plugin.Script/Quest/A6.cs`下的`q21000s`方法，对应的就是 `/src/Application.Resources/scripts/quest/21000.js`的`start`方法。

### 对话逻辑

任务脚本中也会有对话，所以这一块可以参照npc脚本的对话逻辑。

## 其他说明

- C# 中大部分的方法都已经实现了，只需要处理那些方法体中有`// TODO`的。
- 如果对应的js脚本不存在，则跳过
- 如果代码字符串中存在英文，则对其汉化。一些特殊替换域除外（#号开始、#号结束）
- 在处理大量方法时，如果可以复用一些代码，尽量提取出来，避免重复编写。
- 生成C#代码时，对代码进行优化，使其可读性更高。
- 最终的代码应该符合C#的语法规范，避免使用不合法的语法。

## 举例

js脚本举例
```js
function start() {
    cm.sendOk("Hello, welcome to my shop.");
}
function action(mode, type, selection) {
    cm.dispose();
}
```
则其对应的C# 脚本应该是
```csharp
public async Task hair_mureung1()
{
    await SayOK("你好，欢迎光临我的店铺。");
}
```

## 执行流程

1. 定位目标 C# 文件（通常是 `/src/Application.Plugin.Script/Npc/*.cs` 或 `/src/Application.Plugin.Script/Quest/*.cs`）。
2. 找到其中标记了 `// TODO` 的方法（例如 `hair_mureung1` 或 `q21000s`）。
3. 根据方法上的注释（`Npc: {npc_id}` 或 `Quest: {quest_id}`）找到对应的 JS 文件路径，**这一步很重要，绝对不能搞错**。
4. 如果 JS 文件不存在，则跳过并输出提示。
5. 存在对应的 JS 脚本，则输出这个JS脚本文件名。
6. 读取 JS 文件内容，解析其中的 `start` 和 `action`（或 `start`/`end`）逻辑。
7. 按照对话映射表，将 JS 中的顺序调用（sendOk / sendNext / …）转换为 C# 中连续的 `await SayXXX` 语句。
8. 对字符串中的英文进行汉化（保留特殊替换域，如 `#b`、`#k`、`#n` 等）。
9. 用转换后的代码替换原 C# 方法体中 `// TODO` 所在的位置（或整个方法体）。
10. 有多个方法时，逐个替换，避免一次性替换整个文件。
11. 按以上流程做完后，回顾代码，如果有大量重复的代码，尽量提取、优化，避免重复编写，但是这个修改不能影响原有功能。