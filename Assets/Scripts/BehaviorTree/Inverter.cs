using System.Collections;
using UnityEngine;

public class Inverter : Node
{
    public Node NodeItem { get; set; }

    public Inverter(Node node)
    {
        NodeItem = node;
    }

    public override NodeStates Evaluate()
    {
        switch (NodeItem.Evaluate())
        {
            case NodeStates.FAILURE:
                NodeState = NodeStates.FAILURE;
                return NodeState;

            case NodeStates.SUCCESS:
                NodeState = NodeStates.SUCCESS;
                return NodeState;

            case NodeStates.RUNNING:
                NodeState = NodeStates.RUNNING;
                return NodeState;
        }
        NodeState = NodeStates.SUCCESS;
        return NodeState;
    }
}