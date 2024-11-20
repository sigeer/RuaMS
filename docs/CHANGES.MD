- `async/await` 的处理，如果使用`async/await`，js脚本部分也要进行相应的调整，需要大量修改。改为同步调用
- Java中的byte，对应C#中的sbyte，在网络请求中，byte与sbyte没有区别。但是作为数值参与计算时，两者有区别。
- 移除`GuildSummary`
- 角色在线功能的判定：原先的`lodgeIn`字段被移除，通过`Client`的实现来判断
- 通过分部类将`Character`按功能拆分 （未完成）
- 提取了一些静态方法到`XXXManager`中
- 提取了一些接口
- Party重命名为Team
- 调整了PlayerStorage，通过`OnChannelAddPlayer`事件更新WorldPlayerStorage
	- 修改了机制，之前PlayerStorage中只会存放在线的角色。现在改为存放所有关联到的角色，所有角色都从`WorldStorage`中获取。所以之前通过null判断在线的，还需要进行在线判断
- 移除`GuildCharacter`、`PartyCharacter`
	- 还有一些Party.members循环的null判断，但是并没有看到为null的情况（下线不会置null）
- `Gachapon` 相关数据改为从数据库读取

##### 2024年11月20日
- worldconfig 从配置文件 移到数据库。另外，之前的逻辑中，worldId 是基于worldCount的。新的逻辑中不再有这种关联，可能成为隐患
- 由于代码迁移不知道什么时候生成了一个`drop_data.mobid1`的索引导致报错，需要删库重新运行