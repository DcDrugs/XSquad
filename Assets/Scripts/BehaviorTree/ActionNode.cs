using System.Collections;
using UnityEngine;

public class ActionNode : Node
{
    public delegate NodeStates ActionNodeDelegate();

    private ActionNodeDelegate action;

    public ActionNode(ActionNodeDelegate action)
    {
        this.action = action;
    }

    public override NodeStates Evaluate()
    {
        switch (action())
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

            default:
                NodeState = NodeStates.FAILURE;
                return NodeState;
        }
    }
}