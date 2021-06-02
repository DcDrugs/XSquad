using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListPool<T>
{
    static Stack<List<T>> stack = new Stack<List<T>>();


    public static void Add(List<T> list)
    {
        list.Clear();
        stack.Push(list);
    }

    public static List<T> Get()
    {
        if(stack.Count > 0)
            return stack.Pop();
        else
            return new List<T>();
    }
}
