using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class Sequence : Node
{
    private List<Node> nodes = new List<Node>();
    
    public Sequence(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeStates Evaluate()
    {
        bool anyChildRunning = false;

        foreach(Node node in nodes)
        {
            switch(node.Evaluate())
            {
                case NodeStates.FAILURE:
                    NodeState = NodeStates.FAILURE;
                    return NodeState;

                case NodeStates.SUCCESS:
                    continue;

                case NodeStates.RUNNING:
                    anyChildRunning = true;
                    continue;

                default:
                    NodeState = NodeStates.SUCCESS;
                    return NodeState;
            }
        }
        NodeState = anyChildRunning ? NodeStates.RUNNING : NodeStates.SUCCESS;
        return NodeState;

    }
}
