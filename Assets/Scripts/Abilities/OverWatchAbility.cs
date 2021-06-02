using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class OverWatchAbility : MonoBehaviour, IAbility
{
    HexUnit owner;

    HexShootCamera shootCamera;

    public Button ButtonAbility { get; set; }

    public Image Spriteprefab { get; set; }

    public Transform TransformAbility { get; set; }

    TeamTypes team;

    bool isStarted = false;

    bool isChanged = false;

    public void Instance(HexUnit owner)
    {
        team = owner.Team;
        Spriteprefab = transform.GetComponent<Image>();
        ButtonAbility = transform.GetComponent<Button>();
        this.owner = owner;
    }

    public bool CanDoAbility()
    {
        if (owner.Action > 0 && !isStarted)
            return true;
        else
            return false;
    }

    public void DoAbility()
    {
        if (CanDoAbility())
        {
            owner.Action = 0;
            if(shootCamera == null)
                shootCamera =
                    HexMapCamera.Instance().transform.GetComponent<HexShootCamera>();
            isChanged = false;
            isStarted = true;
            shootCamera.NeedChangeUnits = false;
            shootCamera.Owner = owner;
            StartCoroutine(WaitTarget());
        }
    }

    IEnumerator WaitTarget()
    {

        while (true)
        {
            if (team != HexGrid.Instance().Team)
            {
                team = HexGrid.Instance().Team;
                isChanged = true;
            }
            if (isChanged && owner.Team == HexGrid.Instance().Team)
            {
                isChanged = false;
                isStarted = false;
                yield break;
            }
            owner.CheckEnemies();
            if (owner.Enemies.Count > 0)
                break;
            yield return owner;
        }

        shootCamera.Units = owner.Enemies;
        shootCamera.enabled = true;
        ShootAbility.Shoot(owner, owner.Enemies[0]);
        isStarted = false;
        isChanged = false;
    }

    public void Save(BinaryWriter writer)
    {

    }

    public void Load(BinaryReader reader)
    {

    }

}
