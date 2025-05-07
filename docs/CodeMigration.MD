主体代码通过字符串替换处理

### C#

```
// 部分多参数枚举替换
(\S*?)\((.*?)\),
public static readonly ItemFactory $1 = new ItemFactory($2);

Entry<.*>\s+(.*?)in
var $1 in

private MonitoredReentrantLock (\S*) = MonitoredReentrantLockFactory.createLock.*?;
private object $1 = new object();

private MonitoredReentrantReadWriteLock (\S*) = new MonitoredReentrantReadWriteLock.*?;
private ReaderWriterLockSlim $1 = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

Monitor\.Enter\(([a-zA-Z]*?)WLock\);
$1Lock.EnterWriteLock();

Monitor\.Exit\(([a-zA-Z]*?)WLock\);
$1Lock.ExitWriteLock();

Monitor\.Enter\(([a-zA-Z]*?)RLock\);
$1Lock.EnterReadLock();

Monitor\.Exit\(([a-zA-Z]*?)RLock\);
$1Lock.ExitReadLock();

substring(startIndex, endIndex)
SubString(startIndex, length);

MINUTES\.toMillis\((\S+)\)
TimeSpan.FromMinutes($1)

HOURS\.toMillis\((\S+)\)
TimeSpan.FromHours($1)

SECONDS.toMillis\((\S+)\)
TimeSpan.FromSeconds($1)
```

### JS

| 范围| 待处理的代码 | 需要处理 | 备注 |
| -----------| ----------- | ----------- |----------- |
| - | `Java.type`      | - | CodeMigration替换 |
| - | `Java.to`      | - | CodeMigration替换 |
| - | `iterator()`      | ❗ | 需要手动处理 |
| - | `size()`, `toArray()`, `get()` | - | 目前通过扩展方法适配，暂不用处理 |
| - | `Channel.getRelativeWeddingTicketExpireTime` -> `CommonManager.GetRelativeWeddingTicketExpireTime` | ❗ | 静态方法移动 |
| - | `Character` -> `CharacterManager` | ❗ | 静态方法移动 |
| - | `Wedding` -> `WeddingPackets` | - | jint保持了相同的别名，暂不用处理 |
| - | `RingActionHandler` -> `RingManager` | ❗ | 静态方法移动 |
| `commands.js` | - | ❗ | 需要重写 |
| `gachaponInfo.js` | `Gachapon.GachaponType` -> `GachaponManager`| ❗ |  |
| `2xEvent.js` | `Server.getInstance().getWorld(em.getChannelServer().getWorld());` -> `em.getWorldServer();` | - | CodeMigration替换 |