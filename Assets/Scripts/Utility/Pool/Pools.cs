using System.Collections.Generic;
using UnityEngine;

public static class Pools
{
    private static readonly Dictionary<string, object> NameToPoolMap = new Dictionary<string, object>();

    public static void New<T>(string name, GameObject prefab, Transform parent, int capacity)
        where T : Component
    {
        if (NameToPoolMap.ContainsKey(name))
        {
            return;
        }

        NameToPoolMap.Add(name, new Pool<T>(prefab, parent, capacity));
    }

    public static Pool<T> Get<T>(string name)
        where T : Component
    {
        return NameToPoolMap[name] as Pool<T>;
    }

    public static void Destroy<T>(string name)
        where T : Component
    {
        if (!NameToPoolMap.ContainsKey(name))
        {
            return;
        }

        (NameToPoolMap[name] as Pool<T>).Destroy();
        NameToPoolMap.Remove(name);
    }
}
