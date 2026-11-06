using Microsoft.EntityFrameworkCore;

namespace Application.Core.EF;

/// <summary>
/// 数据库提供程序及其在 <c>HasColumnType</c> 中使用的列类型。
/// </summary>
public enum DbProvider
{
    Sqlite,
    PostgreSql,
}

/// <summary>
/// 数据基础类型到各提供程序物理列类型的映射。
/// <para>
/// 模型配置中用 <see cref="DbColumnTypes.For{T}"/> 之类的调用取值，避免在
/// <c>OnModelCreating</c> 里散落 <c>isPostgres ? "bytea" : "BLOB"</c> 这样的三元表达式。
/// </para>
/// </summary>
public static class DbColumnTypes
{
    /// <summary>有符号 8 位整数。</summary>
    public static string TinyInt(DbProvider provider) => provider switch
    {
        // SQLite 只有 INTEGER，tinyint 由类型亲和性退化成 INTEGER
        DbProvider.Sqlite => "tinyint",
        // PostgreSQL 没有 1 字节整数，smallint 是能容纳其取值的最小类型
        DbProvider.PostgreSql => "smallint",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
    };

    /// <summary>布尔值。</summary>
    public static string Boolean(DbProvider provider) => provider switch
    {
        DbProvider.Sqlite => "tinyint(1)",
        DbProvider.PostgreSql => "boolean",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
    };

    /// <summary>二进制大对象。</summary>
    public static string Blob(DbProvider provider) => provider switch
    {
        DbProvider.Sqlite => "BLOB",
        DbProvider.PostgreSql => "bytea",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
    };

    /// <summary>短文本。</summary>
    public static string TinyText(DbProvider provider) => provider switch
    {
        DbProvider.Sqlite => "tinytext",
        DbProvider.PostgreSql => "text",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
    };

    /// <summary>定长字符串，<paramref name="length"/> 为字符长度。</summary>
    public static string Varchar(DbProvider provider, int length) => $"varchar({length})";

    /// <summary>日期（不含时间）。</summary>
    public static string Date(DbProvider provider) => "date";

    /// <summary>16 位整数。</summary>
    public static string SmallInt(DbProvider provider) => "smallint";

    /// <summary>32 位整数。</summary>
    public static string Int(DbProvider provider) => "int";

    /// <summary>64 位整数。</summary>
    public static string BigInt(DbProvider provider) => "bigint";

    /// <summary>长文本。</summary>
    public static string Text(DbProvider provider) => "text";

    /// <summary>
    /// 不区分大小写比较所用的排序规则名，两侧都通过 <c>UseCollation</c> 应用。
    /// <para>
    /// SQLite 用内置的 <c>NOCASE</c>；PostgreSQL 用 <c>case_insensitive</c>，它是对 ICU
    /// <c>und-u-ks-level2</c>（第 2 级比较，忽略大小写）的非确定性封装，由迁移语句
    /// <c>CREATE COLLATION</c> 创建，不需要额外安装扩展。
    /// </para>
    /// <para>
    /// 注意：PostgreSQL 的非确定性排序规则不支持 <c>LIKE</c>，模糊匹配需改用 <c>ILIKE</c>。
    /// </para>
    /// </summary>
    public static string CaseInsensitiveCollation(DbProvider provider) => provider switch
    {
        DbProvider.Sqlite => SqliteCaseInsensitiveCollation,
        DbProvider.PostgreSql => PostgreSqlCaseInsensitiveCollation,
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
    };

    /// <summary>PostgreSQL 迁移中创建的不区分大小写排序规则名。</summary>
    public const string PostgreSqlCaseInsensitiveCollation = "case_insensitive";

    /// <summary>SQLite 内置的不区分大小写排序规则名。</summary>
    public const string SqliteCaseInsensitiveCollation = "NOCASE";

    /// <summary>PostgreSQL 的 <c>citext</c> 扩展名。</summary>
}
