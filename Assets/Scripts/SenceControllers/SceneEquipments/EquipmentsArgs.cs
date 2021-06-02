using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentsArgs : SceneArgs
{
    public string path;
    public List<HexUnit> units = new List<HexUnit>();

    public List<HexUnit> favorityUnits = new List<HexUnit>();
}
