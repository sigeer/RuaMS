using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Utility
{
    public static class DynamicObjectFactory
    {
        // 缓存键：目标类型全名 + 参数类型全名组合（避免冲突）
        private static readonly ConcurrentDictionary<string, Delegate> _cache = new();

        // ==================== 0 个参数 ====================
        public static object Create(Type type)
        {
            string key = type.AssemblyQualifiedName;
            var creator = (Func<object>)_cache.GetOrAdd(key, _ => BuildCreator0(type));
            return creator();
        }

        private static Func<object> BuildCreator0(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new MissingMethodException($"类型 '{type.FullName}' 没有无参构造函数。");
            return Expression.Lambda<Func<object>>(Expression.New(ctor)).Compile();
        }

        // ==================== 1 个参数 ====================
        public static object Create<TArg1>(Type type, TArg1 arg1)
        {
            string key = $"{type.AssemblyQualifiedName}|{typeof(TArg1).AssemblyQualifiedName}";
            var creator = (Func<TArg1, object>)_cache.GetOrAdd(key, _ => BuildCreator1<TArg1>(type));
            return creator(arg1);
        }

        private static Func<TArg1, object> BuildCreator1<TArg1>(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(TArg1) });
            if (ctor == null)
                throw new MissingMethodException($"类型 '{type.FullName}' 缺少参数为 ({typeof(TArg1)}) 的构造函数。");
            var p1 = Expression.Parameter(typeof(TArg1), "arg1");
            return Expression.Lambda<Func<TArg1, object>>(
                Expression.New(ctor, p1), p1).Compile();
        }

        // ==================== 2 个参数 ====================
        public static object Create<TArg1, TArg2>(Type type, TArg1 arg1, TArg2 arg2)
        {
            string key = $"{type.AssemblyQualifiedName}|{typeof(TArg1).AssemblyQualifiedName}|{typeof(TArg2).AssemblyQualifiedName}";
            var creator = (Func<TArg1, TArg2, object>)_cache.GetOrAdd(key, _ => BuildCreator2<TArg1, TArg2>(type));
            return creator(arg1, arg2);
        }

        private static Func<TArg1, TArg2, object> BuildCreator2<TArg1, TArg2>(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(TArg1), typeof(TArg2) });
            if (ctor == null)
                throw new MissingMethodException($"类型 '{type.FullName}' 缺少参数为 ({typeof(TArg1)}, {typeof(TArg2)}) 的构造函数。");
            var p1 = Expression.Parameter(typeof(TArg1), "arg1");
            var p2 = Expression.Parameter(typeof(TArg2), "arg2");
            return Expression.Lambda<Func<TArg1, TArg2, object>>(
                Expression.New(ctor, p1, p2), p1, p2).Compile();
        }

        // ==================== 3 个参数 ====================
        public static object Create<TArg1, TArg2, TArg3>(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            string key = $"{type.AssemblyQualifiedName}|{typeof(TArg1).AssemblyQualifiedName}|{typeof(TArg2).AssemblyQualifiedName}|{typeof(TArg3).AssemblyQualifiedName}";
            var creator = (Func<TArg1, TArg2, TArg3, object>)_cache.GetOrAdd(key, _ => BuildCreator3<TArg1, TArg2, TArg3>(type));
            return creator(arg1, arg2, arg3);
        }

        private static Func<TArg1, TArg2, TArg3, object> BuildCreator3<TArg1, TArg2, TArg3>(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) });
            if (ctor == null)
                throw new MissingMethodException($"类型 '{type.FullName}' 缺少参数为 ({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}) 的构造函数。");
            var p1 = Expression.Parameter(typeof(TArg1), "arg1");
            var p2 = Expression.Parameter(typeof(TArg2), "arg2");
            var p3 = Expression.Parameter(typeof(TArg3), "arg3");
            return Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(
                Expression.New(ctor, p1, p2, p3), p1, p2, p3).Compile();
        }

        // ==================== 4 个参数 ====================
        public static object Create<TArg1, TArg2, TArg3, TArg4>(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            string key = $"{type.AssemblyQualifiedName}|{typeof(TArg1).AssemblyQualifiedName}|{typeof(TArg2).AssemblyQualifiedName}|{typeof(TArg3).AssemblyQualifiedName}|{typeof(TArg4).AssemblyQualifiedName}";
            var creator = (Func<TArg1, TArg2, TArg3, TArg4, object>)_cache.GetOrAdd(key, _ => BuildCreator4<TArg1, TArg2, TArg3, TArg4>(type));
            return creator(arg1, arg2, arg3, arg4);
        }

        private static Func<TArg1, TArg2, TArg3, TArg4, object> BuildCreator4<TArg1, TArg2, TArg3, TArg4>(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) });
            if (ctor == null)
                throw new MissingMethodException($"类型 '{type.FullName}' 缺少参数为 ({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}) 的构造函数。");
            var p1 = Expression.Parameter(typeof(TArg1), "arg1");
            var p2 = Expression.Parameter(typeof(TArg2), "arg2");
            var p3 = Expression.Parameter(typeof(TArg3), "arg3");
            var p4 = Expression.Parameter(typeof(TArg4), "arg4");
            return Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, object>>(
                Expression.New(ctor, p1, p2, p3, p4), p1, p2, p3, p4).Compile();
        }

        // ==================== 5 个参数 ====================
        public static object Create<TArg1, TArg2, TArg3, TArg4, TArg5>(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            string key = $"{type.AssemblyQualifiedName}|{typeof(TArg1).AssemblyQualifiedName}|{typeof(TArg2).AssemblyQualifiedName}|{typeof(TArg3).AssemblyQualifiedName}|{typeof(TArg4).AssemblyQualifiedName}|{typeof(TArg5).AssemblyQualifiedName}";
            var creator = (Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>)_cache.GetOrAdd(key, _ => BuildCreator5<TArg1, TArg2, TArg3, TArg4, TArg5>(type));
            return creator(arg1, arg2, arg3, arg4, arg5);
        }

        private static Func<TArg1, TArg2, TArg3, TArg4, TArg5, object> BuildCreator5<TArg1, TArg2, TArg3, TArg4, TArg5>(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) });
            if (ctor == null)
                throw new MissingMethodException($"类型 '{type.FullName}' 缺少参数为 ({typeof(TArg1)}, {typeof(TArg2)}, {typeof(TArg3)}, {typeof(TArg4)}, {typeof(TArg5)}) 的构造函数。");
            var p1 = Expression.Parameter(typeof(TArg1), "arg1");
            var p2 = Expression.Parameter(typeof(TArg2), "arg2");
            var p3 = Expression.Parameter(typeof(TArg3), "arg3");
            var p4 = Expression.Parameter(typeof(TArg4), "arg4");
            var p5 = Expression.Parameter(typeof(TArg5), "arg5");
            return Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>>(
                Expression.New(ctor, p1, p2, p3, p4, p5), p1, p2, p3, p4, p5).Compile();
        }
    }
}
