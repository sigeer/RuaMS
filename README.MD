
### 项目介绍

基于 Cosmic 的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)

初期代码来自字符串替换，可能存在大量bug，不建议搭建游玩。


### 开发环境

- .Net9
- Mysql8

### 项目结构

#### src

- Application.Host 入口应用
- Application.Core 核心功能，目录结构与Cosmic源码类似，部分经过较大整理或者重构，移到了Game目录
- Application.Utility 工具类
- Application.Resource 资源：js、初始化数据sql、配置文件
- Application.Shared 一些公用数据结构、类型
- XmlWzReader xml格式的wz读取工具

#### tools

- CodeMigration：转换代码的迁移工具
	- ReplaceJsContent类：用于替换js脚本中的Java代码

#### test

- ServiceTest 测试

### 数据库迁移

```
// 在 Application.Core 中执行
dotnet ef migrations add [migration_name] --output-dir EF/Migrations --startup-project ../Application.Host

dotnet ef database update --startup-project ../Application.Host
```

### 运行

#### 需要安装的软件

1. Mysql 8

#### 步骤

1. 配置 config.yaml，主要是连接字符串、IP
2. v83客户端（wz） 、服务端wz、服务端Js需要配套。 


项目中的`scripts`来自[BeiDou](https://github.com/BeiDouMS/BeiDou-Server)，
可以使用该项目中的wz、以及客户端。

也可以使用[Commit:ba4bbc26a65e76e12c5ae1b2621390ddaef7a4e1](https://github.com/sigeer/Cosmic_DotNet/commit/ba4bbc26a65e76e12c5ae1b2621390ddaef7a4e1) 之前的 `scripts`
（来自于 [Cosmic](https://github.com/P0nk/Cosmic)）

如果有其他配套的资源，也可以参考[CodeMigration.MD](https://github.com/sigeer/Cosmic_DotNet/blob/master/docs/CodeMigration.MD#js)对js进行处理


#### docker 运行

**生成镜像**

`docker build -f ./src/Application.Host/Dockerfile -t cosmic-dotnet:latest -t cosmic-dotnet:0.0.1 .`

**运行**

```bash
docker run -itd \
-p 8080:8080 \
-p 8484:8484 \
-p 7575-7600:7575-7600 \
-v ./logs:/app/logs \
--name cosmic-dotnet \
cosmic-dotnet

# 非docker compose运行需要设置连接字符串
-e "COSMIC_DOTNET_DB_CONNECTIONSTRING=" \

# 公网运行运行需要修改Host
-e "COSMIC_DOTNET_HOST=" \
```

**docker compose运行**

```
docker compose up -d
```