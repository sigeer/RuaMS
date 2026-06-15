---
name: sync-script
description: 同步脚本代码
---

目标仓库目录：gitRepo
开始的commit: commit
范围：dir
获取该目录从commit至今的所有修改过的文件，

使用命令
`cd {gitRepo} && $tmpfile = New-TemporaryFile; git --no-pager diff --name-only {commit} HEAD -- {dir}/ > $tmpfile.FullName; $tmpfile.FullName`

读取$tmpfile的内容，获得需要处理的js文件列表
使用refactor-from-js将这些文件修改同步到当前项目。

这些修改基本上是一些字符串汉化和新增，对于字符串汉化的部分，如果我的代码中已经处理过了，则跳过