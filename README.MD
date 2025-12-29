
## 项目介绍

基于 Cosmic 的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)


### 注意 ❗

大量内容未经测试，可能出现大量BUG，不建议搭建游玩。

## 开发环境

- .Net10
- Mysql8（可选）

## 项目结构

### src

- **Application.Host** 主服务器入口
- **Application.Host.Channel** 频道服务器入口
- **Application.Core** 频道服务器功能
- **Application.Core.EF** EF
- **Application.Core.EF.MySQL** MySQL驱动，及迁移文件、初始化脚本
- **Application.Core.EF.Sqlite** Sqlite驱动，及迁移文件、初始化脚本
- **Application.Core.Channel.InProgress** 进程内同时运行主服务器 + 频道服务器的实现 （Application.Modules.XXX.Channel.InProgress）
- **Application.Core.Login** 登录服务器、主服务器实现
- **Application.Proto** proto文件
- **Application.Utility** 工具类
- **Application.Resource** 资源：yaml配置文件（后期逐步移除）、script、wz
- **Application.Shared** 一些公用数据结构、类型
- **XmlWzReader** xml格式的wz读取工具
- **Application.Templates** wz数据模板，附[Template.md](https://github.com/sigeer/RuaMS/blob/master/src/Application.Templates/Template.md)
- **Application.Templates.XmlWzReader** wz数据模板数据绑定 wz/xml读取实现
- **Application.Scripting** 脚本引擎
- **Application.Scripting.JS** js脚本引擎
- **Application.Scripting.Lua** lua脚本引擎（未投入使用）
- **Application.Benchmark** 重构代码时，在该项目测试新代码的性能是否优于旧代码

#### 资源说明

##### script

当前使用的`scripts`来自 [BeiDou](https://github.com/BeiDouMS/BeiDou-Server)。
如果有其他java项目的脚本资源，也可以参考[CodeMigration.MD](https://github.com/sigeer/RuaMS/blob/master/docs/CodeMigration.MD#js)对js进行处理（不能保证全覆盖）

##### wz

当前项目使用的`wz`来自 [BeiDou](https://github.com/BeiDouMS/BeiDou-Server) 。

### tools

- **CodeMigration**：转换代码的迁移工具
	- ReplaceJsContent类：用于替换js脚本中的Java代码

### test

- **ServiceTest** 测试
- **XmlWzReader.SouceGenerator** 代码生成，辅助生成Template赋值代码

## 运行

### 支持数据库

- **Sqlite （默认）**
- Mysql 8

### 部分功能需要修改的配置

#### 公网运行

修改`appsettings.json`中的`ChannelServerConfig.ServerHost`

#### 使用其他数据库

修改`Application.Host`.`appsettings.json`中的`ConnectionStrings`

#### 接入其他频道服务器

1. `Application.Host`的设置：解除`Kestrel:Endpoints:grpc`节的注释
2. `Application.Host.Channel`的设置`Services:ruams-master:grpc`


### 单进程模式（master-standalone）

1. 运行`Application.Host.exe`

### 多进程模式

解除`Kestrel:Endpoints:grpc`节的注释

1. 运行`Application.Host.exe`
2. 运行`Application.Host.Channel.exe`

## docker 运行

```bash
docker run -itd \
-p 8080:8080 \
-p 8484:8484 \
-p 7575-7600:7575-7600 \
-v ./logs:/app/logs \
--name ruams-standalone \
sigeer/ruams-standalone

# 环境变量中使用 RUA_MS_ 前缀即可覆盖相应配置文件的值

# 使用Mysql需要设置数据库类型及连接字符串
-e "RUA_MS_Database=MySql" \
-e "RUA_MS_ConnectionStrings__MySql=" \

# 公网运行运行需要修改`ChannelServerConfig.ServerHost`
-e "RUA_MS_ChannelServerConfig__ServerHost=" \

# 允许接入其他频道服务器
-p 7878:7878 \
-e "RUA_MS_Kestrel__Endpoints__grpc__Url=http://0.0.0.0:7878" \
-e "RUA_MS_Kestrel__Endpoints__grpc__Protocols=Http2" \
```