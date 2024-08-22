## 代码更新的处理方式

### 代码替换以及正则表达式

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

js中
```
移除 Java.type

移除 Java.to
```