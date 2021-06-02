using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShotGun : MonoBehaviour, IGun
{
    Damage damage;
    HexUnit unit;

    public List<MeshRenderer> mesh;

    public ShotGun()
    {
        damage = new Damage(4, 8);
    }

    public float GetHitPercentage(HexUnit enemy)
    {
        ProtectiveType protection
            = enemy.GetProtectionFrom(unit);
        return 1f - protection.GetProtection() - Mathf.Pow(unit.Location.coordinates.DistanceTo(enemy.Location.coordinates) * 1f / unit.VisionRange, 2);
    }

    public int GetDamage()
    {
        return damage.GetDamage();
    }

    public List<MeshRenderer> GetAllMesh()
    {
        return mesh;
    }

    public void Save(BinaryWriter writer)
    {
        damage.Save(writer);
    }

    public void Load(BinaryReader reader)
    {
        damage = damage.Load(reader);
    }

    public void Instance(HexUnit unit)
    {
        this.unit = unit;
        transform.SetParent(unit.transform, false);
        transform.position = unit.transform.position + new Vector3(0.5f, 1, 0);
        transform.localScale = new Vector3(10, 2, 10);
        transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
}
