using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[SceneControllerAttribute("Editor")]
public class EditorController : SceneController<EditorController, EditorArgs>
{
    public HexGrid grid;
    public SaveLoadMap map;

    protected override void OnStart()
    {
        grid.InstanceFromController();
        if (Args.path == "New Game")
        {
            InstanceNewMap();
        }
        else
            map.Load(Args.path);
    }

    void InstanceNewMap()
    {
        grid.CreateMap(grid.cellCountX, grid.cellCountZ);
    }
}
