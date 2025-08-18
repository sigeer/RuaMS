
## 项目介绍

基于 Cosmic 的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)


### 注意 ❗

大量内容未经测试，可能出现大量BUG，不建议搭建游玩。

## 开发环境

- .Net9
- Mysql8

## 项目结构

### src

- **Application.Host** 主服务器入口
- **Application.Host.Channel** 频道服务器入口
- **Application.Core** 频道服务器功能
- **Application.Core.EF** EF
- **Application.Core.EF.MySQL** MySQL驱动，及迁移文件、初始化脚本
- **Application.Core.EF.Sqlite** Sqlite驱动，及迁移文件、初始化脚本
- **Application.Core.Channel.InProgress** 进程内同时运行主服务器+频道服务器的实现 （Application.Modules.XXX.Channel.InProgress）
- **Application.Core.Login** 登录服务器、主服务器实现
- **Application.Proto** proto文件
- **Application.Utility** 工具类
- **Application.Resource** 资源：yaml配置文件 后期逐步移除
 > 当前使用的`scripts`来自[BeiDou](https://github.com/BeiDouMS/BeiDou-Server)，可以使用该项目中的wz、以及客户端。    
也可以使用[Commit:ba4bbc26a65e76e12c5ae1b2621390ddaef7a4e1](https://github.com/sigeer/RuaMS/commit/ba4bbc26a65e76e12c5ae1b2621390ddaef7a4e1) 之前的 `scripts`
（来自于 [Cosmic](https://github.com/P0nk/Cosmic)） 但是代码发生了多次更新可能存在BUG
如果有其他配套的资源，也可以参考[CodeMigration.MD](https://github.com/sigeer/RuaMS/blob/master/docs/CodeMigration.MD#js)对js进行处理
- **Application.Shared** 一些公用数据结构、类型
- **XmlWzReader** xml格式的wz读取工具
- **Application.Scripting** 脚本引擎
- **Application.Scripting.JS** js脚本引擎
- **Application.Scripting.Lua** lua脚本引擎（目前事件脚本使用lua重写的版本，文件夹`/scripts/event-lua`）
- **Application.Benchmark** 重构代码时，在该项目测试新代码的性能是否优于旧代码

### tools

- CodeMigration：转换代码的迁移工具
	- ReplaceJsContent类：用于替换js脚本中的Java代码

### test

- ServiceTest 测试

## 发布

通过设置 -p:IsStandalone 切换输出版本

### master-standalone

内置`Application.Core.Channel.InProgress`，可通过设置`ChannelServerConfig`调整频道，也可通过设置`UseExtraChannel=true`接入其他频道服务器

### master

必须设置`UseExtraChannel=true`，不然毫无意义

### channel

通过设置`ChannelServerConfig.MasterServerGrpcAddress`连接到`MasterServer`

### docker（默认单机模式）

**生成镜像**

`docker build -f ./src/Application.Host/Dockerfile-standalone -t ruams-standalone:latest -t ruams-standalone:0.0.1 .`

**运行**

```bash
docker run -itd \
-p 8080:8080 \
-p 8484:8484 \
-p 7575-7600:7575-7600 \
-v ./logs:/app/logs \
--name ruams-standalone \
sigeer/ruams-standalone

# 环境变量中使用 RUA_MS_ 前缀即可覆盖相应配置文件的值
# 使用Mysql且非docker compose运行需要设置连接字符串
-e "RUA_MS_Database=MySql" \
-e "RUA_MS_ConnectionStrings__MySql=" \

# 公网运行运行需要修改`ChannelServerConfig.ChannelConfig`的Host
-e "RUA_MS_ChannelServerConfig__ServerHost=" \

# 允许接入其他频道服务器
-p 7878:7878 \
-e "RUA_MS_UseExtraChannel=true" \
```

**sqlite数据库备份**  
`docker cp [container_id]:/app/database.db ./backup.db`

## 运行

### 支持数据库

- **Sqlite （默认）**
- Mysql 8

### 步骤

1. 配置 Application.Host 下的 `appsettings.json` 中的连接字符串以及其他设置
2. 检查wz路径，需要放在当前应用根目录，或者设置环境变量`ms-wz`

### 单进程模式（master-standalone）

配置`UseExtraChannel=false`，登录服务器 和 频道服务器处于同一进程内，直接运行`Application.Host.exe`即可


### 多进程模式

配置`UseExtraChannel=true`

1. 登录服务器 与 频道服务器 在不同进程：master
2. 一部分频道与登录服务器在同一进程（默认），一部分频道在其他进程：master-standalone
3. 先启动`Application.Host`，启动完成后再启动`Application.Host.Channel`