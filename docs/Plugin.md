# 插件开发指南

> 说明 `Application.Plugin.*` 的开发方式与实现原理，依据 `src/Application.Core/Gameplay/Plugins/` 下的现有基础设施。
> 最后核对：2026-11-12，分支 `refacor/i18n`（`7ccf9e109`）。
> 多语言文本的细节见 [I18n.md](./I18n.md)。

---

## 1. 插件是什么

插件是一个**运行时加载的独立程序集**，宿主（`Application.Host` / `Application.Host.Channel`）在编译期不引用它：

- 宿主进程启动后，`WorldChannelServer` 创建 `PluginManager`，扫描程序基目录下的 `Plugins/`。
- 目录名与主程序集名都必须以 `Application.Plugin.` 开头（`PluginManager.PluginFilePrefix`）。
- 每个插件在**独立的可回收 `AssemblyLoadContext`** 中加载，卸载后程序集可被 GC 回收。
- 插件只在**频道侧**加载：`Application.Host.Channel` 与单进程模式下的 `Application.Host` 都走
  `WorldChannelServer`，Master 侧不加载插件。

与 `modules/` 的区别：模块是编译期引用、通过 DI 注册的普通类库；插件是运行期加载、可热更新的独立程序集。
需要热更新、或希望第三方扩展而不改宿主代码时选插件。

---

## 2. 运行期目录

**一个插件 = 一个目录**，DLL、私有依赖、自带文案都放在同一个目录里：

```
Plugins/
└── Application.Plugin.TreasureMap/
    ├── Application.Plugin.TreasureMap.dll
    ├── Application.Plugin.TreasureMap.deps.json   （可选，私有依赖时用）
    ├── SomePrivateLib.dll                          （可选，插件自带的第三方库）
    └── i18n/
        └── TreasureMapText/
            ├── zh-CN.json
            └── en-US.json
```

| 目录 | 位置 | 作用 |
|---|---|---|
| `Plugins/<插件名>/` | `AppDomain.CurrentDomain.BaseDirectory/Plugins` | 运维放置插件的**源目录**；每个子目录是一个插件整体 |
| `PluginShadowCopy/<guid>/<插件名>/` | 程序基目录 | 每次加载时把插件目录**顶层文件**整体复制一份，从副本加载；卸载时删除整个 `<guid>/` 目录 |

`BaseDirectory` 是程序集所在目录，不是仓库根目录：

- `dotnet run` / 单进程：`src/Application.Host/bin/<Configuration>/<TargetFramework>/Plugins/`
- 多进程：频道服务器进程自己目录下的 `Plugins/`

卷影副本的意义：Windows 上正在使用的 DLL 会被锁定、不能覆盖；复制到新目录后再加载，运维才能直接覆盖
`Plugins/` 下的原文件完成更新。`{guid}` 这一层保证热重载时旧副本不会被新副本覆盖。

> 说明：
> - 卷影复制是**非递归**的，只复制插件目录顶层的文件。`i18n/` 不会被复制，但文案读取的是**源目录**
>   （见 §8），所以不影响使用；`i18n/` 之外的子目录不会被复制，私有依赖请放在插件目录顶层。
> - 因为整个目录连同 `deps.json` 一起复制，`AssemblyDependencyResolver` 能在卷影目录里解析到插件自己的依赖，
>   插件可以携带宿主没有的第三方库。

插件项目自带 `CopyToPlugins` 目标，`dotnet build` 后会按上述布局复制到 `Application.Host` 的输出目录。
**它不会复制到 `Application.Host.Channel` 的输出目录**——跑多进程模式时需要手动放置。

---

## 3. 加载 / 卸载流程

### 3.1 加载

`PluginManager.LoadPlugin(pluginDirectory)` 的参数是**插件目录名**（相对 `Plugins/`）或绝对路径。
`LoadPluginInternal` / `LoadPluginFromSource` 依次做：

1. **定位源目录**：目录不存在抛 `DirectoryNotFoundException`；目录里缺少 `<插件名>.dll` 抛 `FileNotFoundException`。
2. **复制到卷影目录**：把源目录顶层文件复制到 `PluginShadowCopy/<guid>/<插件名>/`。
3. **创建 `PluginLoadContext`**（`isCollectible: true`），从副本加载主程序集。
4. **查找入口类型**：程序集里必须有**且只能有一个**非抽象、实现 `PluginServiceBase` 的类型，
   为零或为多都抛 `InvalidOperationException`。
5. **构造入口实例**：`Activator.CreateInstance(入口类型, container)`，构造参数是
   `PluginContainer<PluginServiceBase>`。
6. **同名插件先卸载**：`RemovePluginInternal(pluginKey)`，因此重复 `LoadPlugin` 就是「热替换」。
7. **重建注册表**：
   - `RebuildRegistrations()`——按优先级排好「服务接口实现」列表；
   - `RebuildScripts()`——重扫所有插件程序集，合并成生效脚本表；
   - `SyncEventTemplatesAsync()`——把合并后的事件模板下发到各频道。
8. **`OnMounted()`**：失败只记日志，不影响插件已注册的内容。
9. 第 7 步任何一步抛异常（例如事件重名）：把插件从容器表里摘掉、重建注册表、卸载上下文并抛给调用方，
   不会留下半加载状态。

装卸操作由 `PluginManager` 内部的 `SemaphoreSlim` 串行化：文件监听与手工调用可能同时触发同一次重载，
读取目录、创建容器、重建注册表必须作为一个整体执行。

### 3.2 卸载

`UnloadPlugin` → `RemovePluginInternal`，顺序固定：

1. 从容器表移除；
2. `RebuildRegistrations()` + `RebuildScripts()` + `SyncEventTemplatesAsync()` —— **先摘注册再卸 ALC**，
   因为事件管理器持有插件类型，必须在程序集卸载前释放；
3. `PluginContainer.DisposeAsync()`：
   - `RequestTracker.DisposeAsync()` 排水（等待在途脚本结束，默认最多 30 秒）；
   - 入口服务 `DisposeAsync()`（插件自己的清理逻辑）；
   - `MessageStore.RemoveDirectory(...)` 摘掉插件文案目录；
   - `LoadContext.Unload()`；
   - 删除整份卷影副本目录。

`PluginManager.DisposeAsync` 同样持有该锁，逐个卸载剩余插件后释放锁。

### 3.3 热更新

`WorldChannelServer` 在频道注册完成后启动 `FileSystemWatcher`，监视 `Plugins/` 下匹配
`Application.Plugin.*.dll` 的文件，并开启 `IncludeSubdirectories`（插件在子目录里）：

| 事件 | 处理 |
|---|---|
| `Changed` / `Created` | 1 秒内只处理一次，再延迟 200ms 等文件写完，然后 `LoadPlugin(文件名去后缀)` |
| `Deleted` | `UnloadPlugin(文件名去后缀)` |

事件回调里的路径是相对 `Plugins/` 的（含子目录），因此判定与取名都用 `Path.GetFileName`：
插件名 = 目录名 = 主程序集名。

也就是说：**替换插件目录里的主 DLL = 重载该插件，删除 = 卸载**。

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

`PluginServiceBase` 提供：

| 成员 | 说明 |
|---|---|
| `Priority` | `High = 0 / Normal = 1 / Low = 2`，数值越小优先级越高，默认 `Normal`。`Application.Plugin.Script` 用 `Low`，所以第三方插件用默认值即可覆盖内置脚本 |
| `Container` | `PluginContainer`：`Logger`、`Node`（`WorldChannelServer`）、`Tracker`、`PluginAssembly`、`MessageDirectory` |
| `OnMounted()` | 加载完成后调用 |
| `DisposeAsync()` | 卸载前调用，必须实现 |
| `_node` / `_pluginName` / `LogInfo` / `LogWarning` / `LogError` | 便捷字段与方法；日志分类为 `Plugin/<插件名>` |

构造参数只有 `container`，**没有 DI 注入**。需要宿主服务时自行取：

```csharp
var executor = _node.ServiceProvider.GetRequiredService<CommandExecutor>();
```

（`FakerService` 即按此方式注册与注销 GM 命令。）

### 4.2 服务接口声明「调用模式」

`[PluginInvocation(mode)]` 标在**接口**上，决定该接口的实现如何被调用：

| 模式 | 语义 | 现有接口 |
|---|---|---|
| `FirstMatch` | 同名请求由优先级最高的实现处理（用于脚本注册类接口） | `IScriptNpcService` / `IScriptQuestService` / `IScriptItemService` / `IScriptPortalService` / `IScriptMapService` / `IScriptReactorService` / `IScriptEventService` |
| `Broadcast` | 每个事件通知**所有**实现（用于监听类接口） | `IPluginMobService` / `IPluginMapService` / `IPluginMapObjectService` |

`PluginManager` 在调用前用 `RequireMode` 校验调用方式与标注一致，用错直接抛异常。
一个入口类可以同时实现多个接口（如 `ScriptService` 同时实现 7 个脚本接口 + `IPluginMapService`）；
`RebuildRegistrations` 保证**每个插件在每个模式列表里只出现一次**，避免同一插件的脚本被算两遍。

> 新增服务接口时必须标注 `[PluginInvocation]`，否则服务列表里不会包含它——调用会静默变成空操作。
> `PluginManagerScriptTests` 专门守住这类静默失败。

---

## 5. 脚本插件：声明方式

### 5.1 种类由基类决定

`ScriptFeatureDiscovery` 只认**直接基类**：

| 基类 | `ScriptKind` | 构造参数 | 触发点 |
|---|---|---|---|
| `NpcScriptBase` | `Npc` | `(IChannelClient, int npc, NPC?)` | 点击 NPC |
| `QuestScriptBase` | `Quest` | `(IChannelClient, Quest, int npc)` | 任务开始/结束对话 |
| `ItemScriptBase` | `Item` | `(IChannelClient, Item, int npcId)` | 道具脚本 |
| `PortalPlayerInteraction` | `Portal` | `(IChannelClient, IPortal)` | 进传送门 |
| `MapScriptMethods` | `Map` | `(IChannelClient, IMap)` | `onUserEnter` / `onFirstUserEnter` |
| `ReactorHitScriptBase` 等 4 个 | `ReactorHit/Act/Touch/Untouch` | `(IChannelClient, Reactor)` | 反应堆 |

要求：**直接继承**上表基类（`TryResolveKind` 只看一层 `BaseType`），中间不要再插自定义基类。
本仓库 165 个处理器全部满足这一点。

### 5.2 方法名 = 脚本名

`TypeUtils.LoadFromType` 扫描类型**自己声明的公开实例方法**：

- 有 `[ScriptName(...)]`：每个名字注册一条（一方法可挂多个名字）；
- 没有该特性：**方法名本身就是脚本名**；
- 同一类型内重复脚本名会抛异常。

因此写处理器时有两条硬约束：

1. **脚本方法必须无参、返回 `Task`**——`PluginManager` 用 `Method.Invoke(instance, null)` 调用，
   再强转成 `Task` await。返回 `null` 或非 `Task` 会在运行期出错。
2. **辅助方法必须 `private` 或 `static`**——公开实例方法会被当成脚本名注册，污染脚本表。

> `[ScriptTag(...)]` 目前只是标注（用于标记功能归属），不参与注册与分发。

脚本名来自 WZ，不是文件名：NPC `script`（为空则 `n{id}`）、Quest `startscript`/`endscript`、
Item `spec/script`、Portal 节点 `script`、Map `onUserEnter`/`onFirstUserEnter`、
Reactor `action`（为空则 `r{id}`）。

### 5.3 推荐的文件组织：一个功能一个目录

仓库现行约定（另见 `docs/CHANGES.MD`）：

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
- `Features/` 目前有 43 个功能目录，与新目录并存的是少量历史遗留的 partial 大类。

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

脚本的收集完全由 `ScriptFeatureDiscovery.CollectHandlers(assembly, source)` 扫描整个程序集完成，
插件因此不持有任何注册表，热更新时也没有需要清理的缓存。

### 5.5 占位实现

WZ 声明了但尚未实现的脚本名，可在 `XxxMissing.cs` 里注册占位方法抛 `NotImplementedException`
（见 `src/Application.Plugin.Script/ItemScriptMissing.cs`）。NPC/Quest 对话会捕获并回一句
「未实现」提示，其他种类只记错误日志。

---

## 6. 事件模板

事件模板（`AbstractEventTemplate` 的子类）与脚本一样由插件提供，但**不是**方法级注册：

- `ScriptFeatureDiscovery.CreateEventTemplates(assembly)` 扫描程序集里所有非抽象模板，
  用 `Activator.CreateInstance(type, nonPublic: true)` 实例化——因此模板需要**无参构造**（可非 public）。
- `PluginManager.ResolveEventTemplates()` 合并所有插件；**事件名跨插件重名直接抛
  `BusinessFatalException`**。事件名是对外契约（存档、`getEventManager(name)` 都按它查找），
  静默覆盖会让另一个功能悄悄失效，所以这里不做优先级裁决。
- 合并结果通过 `EventScriptManager.SyncEventTemplates` 下发：新增/替换模板，并移除本次不存在的模板；
  正在运行的副本不受影响。

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

`PluginManager` 在对应时机依次调用**全部**实现：

- `IPluginMobService`：`OnMobSpawned` / `OnMobDamaged` / `OnMobHealed` / `OnMobKilled`
- `IPluginMapObjectService`：`OnMapObjectEnterField` / `OnMapObjectLeaveField`
- `IPluginMapService`：`OnMapLoad` / `OnMapUnload`

每个实现独立 try/catch，异常只记日志，不会打断其他插件，也不会向游戏逻辑冒泡。
这类接口不能返回「谁处理了」，因为所有实现都会执行。

---

## 8. 插件自带的文案（i18n）

插件目录里的 `i18n/` 会被自动注册到宿主的多语言存储，插件卸载时自动摘除：

- `PluginContainer` 构造时解析 `<插件源目录>/i18n`，非空则 `MessageStore.AddDirectory(...)`；
  卸载时 `MessageStore.RemoveDirectory(...)`。整个过程不需要插件写任何代码。
- 目录约定与宿主一致：**类别目录名 = 生成的常量类名**，**文件名 = `CultureInfo.Name`**
  （`i18n/ScriptMessage/zh-CN.json`、`en-US.json`）。
- 类别名是插件自己的概念，Core 不认识它：取文案统一走
  `IClientCulture.GetMessageByCategory(category, key, culture)`。
- 查找顺序：宿主根目录优先，插件目录按注册顺序在后，**首个命中的文件生效**；同一类别被两个来源提供时，
  后注册的会打一条 warning 且不生效。
- `MessageStore` 对插件目录按**引用计数**管理：热重载期间新旧容器会短暂共享同一个目录，
  旧容器卸载不会摘掉新容器还在用的目录。
- 文案目录来自**源目录**而不是卷影副本，因此修改 `Plugins/<插件名>/i18n/` 下的 JSON 会被
  `MessageStore` 的监听器热加载（不需要重载插件）。

在脚本里取文案：

```csharp
// Application.Plugin.Script 自带的扩展方法，固定使用 ScriptMessage 类别
await SayOK(this.GetTalkMessage(nameof(ScriptMessage.Redeem_InventoryFull)));

// 通用写法（任何类别）
await SayOK(c.CurrentCulture.GetMessageByCategory("MyCategory", "MyKey", c.CurrentCulture.CultureInfo));
```

> `GetTalkMessage` 是插件侧的扩展方法（挂在 `NPCConversationManager` 上），必须带接收者调用；
> 它不属于 Core，其他插件要自己定义或直接用 `GetMessageByCategory`。

编译期需要使用 `tools/Message.SourceGenerator` 生成常量类：在插件项目里引入
`MessageSource.props` 与源生成器引用，并在 `CopyToPlugins` 里把 `i18n/**/*.json` 复制到
`Plugins/<插件名>/i18n/`（照 `Application.Plugin.TreasureMap.csproj` 抄）。

---

## 9. 优先级与裁决

`PluginPriority` 只在两处生效：

1. **脚本合并**（`RebuildScripts`）：按 `(Priority, PluginName)` 排序后逐个登记；
   脚本名已存在则跳过，并为被压制者记一条 warning（同一 `(插件, 种类:脚本名)` 只记一次）。
   告警集合在进程生命周期内不会重置。
2. **事件模板合并**：只影响遍历顺序；重名直接报错。

`FirstMatch` 的实际含义是「脚本名唯一胜者」，而不是「依次尝试直到成功」——**没有回退**。
因此第三方插件覆盖内置脚本时，必须完整实现该脚本名对应的行为。

---

## 10. 脚本执行链路

以 NPC 为例（`PluginManager.StartNpcConversation`）：

1. 检查对话锁 / 点击频率，默认脚本名 `n{npcId}`；
2. `FindScript(ScriptKind.Npc, name)` 查生效表，未命中则显示 NPC 默认对话；
3. `DynamicObjectFactory.Create<NpcScriptBase>(handlerType, c, npcId, npcObj)` 构造**上下文对象**
   （编译期缓存构造表达式，缓存以类型为弱键，`DynamicObjectFactory`）；
4. `using` 取得 `script.Source.Container.Tracker.EnterRequest()` 租约；
5. `Method.Invoke(talk, null)` 并 await；
6. 异常分派：
   - `ConversationInterruptException` → 正常结束；
   - `ConversationDiffInstanceException` / `ConversationDiffMapException` → 把玩家带回，记 warning；
   - `NotImplementedException` → 提示「未实现」；
   - `BusinessException` → 以 `Debug` 频道消息显示异常文本；
   - 其他 → 记错误日志，不向网络层冒泡。

其余种类走同构的路径：`ProcessQuestConversation`、`EnterPortal`、`ItemScript`、`RunMapScript`、
`RunReactorScript`。它们统一通过 `ExecuteScript` 执行并持有租约。

### 并发与热更新

- 生效脚本表是 `volatile` 快照，加载/卸载时**整体替换**。正在执行的调用持有旧快照里的描述符，
  因此热更新不会打断在途脚本。
- `RequestTracker` 让卸载等待在途脚本结束：`EnterRequest()` 之后拒绝新请求，
  `DisposeAsync` 最多等 30 秒（超时后强制放行，避免永久阻塞）。
- `DynamicObjectFactory` 的缓存以 `Type` 为弱键（`ConditionalWeakTable`），不会阻止可回收 ALC 卸载。

> 实践上：插件 `DisposeAsync` 里应主动撤销自己在宿主注册的东西——GM 命令
> （`CommandExecutor.TryRemoveCommand`）、可点击 NPC（`RemoveScriptableNpc`）、定时任务、
> 地图上的实体等。宿主不会替插件做这些。

---

## 11. 开发一个新插件

### 11.1 建项目

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
    <!-- 需要自带文案时再加源生成器 -->
    <ProjectReference Include="..\..\tools\Message.SourceGenerator\Message.SourceGenerator.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <!-- 需要自带文案时：i18n/{Category}/{Culture}.json -> 生成常量类 -->
  <Import Project="..\..\tools\Message.SourceGenerator\MessageSource.props" />

  <!-- 一个插件一个目录：Plugins/{插件名}/{插件名}.dll + 同级 i18n/ -->
  <Target Name="CopyToPlugins" AfterTargets="Build">
    <PropertyGroup>
      <_PluginTargetDir>$(MSBuildProjectDirectory)\..\Application.Host\bin\$(Configuration)\$(TargetFramework)\Plugins\$(AssemblyName)\</_PluginTargetDir>
    </PropertyGroup>
    <ItemGroup>
      <_PluginMessageFile Include="$(MSBuildProjectDirectory)\i18n\**\*.json" />
    </ItemGroup>
    <Copy SourceFiles="$(OutputPath)$(AssemblyName).dll"
          DestinationFolder="$(_PluginTargetDir)"
          SkipUnchangedFiles="true" />
    <Copy SourceFiles="@(_PluginMessageFile)"
          DestinationFiles="@(_PluginMessageFile->'$(_PluginTargetDir)i18n\%(RecursiveDir)%(Filename)%(Extension)')"
          SkipUnchangedFiles="true" />
  </Target>
</Project>
```

编译期需要引用 `Application.Core`（脚本基类都在那里），**运行期不需要把 `Application.Core.dll`
一起放进 `Plugins/`**：`PluginLoadContext.Load` 先尝试从宿主默认上下文解析，宿主已有 `Application.Core`
就复用同一份类型标识，插件里的类型才能与宿主类型互通。

插件**私有**的第三方库（宿主没有的）请放在插件目录顶层；卷影复制会把顶层文件连同 `deps.json`
一起带到副本目录，依赖解析因此能找到它们。

### 11.2 写入口与脚本

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

### 11.3 构建与运行

```bash
dotnet build "./src/Application.Plugin.MyFeature/Application.Plugin.MyFeature.csproj"
# 或者一次编译解决方案：dotnet build RuaMS.slnx
```

构建后确认宿主的 `Plugins/Application.Plugin.MyFeature/` 下出现主 DLL（以及 `i18n/`），启动服务器；
替换主 DLL 或删除插件目录即可触发热更新。多进程模式还需手工复制整个目录到
`Application.Host.Channel` 的输出目录。

### 11.4 验证

- 插件链路：`test/ServiceTest/Games/Gameplay/PluginManagerScriptTests.cs` 覆盖「加载 → 注册 → 合并」，
  断言每个 `ScriptKind` 的生效表非空、与程序集扫描结果一致、每条描述符都带来源插件。
- 扫描层：`ScriptFeatureDiscoveryTests.cs` 校验处理器与事件模板能被发现，并用
  `event-template-names.txt` 作为事件名的基线快照。
- 文案：`PluginMessageDirectoryTests.cs` 校验插件目录里的 `i18n/` 在加载/卸载时正确注册与摘除。
- WZ 一致性：`test/ServiceTest/Infrastructure/WZ/ScriptRegistrationCheck.cs` 用基线文件
  `script-registration-baseline.txt` 校验 WZ 中声明的脚本名都有实现；新增/修复脚本时需更新基线。
- 日志：`logs/Plugin/<插件名>/`；被压制脚本会输出
  「脚本 {Kind}:{Script} 已由更高优先级的插件 {Winner} 提供」。

---

## 12. 常见坑

| 现象 | 原因 |
|---|---|
| 加载时提示 `Plugin directory not found` | `LoadPlugin` 的参数是**目录名**（`Plugins/<插件名>/`），不是 DLL 文件名 |
| 加载时提示 `No type implementing PluginServiceBase found` | 入口类不是 `PluginServiceBase` 的具体子类，或未编译进插件 DLL |
| `每个接口仅允许一个入口服务` | 程序集里有两个及以上入口类 |
| 目录被忽略、完全不加载 | 目录名或 DLL 名不以 `Application.Plugin.` 开头，或目录里没有 `<目录名>.dll` |
| 脚本不生效但无任何报错 | 处理器没有**直接**继承对应脚本基类（中间插了自定义基类）；或服务接口没有 `[PluginInvocation]` |
| 脚本被别的插件「抢走」 | 同名脚本按优先级裁决；检查 `Priority`，看日志里的「已由更高优先级的插件…提供」 |
| 加载后事件报「事件重名」 | 两个插件提供了同名 `AbstractEventTemplate.Name`；重名不做优先级裁决，必须改事件名 |
| 方法注册成了奇怪的脚本名 | 公开实例辅助方法会被自动注册；改成 `private` / `static` |
| 运行期 `NullReferenceException` / 类型转换失败 | 脚本方法返回 `void` 或非 `Task`；必须无参且返回 `Task` |
| 插件重载后旧逻辑还在 | 插件在宿主注册过的东西（命令、NPC、定时任务）没有在 `DisposeAsync` 里撤销 |
| 替换 DLL 后没变化 | 改的不是 `<插件名>.dll`，或插件放在 `Application.Host.Channel` 目录下而实际跑的是另一个进程 |
| 文案全部显示原始键名 | 插件目录里没有 `i18n/`、类别目录名与代码里的类别字符串不一致，或宿主没调用 `MessageStore.Initialize()` |
| 文案是旧值 | 文件在卷影副本的旧路径改的；要改**源目录** `Plugins/<插件名>/i18n/` 下的 JSON |
| 私有依赖 `FileNotFoundException` | 依赖不在插件目录顶层；卷影复制不递归，`i18n/` 以外的子目录会被丢弃 |
| 容器镜像里插件不生效 | 当前 `src/Application.Host/Dockerfile*` 仍按旧的扁平布局只复制单个 DLL，需要改成复制整个插件目录（`/app/Plugins/<插件名>/`） |

---

## 13. 相关文件

| 文件 | 职责 |
|---|---|
| `Gameplay/Plugins/PluginManager.cs` | 目录枚举、加载/卸载/热更新、脚本与事件合并、调用分发、脚本执行链路 |
| `Gameplay/Plugins/PluginContainer.cs` | 一个插件的容器：程序集、ALC、日志、请求跟踪、卷影目录、文案目录 |
| `Gameplay/Plugins/PluginLoadContext.cs` | 可回收 ALC，宿主优先、插件目录兜底的程序集解析 |
| `Gameplay/Plugins/PluginServiceBase.cs` / `IPluginServiceBase.cs` | 入口基类、`PluginPriority` |
| `Gameplay/Plugins/ScriptFeatureDiscovery.cs` | 程序集扫描：脚本处理器（按基类）与事件模板 |
| `Gameplay/Plugins/ScriptNameAttribute.cs` | `[ScriptName]` / `[ScriptTag]`、`TypeUtils.LoadFromType` |
| `Gameplay/Plugins/ScriptHandlerDescriptor.cs` / `ScriptKind.cs` | 注册项与种类枚举 |
| `Gameplay/Plugins/PluginInvocationAttribute.cs` | `FirstMatch` / `Broadcast` 及缓存 |
| `Gameplay/Plugins/RequestTracker.cs` | 请求租约与优雅排水 |
| `Gameplay/Plugins/Services/*.cs` | 各服务接口（脚本接口 + 监听接口） |
| `Channel/WorldChannelServer.cs` | 创建 `PluginManager`、递归目录监视、批量枚举加载 |
| `Application.Resources/i18n/MessageStore.cs` | 多语言存储：宿主根目录 + 插件目录（引用计数） |
| `tools/Message.SourceGenerator/` | 由 `i18n/**/*.json` 生成消息键常量类 |
| `Application.Plugin.Script/` | 内置脚本插件（默认 `Low` 优先级），`Features/` 为功能目录，`ScriptMessageExtensions` 提供 `GetTalkMessage` |
| `Application.Plugin.FakeCharacter/` | 监听型插件示例（`IPluginMapObjectService` + GM 命令） |
| `Application.Plugin.TreasureMap/` | 小型脚本插件示例（NPC + 道具脚本 + 可点击 NPC + 自带 `i18n/`） |
