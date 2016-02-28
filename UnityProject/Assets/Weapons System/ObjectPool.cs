using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IPoolable
{
    int PoolIndex
    {
        get;
        set;
    }
}

public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private T[] objects;
    private int size;
    private int nextAvailable = 0;

    public int ObjectsAvailable
    {
        get { return size - nextAvailable; }
    }

    public ObjectPool(int objectCount, T prefab)
    {
        size = objectCount;

        // We will use an extra space at the very end of the array
        // as a temp holder when swapping the positions of objects.
        objects = new T[size + 1];

        // Initialize the array with a set of new objects
        for(int i = 0; i < size; ++i)
        {
            if (prefab != default(T))
            {
                objects[i] = GameObject.Instantiate(prefab);
            }
            else
            {
                GameObject gameObject = new GameObject();
                objects[i] = gameObject.AddComponent<T>();
            }

            objects[i].gameObject.SetActive(false);
        }
    }

    private void swap(int a, int b)
    {
        objects[size] = objects[a];
        objects[a] = objects[b];
        objects[b] = objects[size];
    }

    public T Allocate()
    {
        if (nextAvailable >= size)
            return default(T);

        objects[nextAvailable].PoolIndex = nextAvailable;
        return objects[nextAvailable++];
    }

    public void Deallocate(T obj)
    {
        objects[nextAvailable - 1].PoolIndex = obj.PoolIndex;
        swap(obj.PoolIndex, nextAvailable - 1);
        --nextAvailable;
    }
}
