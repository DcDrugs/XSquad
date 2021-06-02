using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public interface IAbility
{
    Image Spriteprefab { get; set; }

    Button ButtonAbility { get; set; }

    Transform TransformAbility { get; set; }

    void DoAbility();
    bool CanDoAbility();
    void Instance(HexUnit owner);

    void Save(BinaryWriter writer);

    void Load(BinaryReader reader);
}
