using System;
using System.Collections.Generic;
using UnityEngine;

public static class Pools
{
    public const string DefaultPoolName = "";
    private static readonly Dictionary<(Type, string), object> TypeToPoolMap = new Dictionary<(Type, string), object>();

    public static void New<T>(GameObject prefab, Transform parent, int capacity, string name = DefaultPoolName)
        where T : Component
    {
        if (TypeToPoolMap.ContainsKey((typeof(T), name)))
        {
            return;
        }

        TypeToPoolMap.Add((typeof(T), name), new Pool<T>(prefab, parent, capacity));
    }

    public static Pool<T> Get<T>(string name = DefaultPoolName)
        where T : Component
    {
        return TypeToPoolMap[(typeof(T), name)] as Pool<T>;
    }

    public static void Destroy<T>(string name = DefaultPoolName)
        where T : Component
    {
        if (!TypeToPoolMap.ContainsKey((typeof(T), name)))
        {
            return;
        }

        (TypeToPoolMap[(typeof(T), name)] as Pool<T>).Destroy();
        TypeToPoolMap.Remove((typeof(T), name));
    }
}
