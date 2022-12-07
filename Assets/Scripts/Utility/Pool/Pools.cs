using System.Collections.Generic;
using UnityEngine;

public static class Pools
{
    private static readonly Dictionary<string, object> NameToPoolMap = new Dictionary<string, object>();

    /// <summary>
    /// Create a new global pool.
    /// </summary>
    /// <param name="name">The name of the pool. Used for retrieval.</param>
    /// <param name="prefab">The prefab GameObject of the pool.</param>
    /// <param name="parent">The parent transform.</param>
    /// <param name="capacity">The initial capacity of the pool.</param>
    /// <typeparam name="T">The component to pool.</typeparam>
    /// <returns>The newly created pool, or an already existing pool if it has been created already.</returns>
    public static Pool<T> New<T>(string name, GameObject prefab, Transform parent, int capacity)
        where T : Component
    {
        if (NameToPoolMap.ContainsKey(name))
        {
            return NameToPoolMap[name] as Pool<T>;
        }

        NameToPoolMap.Add(name, new Pool<T>(prefab, parent, capacity));
        return NameToPoolMap[name] as Pool<T>;
    }

    /// <summary>
    /// Retrieve a pool.
    /// </summary>
    /// <param name="name">The name of the pool.</param>
    /// <typeparam name="T">The component type of the pool.</typeparam>
    /// <returns>Retrieved pool, or null if the provided type <see cref="{T}"/> is incorrect.</returns>
    public static Pool<T> Get<T>(string name)
        where T : Component
    {
        return NameToPoolMap[name] as Pool<T>;
    }

    /// <summary>
    /// Destroy the pool.
    /// </summary>
    /// <param name="name">The name of the pool.</param>
    /// <typeparam name="T">The component type of the pool.</typeparam>
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
