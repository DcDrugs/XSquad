using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MissionArgs : SceneArgs
{
   public List<HexUnit> FreandlyUnits { get; set; }
   public List<HexUnit> EnemyUnits { get; set; }

   public string path;
}
