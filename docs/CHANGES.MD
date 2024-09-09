2024年8月28日

- 移除`GuildSummary`
- 角色在线功能的判定：原先的`lodgeIn`字段被移除，通过`Client`的实现来判断
- 通过分部类将`Character`按功能拆分 （未完成）
- 提取了一些静态方法到`XXXManager`中
- 提取了一些接口
- Party重命名为Team
- 调整了PlayerStorage，通过`OnChannelAddPlayer`事件更新WorldPlayerStorage
- 移除`GuildCharacter`、`PartyCharacter`
	- 修改了机制，之前PlayerStorage中只会存放在线的角色。现在改为存放所有关联到的角色，所有角色都从`WorldStorage`中获取。所以之前通过null判断在线的，还需要进行在线判断


2024.9.6

- Storage 从Player 移到 Account