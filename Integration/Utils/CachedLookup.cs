using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Efeu.Integration.Utils;

internal class CachedLookup<TKey, TValue> where TKey : notnull
{
    private Dictionary<TKey, TValue> cache = new Dictionary<TKey, TValue>();

    private Func<TKey[], Task<TValue[]>> fetch;

    private Func<TValue, TKey> getKey;

    public CachedLookup(Func<TKey[], Task<TValue[]>> fetch, Func<TValue, TKey> getKey)
    {
        this.fetch = fetch;
        this.getKey = getKey;
    }

    public CachedLookup(TValue[] items, Func<TKey[], Task<TValue[]>> fetch, Func<TValue, TKey> getKey)
    {
        this.fetch = fetch;
        this.getKey = getKey;
        foreach (TValue item in items)
        {
            cache.Add(getKey(item), item);
        }
    }

    public async Task<TValue[]> GetAsync(TKey[] keys)
    {
        IEnumerable<TKey> missingKeys = keys.Where(i => !cache.ContainsKey(i));
        if (missingKeys.Any())
        {
            TValue[] missingValues = await fetch(missingKeys.ToArray());
            foreach (TValue value in missingValues)
            {
                cache.Add(getKey(value), value);
            }
        }

        return keys.Select(i => cache[i]).ToArray();
    }

    public async Task<TValue> GetAsync(TKey key)
    {
        TValue[] result = await GetAsync([key]);
        return result.First();
    }

    public TValue GetCached(TKey key)
    {
        return cache[key];
    }

    public void Inject(TKey key, TValue value)
    {
        cache[key] = value;
    }
}