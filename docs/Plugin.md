# 插件开发指南

> 说明 `Application.Plugin.*` 的开发方式与实现原理，依据 `src/Application.Core/Gameplay/Plugins/` 下的现有基础设施。
> 最后核对：2026-11-12（`master`，`cc44fffe4`）。

---

## 1. 插件是什么

插件是一个**运行时加载的独立程序集**，宿主（`Application.Host` / `Application.Host.Channel`）在编译期不引用它：

- 宿主进程启动后，`WorldChannelServer` 创建 `PluginManager`（`src/Application.Core/Channel/WorldChannelServer.cs:128,182`），
  扫描程序基目录下的 `Plugins/`。
- 文件名必须匹配 `Application.Plugin.*.dll`（`WorldChannelServer.cs:341,404`），其他 DLL 不会被当作插件。
- 每个插件在**独立的可回收 `AssemblyLoadContext`** 中加载，卸载后程序集可被 GC 回收。
- 插件只在**频道侧**加载：`Application.Host.Channel` 与单进程模式下的 `Application.Host` 都走
  `WorldChannelServer`，Master 侧不加载插件。

与 `modules/` 的区别：模块是编译期引用、通过 DI 注册的普通类库；插件是运行期加载、可热更新的独立程序集。
需要热更新、或希望第三方扩展而不改宿主代码时选插件。

---

## 2. 运行期目录

| 目录 | 位置 | 作用 |
|---|---|---|
| `Plugins/` | `AppDomain.CurrentDomain.BaseDirectory/Plugins`（`PluginManager.cs:58`） | 运维放置插件 DLL 的**源目录**，也是 `FileSystemWatcher` 监视的目录 |
| `PluginShadowCopy/<guid>/` | 程序基目录（`PluginManager.cs:60`） | 每次加载复制一份 DLL 的**卷影副本**，从副本加载；卸载时删除 |

`BaseDirectory` 是程序集所在目录，不是仓库根目录：

- `dotnet run` / 单进程：`src/Application.Host/bin/<Configuration>/<TargetFramework>/Plugins/`
- 多进程：频道服务器进程自己目录下的 `Plugins/`

插件项目自带 `CopyToPlugins` 目标，`dotnet build` 后会把 DLL 复制到 `Application.Host` 的输出目录
（见 `src/Application.Plugin.Script/Application.Plugin.Script.csproj:16`）。**它不会复制到
`Application.Host.Channel` 的输出目录**——跑多进程模式时需要手动放置。

卷影副本的意义：Windows 上正在使用的 DLL 会被锁定、不能覆盖，复制到新文件后再加载，运维才能直接覆盖
`Plugins/` 下的原文件完成更新。

---

## 3. 加载 / 卸载流程

### 3.1 加载

`PluginManager.LoadPlugin(fileName)`（`PluginManager.cs:63`）实际做这些事（`LoadPluginInternal` / `LoadPluginFromSource`）：

1. **复制到卷影目录**（`PluginManager.cs:163-167`）。
2. **创建 `PluginLoadContext`**（`isCollectible: true`，`PluginLoadContext.cs:12`），从副本加载程序集。
3. **查找入口类型**：程序集里必须有**且只能有一个**非抽象、实现 `PluginServiceBase` 的类型
   （`PluginManager.cs:187-198`）。为零或为多都抛 `InvalidOperationException`。
4. **构造入口实例**：`Activator.CreateInstance(入口类型, container)`，构造参数是
   `PluginContainer<PluginServiceBase>`。
5. **同名插件先卸载**：`RemovePluginInternal(pluginKey)`（`PluginManager.cs:82`），因此重复
   `LoadPlugin` 就是「热替换」。
6. **重建注册表**：
   - `RebuildRegistrations()`（`:374`）——按优先级排好「服务接口实现」列表；
   - `RebuildScripts()`（`:243`）——重扫所有插件程序集，合并成生效脚本表；
   - `SyncEventTemplatesAsync()`（`:309`）——把合并后的事件模板下发到各频道。
7. **`OnMounted()`**（`:103`）：失败只记日志，不影响插件已注册的内容。
8. 任何一步抛异常：把插件从容器表里摘掉、重建注册表、卸载上下文并抛给调用方（`:91-99`），
   不会留下半加载状态。

### 3.2 卸载

`UnloadPlugin` → `RemovePluginInternal`（`:112`）顺序固定：

1. 从 `_pluginContainers` 移除；
2. `RebuildRegistrations()` + `RebuildScripts()` + `SyncEventTemplatesAsync()` —— **先摘注册再卸 ALC**，
   因为事件管理器持有插件类型，必须在程序集卸载前释放；
3. `PluginContainer.DisposeAsync()`：
   - `RequestTracker.DisposeAsync()` 排水（等待在途脚本，默认最多 30 秒，`RequestTracker.cs:12`）；
   - 入口服务 `DisposeAsync()`（插件自己的清理逻辑）；
   - `LoadContext.Unload()`；
   - 删除卷影目录。

### 3.3 热更新

`WorldChannelServer` 在注册完成后启动 `FileSystemWatcher`（`WorldChannelServer.cs:341-347`）：

| 事件 | 处理 |
|---|---|
| `Changed` / `Created` | 1 秒内只处理一次，再延迟 200ms 等文件写完，然后 `LoadPlugin`（`:421`） |
| `Deleted` | `UnloadPlugin`（`:437`） |

也就是说：**覆盖 `Plugins/` 下的 DLL = 重载，删除 = 卸载**。覆盖同名文件时 `LoadPlugin` 内部会先卸载旧版本。

---

## 4. 插件契约

### 4.1 入口类

```csharp
// 每个插件程序集恰好一个
internal sealed class MyService : PluginServiceBase, IScriptNpcService
{
    public MyService(PluginContainer<PluginServiceBase> container) : base(container) { }

    public override Task OnMounted() { /* 挂载后初始化 */ return base.OnMounted(); }
    public override ValueTask DisposeAsync() { /* 清理 */ }
}
```

`PluginServiceBase`（`PluginServiceBase.cs`）提供：

| 成员 | 说明 |
|---|---|
| `Priority` | `High=0 / Normal=1 / Low=2`，数值越小优先级越高，默认 `Normal`。`Application.Plugin.Script` 用 `Low`，所以第三方插件用默认值即可覆盖内置脚本 |
| `Container` | `PluginContainer`：`Logger`、`Node`（`WorldChannelServer`）、`Tracker`、`PluginAssembly` |
| `OnMounted()` | 加载完成后调用 |
| `DisposeAsync()` | 卸载前调用，必须实现 |
| `_node` / `_pluginName` / `LogInfo` / `LogWarning` / `LogError` | 便捷字段与方法；日志分类为 `Plugin/{程序集名}` |

构造参数只有 `container`，**没有 DI 注入**。需要宿主服务时自行取：

```csharp
var executor = _node.ServiceProvider.GetRequiredService<CommandExecutor>();
```

（`FakerService.cs:150` 即按此方式注册 GM 命令。）

### 4.2 服务接口声明「调用模式」

`[PluginInvocation(mode)]`（`PluginInvocationAttribute.cs`）标在**接口**上，决定该接口的实现如何被调用：

| 模式 | 语义 | 现有接口 |
|---|---|---|
| `FirstMatch` | 同名请求由优先级最高的实现处理（用于脚本注册类接口） | `IScriptNpcService` / `IScriptQuestService` / `IScriptItemService` / `IScriptPortalService` / `IScriptMapService` / `IScriptReactorService` / `IScriptEventService` |
| `Broadcast` | 每个事件通知**所有**实现（用于监听类接口） | `IPluginMobService` / `IPluginMapService` / `IPluginMapObjectService` |

`PluginManager` 用 `RequireMode`（`:401`）校验调用方式与标注一致，用错直接抛异常。
一个入口类可以同时实现多个接口（如 `ScriptService` 同时实现 7 个脚本接口 + `IPluginMapService`）；
`RebuildRegistrations`（`:374-396`）保证**每个插件在每个模式列表里只出现一次**，避免同一插件的脚本被算两遍。

> 新增服务接口时必须标注 `[PluginInvocation]`，否则 `GetServices` 返回空列表——调用会静默变成空操作。
> `PluginManagerScriptTests` 专门守住这类静默失败。

---

## 5. 脚本插件：声明方式

### 5.1 种类由基类决定

`ScriptFeatureDiscovery`（`ScriptFeatureDiscovery.cs:24`）只认**直接基类**：

| 基类 | `ScriptKind` | 构造参数 | 触发点 |
|---|---|---|---|
| `NpcScriptBase` | `Npc` | `(IChannelClient, int npc, NPC?)` | 点击 NPC |
| `QuestScriptBase` | `Quest` | `(IChannelClient, Quest, int npc)` | 任务开始/结束对话 |
| `ItemScriptBase` | `Item` | `(IChannelClient, Item, int npcId)` | 道具脚本 |
| `PortalPlayerInteraction` | `Portal` | `(IChannelClient, IPortal)` | 进传送门 |
| `MapScriptMethods` | `Map` | `(IChannelClient, IMap)` | `onUserEnter` / `onFirstUserEnter` |
| `ReactorHitScriptBase` 等 4 个 | `ReactorHit/Act/Touch/Untouch` | `(IChannelClient, Reactor)` | 反应堆 |

要求：**直接继承**上表基类（`TryResolveKind` 只看一层 `BaseType`，`:98`），中间不要再插自定义基类。
本仓库 165 个处理器全部满足这一点。

### 5.2 方法名 = 脚本名

`TypeUtils.LoadFromType`（`ScriptNameAttribute.cs:30`）扫描类型**自己声明的公开实例方法**：

- 有 `[ScriptName(...)]`：每个名字注册一条（一方法可挂多个名字）；
- 没有该特性：**方法名本身就是脚本名**；
- 重复脚本名在同一类型内 `dict.Add` 抛异常。

因此写处理器时有两条硬约束：

1. **脚本方法必须无参、返回 `Task`**——`PluginManager` 用 `Method.Invoke(instance, null)` 调用，
   再 `(Task)` 强转后 await（`:485`、`:550`）。返回 `null` 或非 `Task` 会在运行期出错。
2. **辅助方法必须 `private` 或 `static`**——公开实例方法会被当成脚本名注册，污染脚本表。

脚本名来自 WZ，不是文件名：NPC `script`（为空则 `n{id}`，`PluginManager.cs:523`）、
Quest `startscript`/`endscript`、Item `spec/script`、Portal 节点 `script`、Map
`onUserEnter`/`onFirstUserEnter`、Reactor `action`（为空则 `r{id}`，`:739`）。

### 5.3 推荐的文件组织：一个功能一个目录

仓库现行约定（详见 `docs/CHANGES.MD` 与历史提交）：

```
Features/<FeatureName>/
├── <FeatureName>Feature.cs        // static class，集中常量：EventName、地图 ID、道具 ID…
├── <FeatureName>.Event.cs         // 事件模板（可选）
├── <FeatureName>.NpcHandler.cs    // : NpcScriptBase
├── <FeatureName>.PortalHandler.cs // : PortalPlayerInteraction
├── <FeatureName>.MapHandler.cs    // : MapScriptMethods
└── <FeatureName>.ReactorActHandler.cs
```

要点：

- 同一功能的事件、NPC、传送门、地图、反应堆脚本放在同一目录，靠**编译期常量**互相引用，
  而不是靠字符串（`KerningPQFeature.EventName`）。
- 处理器类型命名 `<Feature><Kind>Handler`，或对小型功能用扁平类名（如 `AmoriaPQPortalHandler`）。
- 不要再往 `NpcScript.cs` / `PortalScript.cs` / `MapScript.cs` / `Reactor/` 这些旧式 partial 大类里加代码。
- 每个功能只有一个顶层类型/文件（符合仓库「一文件一类型」约定）。

### 5.4 入口类声明脚本服务

脚本插件入口只需实现对应的空标记接口，**不需要**在代码里登记任何脚本表：

```csharp
internal class ScriptService : PluginServiceBase,
    IScriptNpcService, IScriptQuestService, IScriptItemService,
    IScriptPortalService, IScriptMapService, IScriptReactorService,
    IScriptEventService, IPluginMapService
{
    public ScriptService(PluginContainer<PluginServiceBase> c) : base(c) { }

    public override PluginPriority Priority => PluginPriority.Low;  // 内置脚本垫底，可被第三方覆盖
    // OnMounted / DisposeAsync 里做「非脚本」的挂载动作（区域 Boss、可点击 NPC 等）
}
```

脚本的收集完全由 `ScriptFeatureDiscovery.CollectHandlers(assembly, source)`（`:46`）扫描整个程序集完成，
插件因此不持有任何注册表，热更新时也没有需要清理的缓存。

### 5.5 占位实现

WZ 声明了但尚未实现的脚本名，可在 `XxxMissing.cs` 里注册占位方法抛 `NotImplementedException`
（见 `src/Application.Plugin.Script/ItemScriptMissing.cs`）。NPC/Quest 对话会捕获并回一句
「未实现」提示（`PluginManager.cs:574,659`），其他种类只记错误日志。

---

## 6. 事件模板

事件模板（`AbstractEventTemplate` 的子类）与脚本一样由插件提供，但**不是**方法级注册：

- `ScriptFeatureDiscovery.CreateEventTemplates(assembly)`（`:76`）扫描程序集里所有非抽象模板，
  用 `Activator.CreateInstance(type, nonPublic: true)` 实例化——因此模板需要**无参构造**（可非 public）。
- `PluginManager.ResolveEventTemplates()`（`:211`）合并所有插件；**事件名跨插件重名直接抛
  `BusinessFatalException`**（`:226`）。事件名是对外契约（存档、`getEventManager(name)` 都按它查找），
  静默覆盖会让另一个功能悄悄失效，所以这里不做优先级裁决。
- 合并结果通过 `EventScriptManager.SyncEventTemplates`（`EventScriptManager.cs:86`）下发：
  新增/替换模板，并移除本次不存在的模板；正在运行的副本不受影响。

```csharp
internal sealed class Event : AbstractPartyQuestEventTemplate
{
    public Event() : base(KerningPQFeature.EventName)   // 事件名
    {
        MinCount = 3; MaxCount = 6; MinLevel = 21; MaxLevel = 30;
        EntryMap = KerningPQFeature.Stage1Map;
        // …奖励、时限等
    }
}
```

模板实例在所有频道间共享，插件卸载时随 `SyncEventTemplates` 一起移除。

---

## 7. 监听类插件（Broadcast）

不提供脚本、只响应事件的插件实现 `IPlugin*Service`：

```csharp
internal class FakerService : PluginServiceBase, IPluginMapObjectService
{
    public Task OnMapObjectEnterField(IMap map, IMapObject obj) { … }
    public Task OnMapObjectLeaveField(IMap map, IMapObject obj) { … }
}
```

`PluginManager` 在对应时机依次调用**全部**实现（`PluginManager.cs:754-808`）：

- `IPluginMobService`：`OnMobSpawned` / `OnMobDamaged` / `OnMobHealed` / `OnMobKilled`
- `IPluginMapObjectService`：`OnMapObjectEnterField` / `OnMapObjectLeaveField`
- `IPluginMapService`：`OnMapLoad` / `OnMapUnload`

每个实现独立 try/catch，异常只记日志（`(cd, e) => cd.Logger.Error(...)`），不会打断其他插件，
也不会向游戏逻辑冒泡。这类接口不能返回「谁处理了」，因为所有实现都会执行。

---

## 8. 优先级与裁决

`PluginPriority` 只在两处生效：

1. **脚本合并**（`RebuildScripts`，`:243`）：按 `(Priority, PluginName)` 排序后逐个登记；
   脚本名已存在则跳过，并为被压制者记一条 warning（同一 `(插件, 种类:脚本名)` 只记一次，`:268`）。
   若被 suppress 的插件随后被重载，告警集合不会重置到进程生命周期结束。
2. **事件模板合并**：只影响遍历顺序；重名直接报错。

`FirstMatch` 的实际含义是「脚本名唯一胜者」，而不是「依次尝试直到成功」——**没有回退**。
因此第三方插件覆盖内置脚本时，必须完整实现该脚本名对应的行为。

---

## 9. 脚本执行链路

以 NPC 为例（`PluginManager.StartNpcConversation`，`:515`）：

1. 检查对话锁 / 点击频率，默认脚本名 `n{npcId}`；
2. `FindScript(ScriptKind.Npc, name)` 查生效表，未命中则显示 NPC 默认对话；
3. `DynamicObjectFactory.Create<NpcScriptBase>(handlerType, c, npcId, npcObj)` 构造**上下文对象**
   （编译期缓存构造表达式，缓存以类型为弱键，`DynamicObjectFactory.cs`）；
4. `using` 取得 `script.Source.Container.Tracker.EnterRequest()` 租约；
5. `Method.Invoke(talk, null)` 并 await；
6. 异常分派：
   - `ConversationInterruptException` → 正常结束；
   - `ConversationDiffInstanceException` / `ConversationDiffMapException` → 把玩家带回，记 warning；
   - `NotImplementedException` → 提示「未实现」；
   - `BusinessException` → 以 `Debug` 频道消息显示异常文本；
   - 其他 → 记错误日志，不向网络层冒泡。

其余种类走同构的路径：`ProcessQuestConversation`（`:601`）、`EnterPortal`（`:678`）、
`ItemScript`（`:689`）、`RunMapScript`（`:714`）、`RunReactorScript`（`:736`）。它们统一通过
`ExecuteScript`（`:475`）执行并持有租约。

### 并发与热更新

- 生效脚本表 `_scripts` 是 `volatile` 快照，加载/卸载时**整体替换**（`:281`）。
  正在执行的调用持有旧快照里的描述符，因此热更新不会打断在途脚本。
- `RequestTracker` 让卸载等待在途脚本结束：`EnterRequest()` 之后拒绝新请求，
  `DisposeAsync` 最多等 30 秒（超时后强制放行，避免永久阻塞）。
- `DynamicObjectFactory` 的缓存以 `Type` 为弱键（`ConditionalWeakTable`），
  不会阻止可回收 ALC 卸载。

> 实践上：插件 `DisposeAsync` 里应主动撤销自己在宿主注册的东西——GM 命令
> （`CommandExecutor.TryRemoveCommand`）、可点击 NPC（`RemoveScriptableNpc`）、定时任务、
> 地图上的实体等。宿主不会替插件做这些。

---

## 10. 开发一个新插件

### 10.1 建项目

```xml
<!-- 不要被宿主 ProjectReference；依赖在运行期由默认 ALC 解析 -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Application.Plugin.MyFeature</AssemblyName> <!-- 必须匹配 Application.Plugin.* -->
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Application.Core\Application.Core.csproj" />
  </ItemGroup>
  <Target Name="CopyToPlugins" AfterTargets="Build">
    <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll"
          DestinationFolder="$(MSBuildProjectDirectory)\..\Application.Host\bin\$(Configuration)\$(TargetFramework)\Plugins\" />
  </Target>
</Project>
```

编译期需要引用 `Application.Core`（脚本基类都在那里），**运行期不需要把 `Application.Core.dll`
一起放进 `Plugins/`**：`PluginLoadContext.Load` 先尝试从宿主默认上下文解析（`PluginLoadContext.cs:19-33`），
宿主已有 `Application.Core` 就复用同一份类型标识，插件里的类型才能与宿主类型互通。

> 私有依赖的现状与限制：卷影副本只复制插件自身的 DLL（`PluginManager.cs:167`），
> 依赖解析 `AssemblyDependencyResolver` 指向卷影目录，而 `.deps.json` / 私有依赖不会一并复制。
> 因此**使用非宿主自带第三方库的插件，目前无法直接加载**。需要的库要么已由宿主引用，
> 要么放进宿主输出目录。

### 10.2 写入口与脚本

```csharp
// MyFeatureService.cs —— 每个程序集有且只有一个
internal sealed class MyFeatureService : PluginServiceBase, IScriptNpcService, IPluginMobService
{
    public MyFeatureService(PluginContainer<PluginServiceBase> container) : base(container) { }

    public override async Task OnMounted()
    {
        LogInfo("{Plugin} 已挂载", _pluginName);
        await base.OnMounted();
    }

    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void OnMobKilled(Monster mob, ICombatantObject? killer) { /* 所有插件都会收到 */ }
    public void OnMobSpawned(Monster mob) { }
    public void OnMobHealed(Monster mob, int value) { }
    public void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker) { }
}
```

```csharp
// Features/MyFeature/MyFeatureFeature.cs
internal static class MyFeatureFeature
{
    internal const string EventName = "MyFeature";
    internal const int EntryNpc = 9000000;
}

// Features/MyFeature/MyFeature.NpcHandler.cs
internal sealed class MyFeatureNpcHandler : NpcScriptBase
{
    public MyFeatureNpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj) { }

    // 方法名 = WZ 中该 NPC 的 script 名
    public async Task my_feature_intro()
    {
        await SayOK("Hello");
    }

    [ScriptName("alias_a", "alias_b")]   // 一个方法挂多个脚本名
    public async Task Alias()
    {
        await SayOK("alias");
    }

    private Task Helper() => Task.CompletedTask;   // 私有方法不会被注册
}
```

### 10.3 构建与运行

```bash
dotnet build "./src/Application.Plugin.MyFeature/Application.Plugin.MyFeature.csproj"
# 或者一次编译解决方案：dotnet build RuaMS.slnx
```

构建后确认 DLL 出现在宿主的 `Plugins/` 下，启动服务器；放/换/删 DLL 即可触发热更新。
多进程模式还需手工复制到 `Application.Host.Channel` 的输出目录。

### 10.4 验证

- 单测：`test/ServiceTest/Games/Gameplay/PluginManagerScriptTests.cs` 覆盖「加载 → 注册 → 合并」链路，
  断言每个 `ScriptKind` 的生效表非空、与程序集扫描结果一致、每条描述符都带来源插件。
- WZ 一致性：`test/ServiceTest/Infrastructure/WZ/ScriptRegistrationCheck.cs` 用基线文件
  `script-registration-baseline.txt` 校验 WZ 中声明的脚本名都有实现；
  新增/修复脚本导致基线变化时需要更新基线。
- 日志：`logs/Plugin/<程序集名>/`；被压制脚本会输出
  「脚本 {Kind}:{Script} 已由更高优先级的插件 {Winner} 提供」。

---

## 11. 常见坑

| 现象 | 原因 |
|---|---|
| `No type implementing PluginServiceBase found` | 入口类不是 `PluginServiceBase` 的具体子类，或未编译进插件 DLL |
| `每个接口仅允许一个入口服务` | 程序集里有两个及以上入口类 |
| 脚本不生效但无任何报错 | 处理器没有**直接**继承对应脚本基类（中间插了自定义基类，`TryResolveKind` 只看一层）；或服务接口没有 `[PluginInvocation]`，导致服务表为空 |
| 脚本被别的插件「抢走」 | 同名脚本按优先级裁决；检查 `Priority`，看日志里的「已由更高优先级的插件…提供」 |
| 加载后事件报「事件重名」 | 两个插件提供了同名 `AbstractEventTemplate.Name`；重名不做优先级裁决，必须改事件名 |
| 方法注册成了奇怪的脚本名 | 公开实例辅助方法会被自动注册；改成 `private` / `static` |
| 运行期 `NullReferenceException` / 类型转换失败 | 脚本方法返回 `void` 或非 `Task`；必须无参且返回 `Task` |
| 插件重载后旧逻辑还在 | 插件在宿主注册过的东西（命令、NPC、定时任务）没有在 `DisposeAsync` 里撤销 |
| 覆盖 DLL 后没变化 | 文件名不匹配 `Application.Plugin.*.dll`，或插件在 `Application.Host.Channel` 目录下而实际跑的是另一个进程 |
| `FileNotFoundException` 加载私有依赖 | 卷影副本与依赖解析的已知限制，见 10.1 |

---

## 12. 相关文件

| 文件 | 职责 |
|---|---|
| `Gameplay/Plugins/PluginManager.cs` | 加载/卸载/热更新、脚本与事件合并、调用分发、脚本执行链路 |
| `Gameplay/Plugins/PluginContainer.cs` | 一个插件的容器：程序集、ALC、日志、请求跟踪、卷影目录 |
| `Gameplay/Plugins/PluginLoadContext.cs` | 可回收 ALC，宿主优先、插件目录兜底的程序集解析 |
| `Gameplay/Plugins/PluginServiceBase.cs` / `IPluginServiceBase.cs` | 入口基类、`PluginPriority` |
| `Gameplay/Plugins/ScriptFeatureDiscovery.cs` | 程序集扫描：脚本处理器（按基类）与事件模板 |
| `Gameplay/Plugins/ScriptNameAttribute.cs` | `[ScriptName]`、`TypeUtils.LoadFromType` |
| `Gameplay/Plugins/ScriptHandlerDescriptor.cs` / `ScriptKind.cs` | 注册项与种类枚举 |
| `Gameplay/Plugins/PluginInvocationAttribute.cs` | `FirstMatch` / `Broadcast` 及缓存 |
| `Gameplay/Plugins/RequestTracker.cs` | 请求租约与优雅排水 |
| `Gameplay/Plugins/Services/*.cs` | 各服务接口（脚本接口 + 监听接口） |
| `Channel/WorldChannelServer.cs` | 创建 `PluginManager`、目录监视、批量加载 |
| `Application.Plugin.Script/` | 内置脚本插件（默认 `Low` 优先级），`Features/` 为功能目录 |
| `Application.Plugin.FakeCharacter/` | 监听型插件示例（`IPluginMapObjectService` + GM 命令） |
| `Application.Plugin.TreasureMap/` | 小型脚本插件示例（NPC + 道具脚本 + 可点击 NPC） |
