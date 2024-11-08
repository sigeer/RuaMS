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

- 移除 `Java.type`  CodeMigration替换，暂不用处理
- 移除 `Java.to` CodeMigration替换，暂不用处理
- `iterator()` 用 `for ... of `替换
- `size()`, `toArray()`, `get()` 目前通过扩展方法适配，暂不用处理
- 一些静态方法的类名已经发生修改
	`Channel` -> `WorldChannel`
	`Character` -> `CharacterManager`
	`Wedding` -> `WeddingPackets`
- 一些静态方法进行了移动
	`RingActionHandler`中的静态方法移动到了`RingManager`
- `commands.js`