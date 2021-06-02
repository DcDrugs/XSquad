using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SceneController<TController, TArgs> : MonoBehaviour
    where TController : SceneController<TController, TArgs>
    where TArgs :SceneArgs, new()
{
    protected TArgs Args { get; private set; }

    private void Start()
    {
        Args = SceneManager.GetArgs<TController, TArgs>();
        OnStart();
    }

    protected virtual void OnStart() { }
}
