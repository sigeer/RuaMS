# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RuaMS is a MapleStory private server emulator written in C#, ported from the Java-based [Cosmic](https://github.com/P0nk/Cosmic). It targets **.NET 10** and supports both **SQLite** (default) and **MySQL 8** databases. The project is in active development and may still have bugs.

## Build & Test Commands

```bash
# Build the master/standalone server
dotnet build "./src/Application.Host/Application.Host.csproj"

# Run the standalone server (single process, master + channel in-progress)
dotnet run --project "./src/Application.Host/Application.Host.csproj"

# Run infrastructure tests (no server needed)
dotnet test --filter "FullyQualifiedName~ServiceTest.Infrastructure" ./test/ServiceTest/ServiceTest.csproj

# Run game/server tests (starts a local test server)
dotnet test --filter "FullyQualifiedName~ServiceTest.Games" ./test/ServiceTest/ServiceTest.csproj

# Run all tests
dotnet test ./test/ServiceTest/ServiceTest.csproj

# Run a single test
dotnet test --filter "FullyQualifiedName~ServiceTest.Infrastructure.UtilityTests" ./test/ServiceTest/ServiceTest.csproj

# Build channel server separately (multi-process mode)
dotnet build "./src/Application.Host.Channel/Application.Host.Channel.csproj"

# Add EF migration (run in src/Application.Core.EF.MySQL or Sqlite)
dotnet ef migrations add <Name> --output-dir Migrations --startup-project ../Application.Host -- --DataBase=MySql/Sqlite

# Remove last EF migration
dotnet ef migrations remove --startup-project ../Application.Host

# Run benchmarks
dotnet run --project ./src/Application.Benchmark/Application.Benchmark.csproj -c Release
```

## Architecture

### Server Deployment Modes

The server supports two deployment modes, controlled by the `IsStandalone` MSBuild property:

1. **Standalone** (`IsStandalone=true`, default): Single process running both Master and Channel servers in-progress via `Application.Core.Channel.InProgress`. Entry point: `Application.Host`.

2. **Multi-process** (`IsStandalone=false` or conditionally undefined): Separate processes communicate via gRPC:
   - `Application.Host` — Master server (login handling, world management, admin REST API)
   - `Application.Host.Channel` — Channel server(s) for gameplay (gRPC client to master)

### Solution Map (40+ projects)

```
src/
├── Application.Host/              — Master server entry point (ASP.NET)
├── Application.Host.Channel/      — Channel server entry point (ASP.NET)
├── Application.Core/              — Channel core: game logic, networking (DotNetty), commands, world management
├── Application.Core.Login/        — Master server: login handling, world/channel registry
├── Application.Core.Channel/      — Channel abstractions (targets net9.0, being migrated into Core)
├── Application.Core.Channel.InProgress/ — In-progress channel server (standalone mode bridge)
├── Application.Core.EF/           — EF Core data layer abstractions
│   ├── Application.Core.EF.MySQL/ — MySQL provider with migrations & seed data
│   └── Application.Core.EF.Sqlite/ — SQLite provider with migrations & seed data
├── Application.Shared/            — Shared types, constants, networking, packets, DTOs
├── Application.Utility/           — Logging (Serilog), config, Quartz scheduler, AutoMapper, extensions
├── Application.Protos/            — gRPC/protobuf definitions (~130 .proto files)
├── Application.Templates/         — WZ data template models (character, item, map, mob, NPC, skill, etc.)
├── Application.Templates.Reader.Xml/ — Data binding: WZ XML → template objects
├── Application.Resources/         — Static resources: wz/, scripts/, yaml configs
├── XmlWzReader/                   — XML WZ data file reader
├── Application.Scripting/         — Script engine abstraction
├── Application.Scripting.JS/      — JavaScript scripting (Jint engine, actively used)
├── Application.Scripting.Lua/     — Lua scripting (NLua, not yet in use)
├── Application.Plugin.Script/     — C#-rewritten scripts (NPC, quest, portal, reactor, map, item, event)
├── Application.Plugin.FakeCharacter/ — Automated fake/mule character plugin
├── Application.Benchmark/         — BenchmarkDotNet performance benchmarks
├── modules/                       — Feature modules (each: Master + Channel + Common + InProgress)
│   ├── Application.Module.BBS/    — Bulletin Board System
│   ├── Application.Module.Duey/   — In-game mail system
│   ├── Application.Module.Family/ — Family system
│   ├── Application.Module.Fishing/ — Fishing system
│   ├── Application.Module.MTS/    — Meso Transfer System
│   ├── Application.Module.Maker/  — Item crafting
│   ├── Application.Module.Marriage/ — Marriage system
│   ├── Application.Module.PlayerNPC/ — Player NPC system
│   └── Application.Module.ExpeditionBossLog/ — Boss expedition logging
└── app/
    └── RuaMS.AppHost/             — .NET Aspire app host for orchestration
        RuaMS.ServiceDefaults/     — .NET Aspire service defaults
```

### Inter-Server Communication

- **Client <-> Server**: MapleStory protocol over TCP via DotNetty (`DotNetty.Buffers`, `.Codecs`, `.Transport`, `.Handlers`)
- **Master <-> Channel**: gRPC with protobuf serialization (service definitions in `Application.Protos/`)

### Database

- SQLite (default) via `Microsoft.EntityFrameworkCore.Sqlite`
- MySQL 8 via `Pomelo.EntityFrameworkCore.MySql`
- Database type configured via `Database` setting in appsettings.json (must match a key in `ConnectionStrings`)
- Environment variables with `RUA_MS_` prefix override any config value

### Scripting

- NPC dialogue, events, portals, items, quests, and reactors are scripted
- JavaScript engine (Jint) for `scripts/` — legacy .js scripts from BeiDou project
- C# scripts via `Application.Plugin.Script` — actively rewritten from JS, enables IDE navigation/refactoring
- **Event system**: Channel → EventScriptManager(1:1) → EventManager(1:N per script) → EventInstanceManager(1:N instances)
- **NPC dialog**: Channel → NPCScriptManager(1:1) creates NPCConversationManager(cn) via start() → action() stages

### Feature Module Pattern

Each feature module follows a consistent layered structure:
- `*.Master` — Master server logic (runs in master process)
- `*.Channel` — Channel server logic (runs in channel process)
- `*.Common` — Shared DTOs and gRPC service definitions shared between master and channel
- `*.Channel.InProgress` — Bridge module for standalone mode (channels master → channel calls)

### Networking / Packet Handling

- `IPacketProcessor<IChannelClient>` / `ChannelPacketProcessor` — routes incoming packets to registered handlers
- `ChannelHandlerBase` — base class that auto-registers handlers via assembly scanning
- Custom `KeepAliveHandler<IChannelClient>` and `CustomPacketHandler<IChannelClient>`

## Configuration Notes

- `appsettings.json` keys can be overridden via environment variables prefixed with `RUA_MS_` (e.g., `RUA_MS_Database=MySql`, `RUA_MS_ChannelServerConfig__ServerHost=...`)
- `LongIdSeed` must be unique per server instance (Yitter ID generator)
- For public server: set `ChannelServerConfig.ServerHost` to the public IP/DNS
- For multi-process mode: uncomment `Kestrel:Endpoints:grpc` section in both hosts

## Key Dependencies

| Package | Purpose |
|---------|---------|
| DotNetty (4.x) | MapleStory protocol TCP networking |
| Grpc.AspNetCore | Inter-server gRPC communication |
| Jint | JavaScript scripting engine |
| Serilog + Sinks.Async/File/Map | Structured logging |
| AutoMapper | Object-to-object mapping |
| Quartz | Job scheduling |
| Yitter.IdGenerator | Snowflake-style distributed IDs |
| Pomelo.EntityFrameworkCore.MySql | MySQL EF provider |
| ZLinq | Enhanced LINQ operations |
| BenchmarkDotNet | Performance benchmarks (test project) |

## Naming & Code Style

- LF line endings, UTF-8 encoding
- 4-space indentation
- Interface prefix `I` (PascalCase)
- Types, methods, properties, events: PascalCase
- `this` qualification: not used for fields/properties/methods/events
- Expression-bodied members: properties/indexers/accessors/lambdas preferred; methods/constructors/operators use block body
- `var` not used for built-in types or when type is apparent
- `using` placed outside namespace
- Prefer `using` statements over braces
- Suppressed analyzers: IDE1006 (naming), IDE0066 (switch), IDE0290 (primary constructor)
- **Constants and fields: PascalCase** (`SomeName` not `SOME_NAME`); local variables: camelCase
