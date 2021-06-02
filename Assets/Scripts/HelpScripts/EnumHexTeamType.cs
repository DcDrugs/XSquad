using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamTypes
{
    BLUE,
    RED
}

public static class TeamTypesExtensions
{
    public static TeamTypes ChangeTeam(this TeamTypes type)
    {
        return (type == TeamTypes.RED) ? TeamTypes.BLUE : TeamTypes.RED;
    }
}

