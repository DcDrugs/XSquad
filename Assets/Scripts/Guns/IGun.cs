using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public interface IGun
{ 
    int GetDamage();

    List<MeshRenderer> GetAllMesh();

    float GetHitPercentage(HexUnit unit);

    void Save(BinaryWriter writer);

    void Load(BinaryReader reader);

    void Instance(HexUnit unit);
}
