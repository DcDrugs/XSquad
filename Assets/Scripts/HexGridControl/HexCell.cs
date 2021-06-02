using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    int terrainTypeIndex;
    private int elevation = int.MinValue;
    [SerializeField]
    HexCell[] neighbors;
    public RectTransform uiRect;
    public HexGridChunk chunk;
    public HexCellShaderData ShaderData { get; set; }
    int waterLevel;
    int distance;
    int visibility;

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)(elevation + 127));
        writer.Write((byte)waterLevel);
    }

    public void Load(BinaryReader reader)
    {
        terrainTypeIndex = reader.ReadByte();
        ShaderData.RefreshTerrain(this);
        elevation = reader.ReadByte() - 127;
        RefrashPosition();
        waterLevel = reader.ReadByte();
    }

    public int Index { get; set; }
    public HexUnit Unit { get; set; }
    public HexFeature Feature { get; set; }
    public HexCell PathFrom { get; set; }
    public int SearchHeuristic { get; set; }

    public int SearchPhase { get; set; }

    public HexCell NextWithSamePriority { get; set; }

    public bool IsVisible
    {
        get
        {
            return visibility > 0;
        }
    }

    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }

    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
                return;
            waterLevel = value;
            Refresh();
        }
    }

    public float WaterSurfaceY
    {
        get
        {
            return
                (waterLevel + HexSettings.waterElevationOffset) *
                HexSettings.elevationStep;
        }
    }

    public bool IsUnderWater
    {
        get
        {
            return waterLevel > elevation;
        }
    }

    public int TerrainTypeIndex
    {
        get
        {
            return terrainTypeIndex;
        }

        set
        {
            if (terrainTypeIndex != value)
            {
               terrainTypeIndex = value;
               ShaderData.RefreshTerrain(this);
            }
        }
    }

    public void IncreaseVisibility()
    {
        visibility++;
        if (visibility == 1)
            ShaderData.RefreshVisibility(this);
    }

    public void DecreaseVisibility()
    {
        visibility--;
        if (visibility == 0)
            ShaderData.RefreshVisibility(this);
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            if (Unit)
            {
                Unit.ValidateLocation();
            }
            if (Feature)
            {
                Feature.ValidateLocation();
            }
            for (int i = 0; i < neighbors.Length; i++)          // исправить
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
                return;

            elevation = value;

            RefrashPosition();
            Refresh();
        }
    }

    public Vector3 LocalPosition
    {
        get
        {
            return transform.localPosition;
        }
    }

    public Vector3 GlobalPosition
    {
        get
        {
            return transform.position;
        }
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexSettings.GetEdgeType(elevation, GetNeighbor(direction).elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexSettings.GetEdgeType(elevation, otherCell.elevation);
    }

    void RefrashPosition()
    {
        Vector3 position = transform.localPosition;
        position.y = elevation * HexSettings.elevationStep;
        position.y += (HexSettings.SampleNoise(position).y * 2f - 1f) * HexSettings.elevationPerturbStrength;
        transform.localPosition = position;

        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -position.y;
        uiRect.localPosition = uiPosition;
    }
    public void SetLabel(string text)
    {
        Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }

    public void DisableHighlight()
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    public void EnableHighlight(Color color)
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    public bool IsFree()
    {
        return !IsUnderWater && Feature == null && Unit == null;
    }
}
