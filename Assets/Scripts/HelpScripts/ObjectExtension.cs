using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ObjectExtension
{
    private static List<Object> savedObjects = new List<Object>();

    public static void DontDestroyOnLoad(this Object obj)
    {
        savedObjects.Add(obj);
        Object.DontDestroyOnLoad(obj);
    }

    public static void Destroy(this Object obj)
    {
        savedObjects.Remove(obj);
        Object.Destroy(obj);
    }

    public static void Remove(this Object obj)
    {
        savedObjects.Remove(obj);
    }

    public static void Remove<T>(this IEnumerable<T> obj) where T : Object
    {
        if (obj != null)
        {
            var enu = obj.GetEnumerator();
            enu.Reset();
            while (enu.MoveNext())
                Remove(enu.Current);
        }
    }

    public static List<T> GetSavedObjects<T>()
    {
        return new List<T>(savedObjects.OfType<T>());
    }

    public static void Clear()
    {
        foreach (Object obj in savedObjects)
        {
            Object.Destroy(obj);
        }
        savedObjects.Clear();
    }
}
