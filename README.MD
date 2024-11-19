
### 项目介绍

基于 Cosmic 的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)

初期代码来自字符串替换，可能存在大量bug，不建议搭建游玩。


### 开发环境

- .Net8
- Mysql8

### 项目结构

#### src

- Application.Console 入口应用
- Application.Core 核心功能，目录结构与Cosmic源码类似，部分经过较大整理或者重构，移到了Game目录
- Application.Utility 工具类

#### tools

- CodeMigration：转换代码的迁移工具
	- ReplaceJsContent类：用于替换js脚本中的Java代码

#### test

- ServiceTest 测试


### 运行的准备工作

推荐使用[Comic](https://github.com/P0nk/Cosmic?tab=readme-ov-file#3---client)的wz以及配套的客户端（本项目中的js都是来自Cosmic）
1. 将 wz 目录放置在根目录，或者配置环境变量`wz-path`，或者使用`Environment.SetEnvironmentVariable("wz-path", "wz目录");`
2. 设置`config.yaml`中的连接字符串


#### 数据库迁移

```
// 在 Application.Core 中执行
dotnet ef migrations add [migration] --output-dir EF/Migrations --startup-project ../Application.Host

dotnet ef database update --startup-project ../Application.Host
```