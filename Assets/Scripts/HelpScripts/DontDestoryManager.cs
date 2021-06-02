using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestoryManager : MonoBehaviour
{
    static DontDestoryManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Object.DontDestroyOnLoad(this);
        }
    }
}
