using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;

public class GunManager : MonoBehaviour
{
    static GunManager manager;

    static Dictionary<string, Func<IGun>> guns = new Dictionary<string, Func<IGun>>();

    public MashineGun mashineGunprefab;
    public SniperGun sniperGunPrefab;
    public ShotGun shotGunPrefab;
    public UziGun uziGunPrefab;
    public BigMashineGun bigMashineGunPrefab;
    private void Awake()
    {
        manager = this;
        if (guns.Count == 0)
            manager.SetupGuns();
    }

    IGun InstanceMashineGun()
    {
        return Instantiate(mashineGunprefab);
    }

    IGun InstanceSniperGun()
    {
        return Instantiate(sniperGunPrefab);
    }

    IGun InstanceShotGun()
    {
        return Instantiate(shotGunPrefab);
    }

    IGun InstanceUziGun()
    {
        return Instantiate(uziGunPrefab);
    }

    IGun InstanceBigMashineGun()
    {
        return Instantiate(bigMashineGunPrefab);
    }

    void SetupGuns()
    {
        guns.Add(typeof(MashineGun).ToString(), InstanceMashineGun);
        guns.Add(typeof(SniperGun).ToString(), InstanceSniperGun);
        guns.Add(typeof(ShotGun).ToString(), InstanceShotGun);
        guns.Add(typeof(UziGun).ToString(), InstanceUziGun);
        guns.Add(typeof(BigMashineGun).ToString(), InstanceBigMashineGun);
    }
    public static void Save(BinaryWriter writer, IGun gun)
    {

        writer.Write(gun.GetType().ToString());
        gun.Save(writer);
    }

    public static IGun Load(BinaryReader reader)
    {
        var t = guns[reader.ReadString()];
        IGun gun = t();
        gun.Load(reader);
        return gun;
    }

    public static T GetGun<T>() where T : IGun
    {
        return (T) guns[typeof(T).ToString()]();
    }

    public static IGun GetRandomGun()
    {
        int i = 0;
        int r = UnityEngine.Random.Range(0, guns.Count);
        foreach(KeyValuePair<string, Func<IGun>> pair in guns)
        {
            if (i == r)
                return pair.Value();
            else
                i++;
        }
        throw new System.ArgumentException("Guns not found!");
    }
}
