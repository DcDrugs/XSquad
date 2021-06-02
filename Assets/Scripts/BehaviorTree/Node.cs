using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node 
{
    public delegate NodeStates NodeReturn();

    public NodeStates NodeState { get; protected set; }

    public Node() { }

    public abstract NodeStates Evaluate();
}

