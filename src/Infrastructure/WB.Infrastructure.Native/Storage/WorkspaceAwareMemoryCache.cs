using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WB.Infrastructure.Native.Storage
{
    // singleton
    public class WorkspaceAwareMemoryCache : IMemoryCacheSource
    {
        ConcurrentDictionary<string, IMemoryCache> caches = new ConcurrentDictionary<string, IMemoryCache>(); 
        
        public IMemoryCache GetCache(string workspace)
        {
            return caches.GetOrAdd(workspace, _ => new MemoryCache(Options.Create(new MemoryCacheOptions())));
        }
    }

    public interface IMemoryCacheSource
    {
        IMemoryCache GetCache(string workspace);
    }
}
