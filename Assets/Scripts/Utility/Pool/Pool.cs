using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A flexible sized object pool for reusing GameObject.
/// </summary>
/// <typeparam name="T">The component to be pooled.</typeparam>
public class Pool<T>
    where T : Component
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private Queue<T> available;
    private HashSet<T> occupied;

    public Pool(GameObject prefab, Transform parent, int capacity = 50)
    {
        this.prefab = prefab;
        this.parent = parent;
        available = new Queue<T>(capacity);
        occupied = new HashSet<T>();

        for (int i = 0; i < capacity; i++)
        {
            AddNewObject();
        }
    }

    public List<T> CurrentlyOccupied => new List<T>(occupied);

    public void AddNewObject()
    {
        GameObject obj = GameObject.Instantiate(prefab, parent);
        T component = obj.GetComponent<T>();
        obj.SetActive(false);
        available.Enqueue(component);
    }

    /// <summary>
    /// Get an object from the pool.
    /// Instantiate a new object and increase the pool's size if the pool is empty.
    /// </summary>
    /// <param name="newParent">Transform to parent the returned object to.
    /// Null means its parent is unchanged.</param>
    /// <returns>An object from the pool.</returns>
    public T Get(Transform newParent = null)
    {
        if (available.Count == 0)
        {
            AddNewObject();
        }

        T obj = available.Dequeue();
        occupied.Add(obj);

        if (newParent == null)
        {
            obj.transform.SetParent(newParent, true);
        }

        obj.gameObject.SetActive(true);

        return obj;
    }

    /// <summary>
    /// Return an object to the pool for reusing.
    /// </summary>
    /// <param name="obj">The object to return to the pool.</param>
    public void Return(T obj)
    {
        if (obj == null)
        {
            return;
        }

        occupied.Remove(obj);
        available.Enqueue(obj);

        obj.transform.SetParent(parent, true);
        obj.gameObject.SetActive(false);
    }

    /// <summary>
    /// Forcibly return all objects to the pool.
    /// </summary>
    public void ReturnAll()
    {
        foreach (T obj in occupied)
        {
            available.Enqueue(obj);
            obj.transform.SetParent(parent, true);
            obj.gameObject.SetActive(false);
        }

        occupied.Clear();
    }

    /// <summary>
    /// Destroy the pool and all objects.
    /// </summary>
    public void Destroy()
    {
        foreach (T obj in occupied)
        {
            GameObject.Destroy(obj);
        }

        foreach (T obj in available)
        {
            GameObject.Destroy(obj);
        }

        available = null;
        occupied = null;
    }
}