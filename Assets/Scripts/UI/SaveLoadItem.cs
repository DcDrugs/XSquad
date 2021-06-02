using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadItem : MonoBehaviour
{
    public SaveLoadMap menu;

    public Text buttonlabel;

    string mapName = "";

    public string MapName
    {
        get
        {
            return mapName;
        }
        set
        {
            buttonlabel.text = value;
            mapName = value;
            transform.GetChild(0);
        }
    }

    public void Select()
    {
        menu.SelectItem(mapName);
    }
}
