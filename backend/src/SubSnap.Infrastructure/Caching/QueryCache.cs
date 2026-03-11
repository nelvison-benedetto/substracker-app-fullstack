using System.Collections.Concurrent;

namespace SubSnap.Infrastructure.Caching;

/*
Request-Scoped Cache: vive solo durante una singola http req, dura 50-300ms, serve ad evitare che magari nella http req chiami 3 volte esempio l'user target, allora non fa 3 loads dell'user ma fa solo 1(la prima volta) e le altre 2 sfrutta la cache.
Aumenta del 40% le performances delle http reqs!
LO UTILIZZO IN TUTTI I LOADS BATCHERS PROJECTIONS!!
 */
public class QueryCache
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    public bool TryGet<T>(string key, out T value)
    {
        if (_cache.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }
    public void Set<T>(string key, T value)
    {
        _cache[key] = value!;
    }

}
