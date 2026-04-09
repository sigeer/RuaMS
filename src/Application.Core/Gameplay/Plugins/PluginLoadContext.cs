using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Application.Core.Plugins
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginPath;
        private readonly AssemblyDependencyResolver _resolver;


        public PluginLoadContext(string pluginPath) : base(isCollectible: true)
        {
            var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "p_tmp", Path.GetFileName(pluginPath));
            var dirPath = Path.GetDirectoryName(destPath)!;
            Directory.CreateDirectory(dirPath);
            File.Copy(pluginPath, destPath, overwrite: true);

            _pluginPath = destPath;
            _resolver = new AssemblyDependencyResolver(destPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // 1. 优先尝试从默认上下文（宿主）中加载
            try
            {
                // Assembly.Load 会在默认上下文中解析（因为当前线程的默认上下文是宿主）
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                    return assembly;
            }
            catch
            {
                // 宿主中没有此程序集，继续往下走
            }

            // 2. 如果宿主中没有，尝试从插件目录解析（例如插件自带的私有依赖）
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null && File.Exists(path))
            {
                return LoadFromAssemblyPath(path);
            }

            // 3. 都找不到，返回 null 让 CLR 触发 FileNotFoundException
            return null;
        }
    }
}
