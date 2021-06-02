using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class HexMapEditor : MonoBehaviour
{
    static HexMapEditor instance;
    public HexGrid grid;

    public Material terrainMaterial;

    private int activeElevation;
    private int activeWaterLevel;

    bool applyElevation = true;
    bool applyWaterLevel = true;

    int brushSize;

    int activeTerrainTypeIndex;

    void Awake()
    {
        instance = this;
        terrainMaterial.DisableKeyword("GRID_ON");
        SetEditMode(false);
    }

    public static HexMapEditor Instance()
    {
        return instance;
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
            {
                HandleInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyUnit();
                }
                else
                {
                    CreateUnit();
                    HexGrid.Instance().WakeUpAllEditor();
                }
                return;
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DestroyFeature();
                }
                else
                {
                    CreateFeature();
                }
                return;
            }
            return;
        }
    }

    void HandleInput()
    {
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell == null)
            return;
        EditCells(currentCell);
    }

    HexCell GetCellUnderCursor()
    {
        return grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;
        
        for(int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for(int x = centerX - r; x <=  centerX + brushSize; x++)
            {
                EditCell(grid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(grid.GetCell(new HexCoordinates(x, z)));
            }
        }

    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (activeTerrainTypeIndex >= 0)
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            if (applyElevation)
                cell.Elevation = activeElevation;
            if (applyWaterLevel)
                cell.WaterLevel = activeWaterLevel;
        }
    }

    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit && !cell.Feature)
        {
            HexUnit.CreateUnit(grid, HexUnit.unitPrefab, GunManager.GetRandomGun(), cell, Random.Range(0f, 360f), grid.Team, HexUnit.MaxAction);
        }
    }

    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            grid.RemoveUnit(cell.Unit, null);
        }
    }


    void CreateFeature()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit && !cell.Feature)
        {
            var feature = HexFeature.CreateRandomFeature(Random.Range(0f, 360f)); // HexFeature.CreateFeature(EnumFeature.WOODBOX4, Random.Range(0f, 360f)); // ИСПРАВИТЬ
            HexFeature.AddFeatureOnGrid(grid, feature, cell);
        }
    }

    void DestroyFeature()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Feature)
        {
            grid.RemoveObject(cell.Feature);
        }
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }
    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    public void SetEditMode(bool toggle)
    {
        enabled = toggle;
        if (toggle)
        {
            Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
            HexGrid.Instance().WakeUpAllEditor();
        }
        else
        {
            Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
            if (HexGrid.Instance() != null)
                HexGrid.Instance().SleepAllEditor();
        }
    }
}
