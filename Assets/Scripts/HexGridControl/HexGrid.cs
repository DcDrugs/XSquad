using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    static HexGrid instance;

    int action = 0;
    bool playMode = true;
    public int AllAction
    {
        get
        {
            return action;
        }
        set
        {
            action = value;
            if (action <= 0)
                Team = team.ChangeTeam();
        }
    }

    public int cellCountX = 20, cellCountZ = 15;

    int chunkCountX, chunkCountZ;

    public HexGameUI gameUI;

    public HexCell cellPrefub;
    public Text cellLabelPrefab;
    public Texture2D noiseSource;
    public HexGridChunk chunkPrefab;
    public HexUnit unitPrefab;

    HexCellShaderData cellShaderData;

    HexGridChunk[] chunks;
    HexCell[] cells;
    readonly List<HexUnit> units = new List<HexUnit>();
    readonly List<HexFeature> features = new List<HexFeature>();

    HexCellPriorityQueue searchFrontier;
    HexCell currentPathFrom, currentPathTo;
    int searchFrontierPhase;

    bool IsGame
    {
        get
        {
            return (HexMapEditor.Instance() == null);
        }
    }


    TeamTypes team = TeamTypes.BLUE;

    private void Awake()
    {
        instance = this;
    }

    public void InstanceFromController()
    {
        HexSettings.noiseSource = noiseSource;
        HexUnit.unitPrefab = unitPrefab;
        if (cellShaderData == null)
            cellShaderData = gameObject.AddComponent<HexCellShaderData>();
    }

    public HexCell GetRandomCell(int numberOfRegion)
    {
        HexCell cell = Region.GetRandomCell(this, numberOfRegion);
        while (!cell.IsFree()) 
        {
            cell = cell.GetNeighbor((HexDirection)Random.Range((int)HexDirection.E, (int)HexDirection.W));
            if (cell == null)
                cell = Region.GetRandomCell(this, numberOfRegion);
        }
        return cell;
    }

    public void CreateMission(List<HexUnit> freadlyUnits, List<HexUnit> enemyUnits)
    {
        foreach (HexUnit unit in freadlyUnits)
        {

            HexUnit.AddUnitOnGrid(this, unit, GetRandomCell(0));
        }

        int i = 4;
        int location = 0;
        foreach (HexUnit unit in enemyUnits)
        {
            if (i > 3)
            {
                location = Random.Range(3, Region.countRegions);
                i = 0;
            }
            HexUnit.AddUnitOnGrid(this, unit, GetRandomCell(location));
            i++;
        }
        Team = TeamTypes.BLUE;
    }
    public static HexGrid Instance()
    {
        return instance;
    }
    public bool HasPath
    {
        get
        {
            return currentPathFrom != null && currentPathTo != null;
        }
    }

    public TeamTypes Team
    {
        get
        {
            return team;
        }
        set
        {
            team = value;
            Sleep(team.ChangeTeam());
        }
    }

    public void ChangeTeam(bool isblue)
    {
        if (isblue)
            Team = TeamTypes.BLUE;
        else
            Team = TeamTypes.RED;
    }

    public void WakeUpAllEditor()
    {
        playMode = false;
        for (int i = 0; i < units.Count; i++)
        {
            units[i].EnableMesh();
        }
    }

    public void SleepAllEditor()
    {
        playMode = true;
        for (int i = 0; i < units.Count; i++)
        {
            units[i].DisenableMesh();
        }
        RefrashTeam();
    }

    public void Sleep(TeamTypes team, bool refrash = false)
    {
        bool originalImmediateMode = cellShaderData.ImmediateMode;
        cellShaderData.ImmediateMode = true;
        action = 0;
        for(int i = 0; i < units.Count; i++)
        {
            if (units[i].Team == team)
            {
                units[i].Sleep();
                if (units[i].LookerCount <= 0 && playMode)
                    units[i].DisenableMesh();
            }
            else
            {
                if(refrash == true)
                {
                    units[i].WakeUp(units[i].Action);
                }
                else
                    units[i].WakeUp(HexUnit.MaxAction);
                action += units[i].Action;
            }
        }
        cellShaderData.ImmediateMode = originalImmediateMode;
    }

    public void RefrashTeam()
    {
        bool originalImmediateMode = cellShaderData.ImmediateMode;
        cellShaderData.ImmediateMode = true;
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Sleep();
            units[i].EnableMesh();
        }
        Sleep(team.ChangeTeam(), true);
        cellShaderData.ImmediateMode = originalImmediateMode;
    }

    public HexCell GetCell(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return GetCell(hit.point);
        }
        return null;
    }

    public HexCell GetCell(int xOffset, int zOffset)
    {
        return cells[xOffset + zOffset * cellCountX];
    }

    public HexCell GetCell(int cellIndex)
    {
        return cells[cellIndex];
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(cellCountX);
        writer.Write(cellCountZ);
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Save(writer);
        }
        writer.Write(features.Count);
        for(int i = 0; i < features.Count; i++)
        {
            features[i].Save(writer);
        }
        writer.Write(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            units[i].Save(writer);
        }
        List<HexUnit> otherUnits = ObjectExtension.GetSavedObjects<HexUnit>();
        writer.Write(otherUnits.Count);
        for (int i = 0; i < otherUnits.Count; i++)
        {
            otherUnits[i].Save(writer);
        }

        writer.Write((short)Team);
    }

    public void Load(BinaryReader reader)
    {
        ClearPath();
        ClearObjects();
        if (!CreateMap(reader.ReadInt32(), reader.ReadInt32()))
            return;
        StopAllCoroutines();
        bool originalImmediateMode = cellShaderData.ImmediateMode;
        cellShaderData.ImmediateMode = true;
        foreach (HexCell cell in cells)
        {
            cell.Load(reader);
        }
        foreach(HexGridChunk chunk in chunks)
        {
            chunk.Refresh();
        }

        int featureCount = reader.ReadInt32();
        for (int i = 0; i < featureCount; i++)
        {
            HexFeature.Load(reader, this);
        }

        int unitCount = reader.ReadInt32();
        for (int i = 0; i < unitCount; i++)
        {
            HexUnit.Load(reader, this);
        }

        int otherUnitCount = reader.ReadInt32();
        for (int i = 0; i < otherUnitCount; i++)
        {
            ObjectExtension.DontDestroyOnLoad(HexUnit.Load(reader));
        }

        Team = (TeamTypes)reader.ReadInt16();
        cellShaderData.ImmediateMode = originalImmediateMode;
    }

    private void OnEnable()
    {
        if(!HexSettings.noiseSource)
        {
            HexSettings.noiseSource = noiseSource;
            HexUnit.unitPrefab = unitPrefab;
        }
    }

    public bool CreateMap(int x, int z)
    {
        if (x <= 0 || x % HexSettings.chunkSizeX != 0 ||
            z <= 0 || z % HexSettings.chunkSizeZ != 0)
        {
            Debug.LogError("Unsupported map size.");
            return false;
        }
        ClearPath();
        ClearObjects();
        if (chunks != null)
        {
            foreach (HexGridChunk chunk in chunks)
                Destroy(chunk.gameObject);
        }
        cellCountX = x;
        cellCountZ = z;

        chunkCountX = cellCountX / HexSettings.chunkSizeX;
        chunkCountZ = cellCountZ / HexSettings.chunkSizeZ;

        cellShaderData.Initialize(cellCountX, cellCountZ);
        CreateChunks();
        CreateCells();
        TriangulateChunks();
        return true;
    }

    void TriangulateChunks()
    {
        for(int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Refresh();
        }
    }

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }

    void CreateCells()
    {
        cells = new HexCell[cellCountX * cellCountZ];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
                CreateCell(x, z, i++);
        }
    }

    void CreateCell(int x, int z, int i)
    {
        var position
            = new Vector3(
                (x + z * 0.5f - z / 2) * (HexSettings.innerRadius * 2f),
                0f,
                z * (HexSettings.outerRadius * 1.5f));

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefub);
        cell.Index = i;
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinate(x, z);
        cell.ShaderData = cellShaderData;

        if(x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if(z > 0)
        {
            if((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if(x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if(x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }


        Text Label = Instantiate<Text>(cellLabelPrefab);
        Label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        //    Label.text = cell.coordiates.ToStringOnSeparateLines();
        Label.text = "";
        cell.uiRect = Label.rectTransform;

        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexSettings.chunkSizeX;
        int chunkZ = z / HexSettings.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexSettings.chunkSizeX;
        int localZ = z - chunkZ * HexSettings.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexSettings.chunkSizeX, cell);
    }
    public HexCell GetCell(HexCoordinates coordiates)
    {
        int z = coordiates.Z;
        if (z < 0 || z >= cellCountZ)
            return null;
        int x = coordiates.X + z / 2;
        if (x < 0 || x >= cellCountX)
            return null;
        return cells[x + z * cellCountX];
    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordiates = HexCoordinates.FromPosition(position);
        int index = coordiates.X + coordiates.Z * cellCountX + coordiates.Z / 2;
        return cells[index];
    }

    public List<HexCell> GetPath()
    {
        if (currentPathFrom == null || currentPathTo == null)
        {
            return null;
        }
        List<HexCell> path = ListPool<HexCell>.Get();
        for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
        {
            path.Add(c);
        }
        path.Add(currentPathFrom);
        path.Reverse();
        return path;
    }

    public void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        //StopAllCoroutines();
        //StartCoroutine(Search(fromCell, toCell, speed));
        ClearPath();
        (currentPathFrom, currentPathTo) = Search(fromCell, toCell, speed);
        ShowPath();
    }

    public void ClearPath()
    {
        if (currentPathFrom != null && currentPathTo != null)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.SetLabel(null);
            current.DisableHighlight();
        }
        currentPathFrom = currentPathTo = null;
    }

    void ShowPath()
    {
        if (currentPathFrom != null && currentPathTo != null)
        {
            HexCell path = currentPathTo;

            path.SetLabel(path.Distance.ToString());
            path = path.PathFrom;
            while (path != currentPathFrom)
            {
                path.EnableHighlight(Color.white);
                path.SetLabel(path.Distance.ToString());
                path = path.PathFrom;
            }
            path.SetLabel(path.Distance.ToString());
            currentPathFrom.EnableHighlight(Color.blue);
            currentPathTo.EnableHighlight(Color.red);
        }
    }

    (HexCell, HexCell) Search(HexCell fromCell, HexCell toCell, int speed)
    {
        searchFrontierPhase += 2;
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        SearchParams(searchFrontier, fromCell);
        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current.Distance >= speed || current == toCell)
            {
                HexCell path = current;

                while (path.Distance >= speed)
                {
                    path = path.PathFrom;
                }
                return (current == toCell)? (fromCell, path) : (null, null);
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
                    continue;

                if (neighbor.IsUnderWater || neighbor.Unit || neighbor.Feature)
                    continue;

                HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff)
                    continue;

                int moveCost = 1;

                int distance = current.Distance + moveCost;
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic =
                        neighbor.coordinates.DistanceTo(toCell.coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }

        return (null, null);
    }

    void SearchParams(HexCellPriorityQueue searchFrontier, HexCell startCell)
    {
        startCell.SearchPhase = searchFrontierPhase;
        startCell.Distance = 0;
        searchFrontier.Enqueue(startCell);
    }

    (List<HexCell>, List<HexUnit>) GetVisibleItems(HexCell fromCell, int range, TeamTypes type, HexCell cell = null)
    {
        List<HexCell> visibleCells = ListPool<HexCell>.Get();
        List<HexUnit> visibleUnits = ListPool<HexUnit>.Get();

        searchFrontierPhase += 2;
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        SearchParams(searchFrontier, (cell != null) ? cell : fromCell);
        HexCoordinates fromCoordinates = fromCell.coordinates;
        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;
            if (current.Unit && current.Unit.Team != type)
                visibleUnits.Add(current.Unit);
            visibleCells.Add(current);

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
                    continue;

                if (cell != null && !neighbor.IsVisible)
                    continue;

                if (neighbor.Feature && neighbor.Feature.protective == ProtectiveType.FULL)
                {
                    if (neighbor.Unit && neighbor.Unit.Team != type)
                        visibleUnits.Add(neighbor.Unit);
                    visibleCells.Add(neighbor);
                    continue;
                }

                int distance = current.Distance + 1;
                if (distance > range)
                    continue;

                
                if (distance > fromCoordinates.DistanceTo(neighbor.coordinates))
                    continue;


                HexEdgeType edgeType = fromCell.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff && fromCell.Elevation <= neighbor.Elevation)
                {
                    if (neighbor.Unit && neighbor.Unit.Team != type)
                        visibleUnits.Add(neighbor.Unit);
                    visibleCells.Add(neighbor);
                    continue;
                }

                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.SearchHeuristic = 0;
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }
        return (visibleCells, visibleUnits);
    }

    public List<HexUnit> GetVisibleEnemies(HexUnit unit, HexCell cell)
    {
        return GetVisibleItems(unit.Location, unit.VisionRange, unit.Team, cell).Item2;
    }

    public void IncreaseVisibility(HexUnit unit, HexCell cell, int range)
    {
       (List<HexCell> cells, List<HexUnit> enemies) = GetVisibleItems(cell, range, unit.Team);
        for(int i =0; i < cells.Count; i++)
        {
            cells[i].IncreaseVisibility();
        }
        unit.Enemies = enemies;
        gameUI.EraseEnemies();
        gameUI.ShowEnemies(unit);
        ListPool<HexCell>.Add(cells);
        ListPool<HexUnit>.Add(enemies);
    }

    public void DecreaseVisibility(HexUnit unit, HexCell cell, int range)
    {
        (List<HexCell> cells, List<HexUnit> enemies) = GetVisibleItems(cell, range, unit.Team);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].DecreaseVisibility();
        }
        unit.Enemies = enemies;
        gameUI.EraseEnemies();
        ListPool<HexCell>.Add(cells);
        ListPool<HexUnit>.Add(enemies);
    }

    public void ShowUI(bool visible)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }

    public void AddUnit(HexUnit unit, HexCell location)
    {
        units.Add(unit);
        unit.transform.SetParent(transform, false);
        unit.Location = location;
 
            if (unit.Team == Team)
            unit.WakeUp(HexUnit.MaxAction);
        else
            unit.Action = unit.Action;
    }

    public void AddFeature(HexFeature feature, HexCell location)
    {
        features.Add(feature);
        feature.transform.SetParent(transform, false);
        feature.Location = location;
        location.Feature = feature;
    }

    public void RemoveUnit(HexUnit unit, HexUnit killer = null)
    {
        units.Remove(unit);
        if (killer == null)
            unit.DieYourSelf();
        else
            unit.DieBy(killer);
        if (IsGame == true)
        {
            bool win = true;
            bool lose = true;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Team == TeamTypes.RED)
                    win = false;
                if (units[i].Team == TeamTypes.BLUE)
                    lose = false;
            }

            if (win || lose)
            {
                List<HexUnit> favorityUnits = new List<HexUnit>();
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Team == TeamTypes.BLUE)
                    {
                        favorityUnits.Add(units[i]);
                        units[i].Location = null;
                        units[i].transform.SetParent(null);
                        DontDestroyOnLoad(units[i]);
                    }
                }
                List<HexUnit> otherUnits = ObjectExtension.GetSavedObjects<HexUnit>();
                SceneManager.OpenSceneWithArgs<EquipmentsController, EquipmentsArgs>(new EquipmentsArgs { favorityUnits = favorityUnits, units = otherUnits });
            }
        }
        else
            RefrashTeam();
    }

    public void RemoveObject(HexFeature feature)
    {
        features.Remove(feature);
        Destroy(feature.gameObject);
    }

    void ClearObjects()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].DieYourSelf();
        }
        units.Clear();
        for (int i = 0; i < features.Count; i++)
        {
            features[i].Clear();
        }
        features.Clear();
    }

}
