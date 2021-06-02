using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Damage
{
    int minDamage;
    int maxDamage;

    public Damage(int min, int max)
    {
        minDamage = min;
        maxDamage = max;
    }

    public int GetDamage()
    {
        return (new System.Random()).Next(minDamage, maxDamage);
    }


    public void Save(BinaryWriter writer)
    {
        writer.Write(minDamage);
        writer.Write(maxDamage);
    }

    public Damage Load(BinaryReader reader)
    {
        return new Damage(reader.ReadInt32(), reader.ReadInt32());
    }
}
