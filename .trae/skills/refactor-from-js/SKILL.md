---
name: refactor-from-js
description: "将js对话脚本用C#重写"
---

## 部分方法映射

|js|c#|
|--|--|
|`sendOk`|`await SayOK`|
|`sendNext`|`await SayNext`|
|`sendYesNo`|`await AskYesNo`|
|`sendAcceptDecline`|`await SayAcceptDecline`|
|`sendSimple`|`await AskMenu`|
|`sendStyle`|`await AskAvatar`|
|`sendGetNumber`|`await AskNumber`|
|`sendGetText`|`await AskText`|
|`sendNext` + `sendNextPrev`|`await SaySpeech`|

## 文件映射

### npc

#### 范围

js: `/src/Application.Resources/scripts/npc`
C#: `/src/Application.Plugin.Script/Npc`
  
#### 映射关系

`/src/Application.Plugin.Script/Npc`下有多个方法，需要通过方法的注释来查找对应的C#方法。
另外注释可能不止1个NpcId: `// Npc: {npc_id}, {npc_id2}, ...`，即可能多个js文件对应1个C#方法。

#### 对话逻辑

js脚本中，会有2个方法: `start`和`action(mode, type, selection)`
其中`start`方法对应npc对话开始时的逻辑，然后玩家在对话中的选择/回复则是调用`action(mode, type, selection)`方法，传入不同的参数以达到连续对话的效果

而C#中，通过`await`等待玩家操作，实现对话的连续性（这里玩家是如何回复的与当前技能无关。）
这样用一个方法达到了原先js脚本中2个方法的效果。

### quest

任务，也是对话形式，所以对话逻辑与npc一样，

#### 范围

js: `/src/Application.Resources/scripts/quest`
C#: `/src/Application.Plugin.Script/Quest`

#### 映射关系

`/src/Application.Plugin.Script/Quest`下虽然有多个文件，但是一个类拆分成了多个文件。
类中有多个方法，方法上有注释，包含了QuestId: `Quest: {quest_id}`。

但是js中可能会有2个方法: `start` 和 `end`。
start 方法对应 quest 开始时的逻辑，end 方法对应 quest 结束时的逻辑。
这里C#中用2个方法对应：`q{quest_id}s`和`q{quest_id}e`。

既将`quest_id.js`中的`start`方法重写成C#的`q{quest_id}s`方法，`end`方法重写成`q{quest_id}e`方法。

### 对话逻辑

任务脚本中也会有对话，所以这一块可以参照npc脚本的对话逻辑。

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

## 其他说明

遇到以下情况需要特殊处理：

1. 当C#中没有找到对应的方法时，对于npc脚本，在`NpcScript.cs`中 用`n{npc_id}`作为方法名创建方法；对于quest脚本，则在`QuestScript.cs`中创建相应的`q{quest_id}s`,`q{quest_id}e`方法。
2. 对字符串中的英文进行汉化（保留特殊替换域，如 `#b`、`#k`、`#n` 等）。
3. 在处理大量方法时，如果可以复用一些代码，尽量提取出来，避免重复编写。
4. 生成C#代码时，对代码进行优化，使其可读性更高；最终的代码应该符合C#的语法规范，避免使用不合法的语法。
