using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.IO;

public class ShootAbility : MonoBehaviour, IAbility
{
    HexShootCamera shootCamera;

    public Image Spriteprefab { get; set; }

    public Button ButtonAbility { get; set; }

    public HexUnit Owner { get; set; }

    public Transform TransformAbility{ get; set; }

    public bool isStarted = false;

    public void Instance(HexUnit owner)
    {
        Owner = owner;
        Spriteprefab = transform.GetComponent<Image>();
        ButtonAbility = transform.GetComponent<Button>();
        TransformAbility = transform;
    }

    public bool CanDoAbility()
    {
        if (!isStarted && Owner.Action > 0 && Owner.Enemies != null)
            return Owner.Enemies.Count > 0;
        else
            return false;
    }

    public void DoAbility()
    {
        if (CanDoAbility())
        {
            isStarted = true;
            if (shootCamera == null)
                shootCamera =
                    HexMapCamera.Instance().transform.GetComponent<HexShootCamera>();
            shootCamera.NeedChangeUnits = true;
            shootCamera.Owner = Owner;
            shootCamera.Units = Owner.Enemies;
            shootCamera.enabled = true;
            StartCoroutine(GetTraget());
        }
    }
    IEnumerator GetTraget()
    {
        HexUnit unit = null;
        while (unit == null)
        {
            unit = shootCamera.GetTarget();
            if (!shootCamera.IsWork)
            {
                isStarted = false;
                yield break;
            }
            yield return Owner;
        }
        Shoot(Owner, unit);
        Owner.Action = 0;
        isStarted = false;
    }

    public static void Shoot(HexUnit owner, HexUnit target)
    {
        System.Random res = new System.Random();

        if (res.Next(0, 100) <= owner.Gun.GetHitPercentage(target) * 100)
        {
            var damage = owner.TakeDamage();
            target.Label.text = "-" + damage.ToString();
            target.GetDamageBy(damage, owner);
        }
        else
        {
            target.Label.text = "MISS";
        }
    }

    public void Save(BinaryWriter writer)
    {

    }

    public void Load(BinaryReader reader)
    {

    }
}
