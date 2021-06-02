using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SkipAbility : MonoBehaviour, IAbility
{
    TeamTypes team;

    public Image Spriteprefab { get; set; }

    public Button ButtonAbility { get; set; }

    public Transform TransformAbility { get; set; }

    public void Instance(HexUnit owner)
    {
        Spriteprefab = transform.GetComponent<Image>();
        ButtonAbility = transform.GetComponent<Button>();
        team = owner.Team;
    }

    public bool CanDoAbility()
    {
        return true;
    }

    public void DoAbility()
    {
        if (CanDoAbility())
        {
            HexGrid.Instance().Team = team.ChangeTeam();
        }
    }

    public void Save(BinaryWriter writer)
    {

    }

    public void Load(BinaryReader reader)
    {

    }
}
