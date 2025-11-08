
## 项目介绍

基于 Cosmic 的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)


### 注意 ❗

大量内容未经测试，可能出现大量BUG，不建议搭建游玩。

## 开发环境

- .Net9
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
如果有其他java项目的脚本资源，也可以参考[CodeMigration.MD](https://github.com/sigeer/RuaMS/blob/master/docs/CodeMigration.MD#js)对js进行处理

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

### 步骤

1. 配置 Application.Host 下的 `appsettings.json` 中的连接字符串以及其他设置
2. 检查wz路径：放在当前应用根目录，或者设置环境变量`RUA_MS_WZConfig__BaseDir`/`WZConfig__BaseDir`

### 单进程模式（master-standalone）

配置`UseExtraChannel=false`，直接运行`Application.Host.exe`


### 多进程模式

配置`UseExtraChannel=true`

- 使用master版：仅登录服务器，必须启动`Application.Host.Channel`
- 使用master-standalone版：登录服务器+频道服务器，通过启动`Application.Host.Channel`扩展