using System;
using UnityEngine;

public class ParticlePool<T>
    where T : Component
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private T[] pool;
    private int index = 0;

    public ParticlePool(GameObject prefab, Transform parent, int poolSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new T[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = UnityEngine.Object.Instantiate(prefab, parent);
            pool[i] = go.GetComponent<T>();
        }
    }

    public T Get()
    {
        T result = pool[index];
        index += 1;
        if (index >= pool.Length)
        {
            index = 0;
        }

        return result;
    }

    public void Destroy()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            UnityEngine.Object.Destroy(pool[i].gameObject);
        }

        pool = new T[0];
    }

    public void Resize(int newPoolSize)
    {
        if (newPoolSize > pool.Length)
        {
            T[] newPool = new T[newPoolSize];
            Array.Copy(pool, newPool, pool.Length);
            for (int i = pool.Length; i < newPoolSize; i++)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab, parent);
                newPool[i] = go.GetComponent<T>();
            }

            pool = newPool;
        }
        else if (newPoolSize < pool.Length)
        {
            T[] newPool = new T[newPoolSize];
            Array.Copy(pool, newPool, newPoolSize);
            for (int i = newPoolSize; i < pool.Length; i++)
            {
                UnityEngine.Object.Destroy(pool[i].gameObject);
            }

            pool = newPool;
        }
    }
}