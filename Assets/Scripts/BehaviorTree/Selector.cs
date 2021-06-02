using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class Selector : Node
{
    protected List<Node> nodes = new List<Node>();

    public Selector(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeStates Evaluate()
    {
        foreach(Node node in nodes)
        {
            switch(node.Evaluate())
            {
                case NodeStates.FAILURE:
                    continue;

                case NodeStates.SUCCESS:
                    NodeState = NodeStates.SUCCESS;
                    return NodeState;

                case NodeStates.RUNNING:
                    NodeState = NodeStates.RUNNING;
                    return NodeState;

                default:
                    continue;
            }
        }
        NodeState = NodeStates.FAILURE;
        return NodeState;
    }
}
