using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeManager : MonoBehaviour
{
    static EscapeManager instance;

    GameObject UI;

    bool isLock = false;

    public bool IsLock
    {
        get
        {
            return isLock;
        }
        set
        {
            isLock = value;
        }
    }

    public static EscapeManager Instance()
    {
        return instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isLock)
        {
            UI.gameObject.SetActive(!UI.gameObject.activeSelf);
            if (HexMapCamera.Instance())
                HexMapCamera.Locked = !HexMapCamera.Locked;
        }
    }

    private void Awake()
    {
        instance = this;
        UI = transform.GetChild(0).gameObject;
    }
    public void Play()
    {
        UI.SetActive(false);
        if (HexMapCamera.Instance())
            HexMapCamera.Locked = false;
        if (HexGameUI.Instance())
            HexGameUI.Instance().gameObject.SetActive(true);
        Time.timeScale = 1f;
        isLock = false;
    }

    public void Load()
    {
        UI.SetActive(false);
        Lock();
    }

    public void Save()
    {
        UI.SetActive(false);
        Lock();
    }

    public void Cancel()
    {
        Play();
    }

    public void Lock()
    {
        if (HexMapCamera.Instance())
            HexMapCamera.Locked = true;
        if (HexGameUI.Instance())
            HexGameUI.Instance().gameObject.SetActive(false);
        Time.timeScale = 1f;
        isLock = true;
    }
    public void Exit()
    {
        Play();
        SceneManager.OpenSceneWithArgs<MenuController, MenuArgs>(null);
    }
}
