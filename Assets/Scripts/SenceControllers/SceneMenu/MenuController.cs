using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[SceneControllerAttribute("Main")]
public class MenuController : SceneController<MenuController, MenuArgs>
{
    protected override void OnStart() 
    {
       ObjectExtension.Clear();
    }

    public static void Load(string path, EnumMode mode)
    {
        switch (mode)
        {
            case EnumMode.GAME:
                SceneManager.OpenSceneWithArgs<MissionController, MissionArgs>(new MissionArgs { path = path });
                break;

            case EnumMode.EDITOR:
                SceneManager.OpenSceneWithArgs<EditorController, EditorArgs>(new EditorArgs { path = path });
                break;

            case EnumMode.EQUIP:
                SceneManager.OpenSceneWithArgs<EquipmentsController, EquipmentsArgs>(new EquipmentsArgs { path = path });
                break;

            case EnumMode.NONE:
                SceneManager.OpenSceneWithArgs<MenuController, MenuArgs>(null);
                break;
        }
    }
}
