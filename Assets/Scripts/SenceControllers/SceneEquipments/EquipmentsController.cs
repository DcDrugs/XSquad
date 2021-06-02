using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[SceneControllerAttribute("Equipments")]
public class EquipmentsController : SceneController<EquipmentsController, EquipmentsArgs>
{ 
    public Transform FolderUnit;

    public Transform[] SelectUnits;

    public SaveLoadMap saveLoader;

    public HexUnit unitPrefab;

    protected override void OnStart()
    {
        HexUnit.unitPrefab = unitPrefab;
        saveLoader.LoadAction = Load;
        saveLoader.SaveAction = Save;
        saveLoader.mode = EnumMode.EQUIP;
        ObjectExtension.Remove(Args.units);
        ObjectExtension.Remove(Args.favorityUnits);
        if (Args.path == "New Game")
            InstanceUnits();
        else if (Args.path == null)
            UpdateUnits(Args.units, Args.favorityUnits);
        else
            saveLoader.Load(Args.path);
    }

    void InstanceUnits()
    {
        if (Args.units.Count == 0 && Args.favorityUnits.Count == 0)
        {
            for (int i = 0; i < 6; i++)
                Args.units.Add(HexUnit.GenerateUnit(HexUnit.unitPrefab, GunManager.GetGun<MashineGun>(), 0, TeamTypes.BLUE, 2));
            for (int i = 0; i < SelectUnits.Length; i++)
                Args.favorityUnits.Add(HexUnit.GenerateUnit(HexUnit.unitPrefab, GunManager.GetGun<MashineGun>(), 0, TeamTypes.BLUE, 2));
        }
        UpdateUnits(Args.units, Args.favorityUnits);
    }

    void UpdateUnits(List<HexUnit> units, List<HexUnit> favorityUnits)
    {
        foreach (HexUnit unit in units)
        {
            unit.Location = null;
            unit.transform.SetParent(FolderUnit, false);
            unit.transform.localPosition = Vector3.zero;
        }
        int i = 0;
        for (; i < favorityUnits.Count && i < SelectUnits.Length; i++)
        {
            favorityUnits[i].Location = null;
            favorityUnits[i].transform.SetParent(SelectUnits[i], false);
            favorityUnits[i].transform.localPosition = Vector3.zero;
            SetLayer(favorityUnits[i].gameObject, 5);
        }

        for (; i < SelectUnits.Length && FolderUnit.childCount > 0; i++)
        {
            Transform tmp = FolderUnit.GetChild(0);
            tmp.SetParent(SelectUnits[i], true);
            tmp.localPosition = Vector3.zero;
            SetLayer(tmp.gameObject, 5);
        }
    }

    void SetLayer(GameObject obj, int layer = 0)
    {
        obj.layer = layer;
        foreach(Transform child in obj.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = layer;
        }
    }


    public void Save(string path, int fileVersion, EnumMode mode)
    {
        using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(fileVersion);
            writer.Write((short)mode);
            writer.Write(Args.units.Count);
            for (int i = 0; i < Args.units.Count; i++)
                Args.units[i].Save(writer);
            writer.Write(Args.favorityUnits.Count);
            for (int i = 0; i < Args.favorityUnits.Count; i++)
                Args.favorityUnits[i].Save(writer);
        }
    }

    public void Load(string path, EnumMode mode)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not Found");
            return;
        }
        using (var reader = new BinaryReader(File.OpenRead(path)))
        {
            var version = reader.ReadInt32();
            if (mode == (EnumMode)reader.ReadInt16())
            {
                switch (version)
                {
                    case 0:
                        Args.units = new List<HexUnit>(reader.ReadInt32());
                        for (int i = 0; i < Args.units.Capacity; i++)
                            Args.units.Add(HexUnit.Load(reader));
                        Args.favorityUnits = new List<HexUnit>(reader.ReadInt32());
                        for (int i = 0; i < Args.favorityUnits.Capacity; i++)
                            Args.favorityUnits.Add(HexUnit.Load(reader));
                        HexMapCamera.ValidatePosition();
                        break;
                    default:
                        Debug.LogError("Unkown version load");
                        break;
                }
            }
        }
        UpdateUnits(Args.units, Args.favorityUnits);
    }

    public void StartMission()
    {
        var units = FolderUnit.GetComponentsInChildren<HexUnit>();
        for (int i = 0; i < units.Length; i++)
        {
            HexUnit unit = units[i];
            unit.transform.parent = null;
            ObjectExtension.DontDestroyOnLoad(unit);
        }

        List<HexUnit> enemyUnits = new List<HexUnit>(12);

        for (int i = 0; i < 12; i++)
        {
            enemyUnits.Add(
                HexUnit.GenerateUnit(
                    HexUnit.unitPrefab, GunManager.GetRandomGun(), 
                    Random.Range(0f, 360f), TeamTypes.RED, HexUnit.MaxAction
                    )
                );
            DontDestroyOnLoad(enemyUnits[i].gameObject);
        }

        List<HexUnit> freandlyUnits = new List<HexUnit>(4);
        for (int i = 0; i < SelectUnits.Length; i++)
        {
            HexUnit unit = SelectUnits[i].GetComponentInChildren<HexUnit>();
            unit.transform.parent = null;
            ObjectExtension.DontDestroyOnLoad(unit);
            freandlyUnits.Add(unit);
        }

        SceneManager.OpenSceneWithArgs<MissionController, MissionArgs>(new MissionArgs { FreandlyUnits = freandlyUnits, EnemyUnits = enemyUnits });
    }
}
