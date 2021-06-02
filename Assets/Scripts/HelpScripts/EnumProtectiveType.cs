using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProtectiveType
{
    NONE,
    HALF,
    FULL
}


public static class ProtectionTypeExtensions
{
    public static float GetProtection(this ProtectiveType type)
    {
        if (type == ProtectiveType.NONE)
            return 0f;
        else if (type == ProtectiveType.HALF)
            return 0.2f;
        else if (type == ProtectiveType.FULL)
            return 0.5f;
        throw new System.ArgumentException("Protection type not found!");
    }
}
