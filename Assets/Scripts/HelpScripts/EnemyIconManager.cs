using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyIconManager : MonoBehaviour
{
    public int indexEnemy { get; set; }

    public List<HexUnit> Enemies { get; set; }

    public Image icon;

    public void ShowEnemy()
    {
        if (HexMapCamera.Locked)
            HexShootCamera.Instance().ChangeIndex(indexEnemy);
        else
        {
            HexMapCamera camera = HexMapCamera.Instance();
            camera.swivel.DOMove(Enemies[indexEnemy].transform.position, 1);
        }
    }
}
