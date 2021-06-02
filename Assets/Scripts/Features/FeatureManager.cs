using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FeatureManager : MonoBehaviour
{
    static Dictionary<EnumFeature, HexFeature> features = new Dictionary<EnumFeature, HexFeature>();

    public HexFeature[] prefab;

    private void Awake()
    {
        if (features.Count == 0)
            SetupFeature();
    }

    void SetupFeature()
    {
        for(int i = 0; i < prefab.Length; i++)
        {
            features.Add(prefab[i].id, prefab[i]);
        }
    }

    public static HexFeature GetObjectFeature(EnumFeature typeName)
    {
        return Instantiate(features[typeName]);
    }

    public static HexFeature GetRandomFeature()
    {
        int i = 0;
        int r = UnityEngine.Random.Range(0, features.Count);
        foreach (KeyValuePair<EnumFeature, HexFeature> feature in features)
        {
            if (i == r)
                return Instantiate(feature.Value);
            else
                i++;
        }
        throw new System.ArgumentException("Error random Feature!");
    }
}
