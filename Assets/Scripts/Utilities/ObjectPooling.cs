using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling<T> where T : MonoBehaviour, IPoolable
{
    private Stack<T> _pooledObjects;
    private T _loadedObject;

    public ObjectPooling(string path, int initialSize)
    {
        _pooledObjects = new Stack<T>(initialSize);
        _loadedObject = Resources.Load<T>(path);

        for (int i = 0; i < initialSize; i++)
        {
            T obj = InstantiateObject();
            _pooledObjects.Push(obj);
        }
    }

    public T Rent()
    {
        T obj;

        if (!_pooledObjects.TryPop(out obj))
        {
            obj = InstantiateObject();
        }

        obj.SetDisableCallbackAction(this.TurnBack);
        return obj;
    }

    public void TurnBack(IPoolable obj)
    {
        T TObj = (T)obj;
        TObj.gameObject.SetActive(false);
        _pooledObjects.Push(TObj);
    }

    private T InstantiateObject()
    {
        T obj = UnityEngine.Object.Instantiate(_loadedObject);
        obj.gameObject.SetActive(false);
        return obj;
    }
}

public interface IPoolable
{
    void SetDisableCallbackAction(Action<IPoolable> callback);
}
