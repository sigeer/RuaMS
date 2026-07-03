# 项目介绍

![.NET](https://img.shields.io/badge/net10.0-512bd4?logo=.NET&labelColor=gray)
[![Build Check](https://github.com/sigeer/RuaMS/actions/workflows/build.yml/badge.svg)](https://github.com/sigeer/RuaMS/actions/workflows/build.yml)
[![License](https://img.shields.io/github/license/sigeer/RuaMS)](https://github.com/sigeer/RuaMS/blob/master/LICENSE)

基于 [Cosmic](https://github.com/P0nk/Cosmic) 的 C# 移植版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)

## 注意 ❗

大量内容未经测试，可能出现大量 BUG，不建议搭建游玩。

## 开发环境

- .NET 10
- MySQL 8（可选）

## 构建与测试

```bash
# 构建主服务器（单进程模式）
dotnet build "./src/Application.Host/Application.Host.csproj"

# 运行（单进程，Master + Channel 一体）
dotnet run --project "./src/Application.Host/Application.Host.csproj"

# 运行基础设施测试（无需启动服务器）
dotnet test --filter "FullyQualifiedName~ServiceTest.Infrastructure" ./test/ServiceTest/ServiceTest.csproj

# 运行游戏/服务器测试（会启动本地测试服务器）
dotnet test --filter "FullyQualifiedName~ServiceTest.Games" ./test/ServiceTest/ServiceTest.csproj

# 运行所有测试
dotnet test ./test/ServiceTest/ServiceTest.csproj

# 运行单个测试
dotnet test --filter "FullyQualifiedName~ServiceTest.Infrastructure.UtilityTests" ./test/ServiceTest/ServiceTest.csproj

# 构建频道服务器（多进程模式）
dotnet build "./src/Application.Host.Channel/Application.Host.Channel.csproj"

# 构建插件项目（以 Application.Plugin 开头，支持热更新，需单独构建）
dotnet build "./src/Application.Plugin.Script/Application.Plugin.Script.csproj"
dotnet build "./src/Application.Plugin.FakeCharacter/Application.Plugin.FakeCharacter.csproj"
# 构建后将 dll 放入根目录 `Plugins` 文件夹即可自动加载
```

## 架构概述

### 部署模式

支持两种部署模式，通过 MSBuild 属性 `IsStandalone` 控制：

1. **单进程模式**（`IsStandalone=true`，默认）：单一进程同时运行 Master 和 Channel 服务器，通过 `Application.Core.Channel.InProgress` 桥接。入口：`Application.Host`。
2. **多进程模式**（`IsStandalone=false`）：分离进程通过 gRPC 通信：
   - `Application.Host` — Master 服务器（登录处理、世界管理、Admin REST API）
   - `Application.Host.Channel` — Channel 服务器（游戏逻辑，gRPC 客户端连接 Master）

### 通信方式

- **客户端 ↔ 服务器**：MapleStory 协议 over TCP，基于 DotNetty（`DotNetty.Buffers`、`.Codecs`、`.Transport`、`.Handlers`）
- **Master ↔ Channel**：gRPC + Protobuf 序列化

### 脚本引擎

- **JavaScript**：基于 Jint 引擎，运行 `scripts/` 下的遗留 JS 脚本
- **C# 脚本**：通过 `Application.Plugin.Script` 重写，支持 IDE 导航和重构（持续迁移中）
- **Lua**：基于 NLua，暂未投入使用

### 数据库

- **SQLite**（默认）
- **MySQL 8**（可选，通过 Pomelo.EntityFrameworkCore.MySql 驱动）
- 通过 `appsettings.json` 中的 `Database` 配置切换
- 环境变量使用 `RUA_MS_` 前缀可覆盖任意配置项

## 项目结构

### 解决方案目录（40+ 项目）

```
src/
├── Application.Host/                     — 主服务器入口（ASP.NET）
├── Application.Host.Channel/             — 频道服务器入口（ASP.NET）
├── Application.Core/                     — 频道核心：游戏逻辑、网络（DotNetty）、命令、世界管理
├── Application.Core.Login/               — Master 服务器：登录处理、世界/频道注册
├── Application.Core.Channel.InProgress/  — 单进程模式下进程内运行 Channel 的实现
├── Application.Core.EF/                  — EF Core 数据层抽象
│   ├── Application.Core.EF.MySQL/        — MySQL 驱动，含迁移文件和种子数据
│   └── Application.Core.EF.Sqlite/       — SQLite 驱动，含迁移文件和种子数据
├── Application.Shared/                   — 公共数据结构、类型、常量、网络包、DTO
├── Application.Utility/                  — 工具类（Serilog 日志、Quartz 调度、AutoMapper 映射、扩展方法）
├── Application.Protos/                   — gRPC/Protobuf 定义文件（约 130 个 .proto）
├── Application.Templates/                — WZ 数据模板模型（角色、道具、地图、怪物、NPC、技能等）
├── Application.Templates.XmlWzReader/    — WZ XML → 模板对象数据绑定
├── Application.Resources/                — 静态资源：wz/、scripts/、yaml 配置文件
├── XmlWzReader/                          — XML 格式 WZ 数据读取工具
├── Application.Scripting/                — 脚本引擎抽象
├── Application.Scripting.JS/             — JavaScript 脚本引擎（Jint，停止使用）
├── Application.Scripting.Lua/            — Lua 脚本引擎（NLua，未投入使用）
├── Application.Plugin.Script/            — C# 重写的脚本
│                                          ⚠ 插件项目，未直接引用，以热更新方式加载。修改后需 `dotnet build`，将 dll 放入 `Plugins/`
├── Application.Plugin.FakeCharacter/     — 假人/自动角色插件
│                                          ⚠ 同上，插件项目需单独构建
├── Application.Benchmark/                — BenchmarkDotNet 性能基准测试
└── app/
    ├── RuaMS.AppHost/                    — .NET Aspire 应用编排
    └── RuaMS.ServiceDefaults/            — .NET Aspire 服务默认配置
```

### 模块体系（`modules/`）

各功能模块遵循一致的**四层结构**：
- `*.Master` — Master 服务器端逻辑
- `*.Channel` — Channel 服务器端逻辑
- `*.Common` — 共享 DTO 和 gRPC 服务定义
- `*.Channel.InProgress` — 单进程模式的桥接模块

已有模块：

| 模块 | 说明 |
|------|------|
| `Application.Module.BBS` | 论坛/公告板系统 |
| `Application.Module.Duey` | 游戏内邮件系统 |
| `Application.Module.Family` | 家族系统 |
| `Application.Module.Fishing` | 钓鱼系统 |
| `Application.Module.MTS` | Meso 交易系统 |
| `Application.Module.Maker` | 道具制作系统 |
| `Application.Module.Marriage` | 结婚系统 |
| `Application.Module.PlayerNPC` | 玩家 NPC 系统 |
| `Application.Module.ExpeditionBossLog` | Boss 远征记录 |

### 资源说明

#### scripts

当前使用的 `scripts` 来自 [BeiDou](https://github.com/BeiDouMS/BeiDou-Server)。
C# 重写版本在 `Application.Plugin.Script` 中，需单独 `dotnet build`，将生成的 dll 放入根目录 `Plugins/` 下。

#### wz

当前项目使用的 `wz` 来自 [BeiDou](https://github.com/BeiDouMS/BeiDou-Server) 。

### tools

- **CodeMigration**：代码迁移转换工具
  - `ReplaceJsContent` 类：替换 JS 脚本中的 Java 代码

### test

- **ServiceTest** 测试项目
- **XmlWzReader.SourceGenerator** 代码生成，辅助生成 Template 赋值代码

## 运行

### 支持数据库

- **SQLite（默认）**
- **MySQL 8**

### 部分功能需要修改的配置

#### 公网运行

修改 `appsettings.json` 中的 `ChannelServerConfig.ServerHost`

#### 使用其他数据库

修改 `Application.Host/appsettings.json` 中的 `ConnectionStrings`

#### 接入其他频道服务器

1. `Application.Host` 设置：解除 `Kestrel:Endpoints:grpc` 节的注释
2. `Application.Host.Channel` 设置：`Services:ruams-master:grpc`

### 单进程模式（master-standalone）

```
dotnet run --project "./src/Application.Host/Application.Host.csproj"
```

### 多进程模式

解除 `Kestrel:Endpoints:grpc` 节的注释

1. 运行 `Application.Host.exe`
2. 运行 `Application.Host.Channel.exe`

## Docker 运行

```bash
docker run -itd \
-p 8080:8080 \
-p 8484:8484 \
-p 7575-7600:7575-7600 \
-v ./logs:/app/logs \
--name ruams-standalone \
sigeer/ruams-standalone

# 环境变量中使用 RUA_MS_ 前缀即可覆盖相应配置文件的值

# 使用 MySQL 需要设置数据库类型及连接字符串
-e "RUA_MS_Database=MySql" \
-e "RUA_MS_ConnectionStrings__MySql=" \

# 公网运行需要修改 ChannelServerConfig.ServerHost
-e "RUA_MS_ChannelServerConfig__ServerHost=" \

# 允许接入其他频道服务器
-p 7878:7878 \
-e "RUA_MS_Kestrel__Endpoints__grpc__Url=http://0.0.0.0:7878" \
-e "RUA_MS_Kestrel__Endpoints__grpc__Protocols=Http2" \
```
