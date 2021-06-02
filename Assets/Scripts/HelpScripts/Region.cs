using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
	static Region instance = null;

	public const int countRegions = 36;

	protected Region() { }

	public static Region Instance()
    {
		if (instance == null)
			instance = new Region();
		return instance;
    }

	public struct MapRegion
	{
		public int xMin, xMax, zMin, zMax;
	}

	public List<MapRegion> Regions { get; private set; }

	public void CreateRegions(int cellCountX, int cellCountZ, int mapBorderX, int mapBorderZ)
	{
		if (Regions == null)
		{
			Regions = new List<MapRegion>();
		}
		else
		{
			Regions.Clear();
		}

		int size = (int)Mathf.Sqrt(countRegions);

		int xRigion = (cellCountX - 2 * mapBorderX) / size;
		int zRigion = (cellCountZ - 2 * mapBorderZ) / size;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				Regions.Add(
					CreateRegion(
						i * xRigion + mapBorderX,
						(i + 1) * xRigion + mapBorderX,
						j * zRigion + mapBorderZ,
						(j + 1) * zRigion + mapBorderZ
						)
					);
			}
		}
	}

	public static HexCell GetRandomCell(HexGrid grid, int numberOfRigion)
	{
		MapRegion region = Instance().Regions[numberOfRigion];
		return grid.GetCell(
			Random.Range(region.xMin, region.xMax),
			Random.Range(region.zMin, region.zMax)
		);
	}

	public static HexCell GetRandomCell(HexGrid grid, Region.MapRegion region)
	{
		return grid.GetCell(
			Random.Range(region.xMin, region.xMax),
			Random.Range(region.zMin, region.zMax)
		);
	}

	MapRegion CreateRegion(int xMin, int xMax, int zMin, int zMax)
	{
		MapRegion region;
		region.xMin = xMin;
		region.xMax = xMax;
		region.zMin = zMin;
		region.zMax = zMax;
		return region;
	}
}
