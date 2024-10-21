
### 项目介绍

基于 Cosmic的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)

初期代码来自字符串替换，可能存在大量bug，不建议搭建游玩。


## 项目

### 项目结构

#### src

- Application.Console 入口应用
- Application.Core 核心功能，目录结构与Cosmic源码类似
- Application.Utility 工具类

#### tools

- CodeMigration：转换代码的迁移工具
	- ReplaceJsContent类：用于替换js脚本中的Java代码

#### sql

初始化sql脚本。
 由于EFCore会将`tiny(1)` 视作`bool`，现将脚本中非bool类型却使用`tiny(1)`的字段的数据类型改为`tiny`。

#### test

- ServiceTest 测试

### 记录

1. `async/await` 的处理，如果使用`async/await`，js脚本部分也要进行相应的调整，需要大量修改。改为同步调用
2. Java中的byte，对应C#中的sbyte，在网络请求中，byte与sbyte没有区别。但是作为数值参与计算时，两者有区别。
