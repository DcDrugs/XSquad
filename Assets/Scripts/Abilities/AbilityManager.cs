using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Reflection;

public class AbilityManager : MonoBehaviour
{
    static AbilityManager manager;

    static Dictionary<string, Func<IAbility>> abilities = new Dictionary<string, Func<IAbility>>();

    public ShootAbility shootIconPrefab;
    public OverWatchAbility overWatchIconPrefab;
    public SkipAbility skipIconPrefab;

    private void Awake()
    {
        manager = this;
        if (abilities.Count == 0)
            manager.SetupAbilities();
    }

    IAbility InstanceShootIcon()
    {
        return shootIconPrefab;
    }

    IAbility InstanceOverWatchIcon()
    {
        return overWatchIconPrefab;
    }

    IAbility InstanceSkipIcon()
    {
        return skipIconPrefab;
    }

    void SetupAbilities()
    {
        abilities.Add(typeof(ShootAbility).ToString(), InstanceShootIcon);
        abilities.Add(typeof(SkipAbility).ToString(), InstanceSkipIcon);
        abilities.Add(typeof(OverWatchAbility).ToString(), InstanceOverWatchIcon);
    }
    public static void Save(BinaryWriter writer, IAbility ability)
    {
        writer.Write(ability.GetType().ToString());
        ability.Save(writer);
    }

    public static void Load(BinaryReader reader, HexUnit unit)
    {
        string str = reader.ReadString();
        var type = Type.GetType(str);
        var method = typeof(AbilityManager).GetMethod("InstanceAbility", BindingFlags.Static);
        var genericMethod = method.MakeGenericMethod(typeof(HexUnit), type);
        IAbility ability = abilities[str]();
        ability.Load(reader);
        genericMethod.Invoke(null, new object[] { unit, ability });
    }
    static void InstanceAbility<T>(HexUnit unit, T objIconPrefab) where T : UnityEngine.MonoBehaviour, IAbility
    {
        var prefab = Instantiate(objIconPrefab);
        prefab.transform.SetParent(unit.transform.GetChild(0), false);
        prefab.GetComponent<Image>().color = Color.blue;
        prefab.Instance(unit);
        prefab.Spriteprefab.enabled = false;
        prefab.ButtonAbility.enabled = false;
        unit.AddAbility(prefab);
    }

    public static void SetAbilityOnUnit<T>(HexUnit unit) where T : UnityEngine.MonoBehaviour, IAbility
    {
        InstanceAbility<T>(unit, (T)abilities[typeof(T).ToString()]());
    }
}
