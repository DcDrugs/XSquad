using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SceneControllerAttribute("Mission")]
public class MissionController : SceneController<MissionController, MissionArgs>
{
    public HexGrid grid;
    public SaveLoadMap map;
    public HexMapGenerator generator;
    public HexMapEditor editor;

    protected override void OnStart()
    {
        map.mode = EnumMode.GAME;
        grid.InstanceFromController();
        editor.SetEditMode(false);
        editor.gameObject.SetActive(false);
        if (Args.path == null || Args.path == "New Game")
        {
            generator.GenerateMap(80, 60);
            ObjectExtension.Remove(Args.FreandlyUnits);
            ObjectExtension.Remove(Args.EnemyUnits);
            grid.CreateMission(Args.FreandlyUnits, Args.EnemyUnits);
        }
        else
            map.Load(Args.path);
    }
}
