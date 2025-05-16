
## 项目介绍

基于 Cosmic 的C#版。
截至 [Commit: a5b572023b8bab68ab5f4ce1478cf2fcdb969770](https://github.com/P0nk/Cosmic/commit/a5b572023b8bab68ab5f4ce1478cf2fcdb969770)

初期代码来自字符串替换，可能存在大量bug，不建议搭建游玩。


## 开发环境

- .Net9
- Mysql8

## 项目结构

### src

- Application.Host 入口应用
- Application.Core 核心功能，目录结构与Cosmic源码类似，部分经过较大整理或者重构，移到了Game目录
- Application.Utility 工具类
- Application.Resource 资源：js、初始化数据sql、配置文件
 > 当前使用的`scripts`来自[BeiDou](https://github.com/BeiDouMS/BeiDou-Server)，可以使用该项目中的wz、以及客户端。    
也可以使用[Commit:ba4bbc26a65e76e12c5ae1b2621390ddaef7a4e1](https://github.com/sigeer/RuaMS/commit/ba4bbc26a65e76e12c5ae1b2621390ddaef7a4e1) 之前的 `scripts`
（来自于 [Cosmic](https://github.com/P0nk/Cosmic)）  
如果有其他配套的资源，也可以参考[CodeMigration.MD](https://github.com/sigeer/RuaMS/blob/master/docs/CodeMigration.MD#js)对js进行处理
- Application.Shared 一些公用数据结构、类型
- XmlWzReader xml格式的wz读取工具
- Application.Scripting 脚本引擎
- Application.Scripting.JS js脚本引擎
- Application.Scripting.Lua lua脚本引擎（未启用）
- Application.Benchmark 重构代码时，在该项目测试新代码的性能是否优于旧代码

### tools

- CodeMigration：转换代码的迁移工具
	- ReplaceJsContent类：用于替换js脚本中的Java代码

### test

- ServiceTest 测试

### 数据库迁移

```
// 在 Application.Core 中执行
dotnet ef migrations add [migration_name] --output-dir EF/Migrations --startup-project ../Application.Host

dotnet ef database update --startup-project ../Application.Host
```

### 运行

#### 需要安装的软件

- Mysql 8

#### 步骤

1. 配置 `config.yaml`，主要是连接字符串`DB_CONNECTIONSTRING`、`HOST`
2. 运行`Application.Host.exe`

#### docker

**生成镜像**

`docker build -f ./src/Application.Host/Dockerfile -t rua-ms:latest -t rua-ms:0.0.1 .`

**运行**

```bash
docker run -itd \
-p 8080:8080 \
-p 8484:8484 \
-p 7575-7600:7575-7600 \
-v ./logs:/app/logs \
--name rua-ms \
rua-ms

# 环境变量中使用 RUA_MS_ 前缀即可覆盖相应配置文件的值
# 非docker compose运行需要设置连接字符串
-e "RUA_MS_ConnectionStrings__MySql=" \

# 公网运行运行需要修改Host
-e "RUA_MS_HOST=" \
```

**docker compose运行**

在根目录（`docker-compose.yaml`同级目录）下执行
```
docker compose up -d
```