using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x, z;

    public int X {
        get
        {
            return x;
        }
    }

    public int Z
    {
        get
        {
            return z;
        }
    }

    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public int Y
    {
        get
        {
            return - x - z;
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(z);
    }

    public static HexCoordinates Load(BinaryReader reader)
    {
        HexCoordinates c;
        c.x = reader.ReadInt32();
        c.z = reader.ReadInt32();
        return c;
    }

    public static HexCoordinates FromOffsetCoordinate(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexSettings.innerRadius * 2f);
        float y = -x;
        float offset = position.z / (HexSettings.outerRadius * 3f);
        x -= offset;
        y -= offset;
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if(iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if(dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }

    public int DistanceTo(HexCoordinates other)
    {
        return ((x < other.x ? other.x - x : x - other.x) +
            (Y < other.Y ? other.Y - Y : Y - other.Y) +
            (z < other.z ? other.z - z : z - other.z)) / 2;
    }

    class Direction
    {
        public float min, max;
        public HexDirection State { get; private set; }

        public Direction(float dminAngle, float dmaxAngle, HexDirection s)
        {
            min = dminAngle;
            max = dmaxAngle;
            State = s;
        }

        public bool IsIn(float angle)
        {
            if (angle > 330)
                angle -= 360;
            if (angle > min && angle <= max)
                return true;
            else
                return false;
        }

        public static List<Direction> directions = new List<Direction>
        {
            new Direction(-30, 30, HexDirection.E),
            new Direction(30, 90, HexDirection.NE),
            new Direction(90, 150, HexDirection.NW),
            new Direction(150, 210, HexDirection.W),
            new Direction(210, 270, HexDirection.SW),
            new Direction(270, 330, HexDirection.SE),
        };
    }

    public static HexDirection GetDirection(HexUnit unit, HexUnit enemy)
    {

        float angle = Vector3.Angle(enemy.transform.position - unit.transform.position, Vector3.left);
        if (enemy.transform.position.z > unit.transform.position.z)
            angle = -angle;
        //float angle = Mathf.Atan(d.z / d.x) * Mathf.Rad2Deg;
        if (angle < -30)
            angle += 360;
        foreach (var direction in Direction.directions)
        {
            if (direction.IsIn(angle))
                return direction.State;
        }
        throw new System.ArgumentException("Error");
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
}
