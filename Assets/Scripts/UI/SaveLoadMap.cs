using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class SaveLoadMap : MonoBehaviour
{
    public HexGrid hexGrid;
    bool saveMode;

    public Text menuLabel, actionButtonLabel;
    public InputField nameInput;

    public RectTransform listContent;

    public SaveLoadItem itemPrefab;

    const int mapFileVersion = 0;

    public EnumMode mode;

    public Action<string, int, EnumMode> SaveAction { get; set; }
    public Action<string, EnumMode> LoadAction { get; set; }

    public SaveLoadMap()
    {
        SaveAction = SaveGrid;
        LoadAction = LoadGrid;
    }

    public void Open(bool saveMode)
    {
        this.saveMode = saveMode;
        if (saveMode)
        {
            menuLabel.text = "Save Map";
            actionButtonLabel.text = "Save";
        }
        else
        {
            menuLabel.text = "Load Map";
            actionButtonLabel.text = "Load";
        }
        FillList();
        gameObject.SetActive(true);
        if(HexMapCamera.Instance() != null)
            HexMapCamera.Locked = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (HexMapCamera.Instance() != null)
            HexMapCamera.Locked = true;
    }

    private static bool IsValidFilename(string fileName)
    {
        var invalidChars = string.Join("", Path.GetInvalidFileNameChars());
        var regex = new Regex("[" + Regex.Escape(string.Join("", invalidChars)) + "]");

        return !regex.IsMatch(fileName);
    }

    string GetSelectPath()
    {
        string mapName = nameInput.text;
        if (mapName.Length == 0 || !IsValidFilename(mapName + ".map"))
            return null;
        if (mapName == "New Game")
            return mapName;
        return Path.Combine(Application.persistentDataPath, mapName + ".map");
    }

    public void Action()
    {
        string path = GetSelectPath();
        if (path == null)
            return;
        if (saveMode)
            Save(path);
        else
        {
            EnumMode tmp = mode;
            if (path != "New Game")
            {
                using (var reader = new BinaryReader(File.OpenRead(path)))
                {
                    reader.ReadInt32();
                    tmp = (EnumMode)reader.ReadInt16();
                }
            }
            MenuController.Load(path, tmp);
        }
        Close();
    }

    public void Delete()
    {
        string path = GetSelectPath();
        if (path == null)
            return;
        if(File.Exists(path))
            File.Delete(path);
        nameInput.text = "";
        FillList();
    }


    public void Save(string path)
    {
        SaveAction(path, mapFileVersion, mode);
    }

    public void Load(string path)
    {
        LoadAction(path, mode);
    }

    public void SelectItem(string name)
    {
        nameInput.text = name;
    }

    void FillList()
    {
        for (int i = 0; i < listContent.childCount; i++)
            Destroy(listContent.GetChild(i).gameObject);
        string[] paths =
            Directory.GetFiles(Application.persistentDataPath, "*.map");
        Array.Sort(paths);
        AddItemOnScroleView(paths);
    }

    void AddItemOnScroleView(string[] paths)
    {
        foreach (string path in paths)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                reader.ReadInt32();
                EnumMode tmp = (EnumMode)reader.ReadInt16();
                if (mode == tmp || (mode == EnumMode.EQUIP && tmp == EnumMode.GAME) || (tmp == EnumMode.EQUIP && mode == EnumMode.GAME))
                {
                    AddItem(path);
                }
            }
        }
        if(!saveMode)
            AddItem("New Game");
    }

    void AddItem(string path)
    {
        SaveLoadItem item = Instantiate(itemPrefab);
        item.menu = this;
        item.MapName = Path.GetFileNameWithoutExtension(path);
        item.transform.SetParent(listContent, false);
    }

    public void SaveGrid(string path, int fileVersion, EnumMode mode)
    {
        using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(fileVersion);
            writer.Write((short)mode);
            hexGrid.Save(writer);
        }
    }

    public void LoadGrid(string path, EnumMode mode)
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
                        hexGrid.Load(reader);
                        HexMapCamera.ValidatePosition();
                        break;
                    default:
                        Debug.LogError("Unkown version load");
                        break;
                }
            }
        }
    }
}
