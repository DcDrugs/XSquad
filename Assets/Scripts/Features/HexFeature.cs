using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class HexFeature : MonoBehaviour
{
    public ProtectiveType protective;

    protected HexCell location;

    protected float orientation;

    public EnumFeature id;

    protected HexFeature() { }


    public static HexFeature featurePrefab;

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            location = value;
            SetFeatureTransform(value.LocalPosition);
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    protected void SetFeatureTransform(Vector3 value)
    {
        Vector3 position = Vector3.zero;
       // position.y += transform.localScale.y * 0.5f;
        transform.localPosition = value + position;
    }
    public void ValidateLocation()
    {
        SetFeatureTransform(location.LocalPosition);
    }

    public virtual void Clear()
    {
        location.Feature = null;
        Destroy(gameObject);
    }

    public virtual void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
        writer.Write((short)id);
    }

    static void Fill(HexFeature feature, float orientation)
    {
        feature.location = null;
        feature.Orientation = orientation;
    }

    public static HexFeature Load(BinaryReader reader)
    { 
        float orientation = reader.ReadSingle();
        var feature = FeatureManager.GetObjectFeature((EnumFeature)reader.ReadInt16());
        Fill(feature, orientation);
        return feature;
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        var feature = Load(reader);
        grid.AddFeature(feature, grid.GetCell(coordinates));
    }

    public static void AddFeatureOnGrid(HexGrid grid, HexFeature feature, HexCell cell)
    {
        grid.AddFeature(feature, cell);
    }

    public static HexFeature CreateFeature(EnumFeature type, float orientation)
    {
        var feature = FeatureManager.GetObjectFeature(type);
        HexFeature.Fill(feature, orientation);
        return feature;
    }

    public static HexFeature CreateRandomFeature(float orientation)
    {
        var feature = FeatureManager.GetRandomFeature();
        HexFeature.Fill(feature, orientation);
        return feature;
    }
}
