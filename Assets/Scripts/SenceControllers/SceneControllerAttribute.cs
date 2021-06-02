using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SceneControllerAttribute : Attribute
{
   public string SceneName { get; private set; }

    public SceneControllerAttribute(string name)
    {
        SceneName = name;
    }
}
