using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSettings
{

    public static Texture2D noiseSource;

    public const int chunkSizeX = 5, chunkSizeZ = 5;

    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.86602540378f;

    public const float solidFactor = 0.8f;
    public const float blendFactor = 1 - solidFactor;

    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    public const float elevationStep = 3f;

    public const float cellPerturbStrength = 4f;
    public const float elevationPerturbStrength = 1.5f;
    public const float noiseScale = 0.003f;

    public const float waterElevationOffset = -0.5f;
    public const float waterFactor = 0.8f;
    public const float waterBlendFactor = 1 - waterFactor;


    readonly static Vector3[] corners =
    {
        new Vector3(0f, 0, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0, outerRadius)
    };

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return GetFirstCorner(direction) * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return GetSecondCorner(direction) * solidFactor;
    }

    public static Vector3 GetFirstWaterCorner(HexDirection direction)
    {
        return GetFirstCorner(direction) * waterFactor;
    }

    public static Vector3 GetSecondWaterCorner(HexDirection direction)
    {
        return GetSecondCorner(direction) * waterFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (GetFirstCorner(direction) + GetSecondCorner(direction)) * blendFactor;
    }

    public static Vector3 GetWaterBridge(HexDirection direction)
    {
        return (GetFirstCorner(direction) + GetSecondCorner(direction)) * waterBlendFactor;
    }

    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * HexSettings.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        float v = ((step + 1) / 2) * HexSettings.verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;

        return a;
    }

    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * HexSettings.horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    public static HexEdgeType GetEdgeType(int elevationLhs, int elevationRhs)
    {
        if (elevationLhs == elevationRhs)
        {
            return HexEdgeType.Flat;
        }
        int delta = elevationRhs - elevationLhs;
        if (delta == -1 || delta == 1)
            return HexEdgeType.Slope;
        return HexEdgeType.Cliff;
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(
            position.x * noiseScale, 
            position.z * noiseScale);
    }

    public static Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = HexSettings.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * HexSettings.cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * HexSettings.cellPerturbStrength;
        return position;
    }
}
