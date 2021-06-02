using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public SaveLoadMap load;
    public void LoadGame()
    {
        gameObject.SetActive(false);
        load.mode = EnumMode.EQUIP;
        load.Open(false);
    }

    public void LoadEditor()
    {
        gameObject.SetActive(false);
        load.mode = EnumMode.EDITOR;
        load.Open(false);
    }

    public void Close()
    {
        gameObject.SetActive(true);
        load.gameObject.SetActive(false);
    }    
    public void Exit()
    {
        Application.Quit();
    }
}
