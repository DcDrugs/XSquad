using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
    HexCell[] cells;
    public HexMesh terrain;
    public HexMesh water;
    public HexMesh waterShore;

    Canvas gridCanvas;

    static Color weights1 = new Color(1f, 0f, 0f);
    static Color weights2 = new Color(0f, 1f, 0f);
    static Color weights3 = new Color(0f, 0f, 1f);

    public void Refresh()
    {
        enabled = true;
    }

    private void LateUpdate()
    {
        Triangulate();
        enabled = false;
    }

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();

        cells = new HexCell[HexSettings.chunkSizeX * HexSettings.chunkSizeZ];
    }
    public void AddCell(int index, HexCell cell)
    {
        cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }

    public void ShowUI(bool visible)
    {
        gridCanvas.gameObject.SetActive(visible);
    }

    public void Triangulate()
    { 
        terrain.Clear();
        waterShore.Clear();
        water.Clear();
        for (int i = 0; i < cells.Length; i++)
            Triangulate(cells[i]);
        terrain.Apply();
        water.Apply();
        waterShore.Apply();
    }

    private void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.LocalPosition;
        var e = new EdgeVertices
            (
             center + HexSettings.GetFirstSolidCorner(direction),
             center + HexSettings.GetSecondSolidCorner(direction)
            );

        TriangulateEdgeFan(center, e, cell.Index);

        if (direction <= HexDirection.SE)
            TriangulateConnection(direction, cell, e);

        if (cell.IsUnderWater)
            TriangulateWater(direction, cell, center);
    }

    void TriangulateWater(HexDirection direction, HexCell cell, Vector3 center)
    {
        center.y = cell.WaterSurfaceY;

        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null || !neighbor.IsUnderWater)
            TriangulateWaterShore(direction, cell, neighbor, center);
        else
            TriangulateOpenWater(direction, cell, neighbor, center);
    }

    void TriangulateOpenWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
    {
        Vector3 c1 = center + HexSettings.GetFirstSolidCorner(direction);
        Vector3 c2 = center + HexSettings.GetSecondSolidCorner(direction);
        Vector3 indices;
        water.AddTriangle(center, c1, c2);
        indices.x = indices.y = indices.z = cell.Index;
        water.AddTriangleCellData(indices, weights1);

        if (direction <= HexDirection.SE && neighbor != null)
        {

            Vector3 bridge = HexSettings.GetBridge(direction);
            Vector3 e1 = c1 + bridge;
            Vector3 e2 = c2 + bridge;

            water.AddQuad(c1, c2, e1, e2);
            indices.y = neighbor.Index;
            water.AddQuadCellData(indices, weights1, weights2);

            if (direction <= HexDirection.E)
            {
                HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
                if (nextNeighbor == null || !nextNeighbor.IsUnderWater)
                    return;

                water.AddTriangle(c2, e2, c2 + HexSettings.GetBridge(direction.Next()));
                indices.z = nextNeighbor.Index;
                water.AddTriangleCellData(indices, weights1, weights2, weights3);
            }
        }
    }

    void TriangulateWaterShore(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
    {
        var e1 = new EdgeVertices(
            center + HexSettings.GetFirstSolidCorner(direction),
            center + HexSettings.GetSecondSolidCorner(direction)
        );
        water.AddTriangle(center, e1.v1, e1.v2);
        water.AddTriangle(center, e1.v2, e1.v3);
        water.AddTriangle(center, e1.v3, e1.v4);

        Vector3 indices;
        indices.x = indices.z = cell.Index;
        if(neighbor != null)
            indices.y = neighbor.Index;
        else
            indices.y = cell.Index;
        water.AddTriangleCellData(indices, weights1);
        water.AddTriangleCellData(indices, weights1);
        water.AddTriangleCellData(indices, weights1);

        if (neighbor != null)
        {
            Vector3 center2 = neighbor.LocalPosition;
            center2.y = center.y;
            var e2 = new EdgeVertices(
                center2 + HexSettings.GetSecondSolidCorner(direction.Opposite()),
                center2 + HexSettings.GetFirstSolidCorner(direction.Opposite())
            );
            waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);

            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (nextNeighbor != null)
            {
                Vector3 v3 = nextNeighbor.LocalPosition
                    + (nextNeighbor.IsUnderWater ?
                    HexSettings.GetFirstWaterCorner(direction.Previous()) :
                    HexSettings.GetFirstSolidCorner(direction.Previous()));
                v3.y = center.y;
                waterShore.AddTriangle(e1.v4, e2.v4, v3);
                waterShore.AddTiangleUV(new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, nextNeighbor.IsUnderWater ? 0f : 1f)
                );
            }
        }
    }

    void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
            return;

        Vector3 bridge = HexSettings.GetBridge(direction);
        bridge.y = neighbor.LocalPosition.y - cell.LocalPosition.y;
        var e2 = new EdgeVertices(
            e1.v1 + bridge,
            e1.v4 + bridge
        );


        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        else
        {
            TriangulateEdgeStrip(e1, weights1, cell.Index, e2, weights2, neighbor.Index);
        }

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = e1.v4 + HexSettings.GetBridge(direction.Next());
            v5.y = nextNeighbor.LocalPosition.y;

            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e1.v4, cell, e2.v4, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(e2.v4, neighbor, v5, nextNeighbor, e1.v4, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
            }
        }
    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        terrain.AddTriangle(center, edge.v3, edge.v4);

        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
        terrain.AddTriangleCellData(indices, weights1);
    }

    void TriangulateEdgeStrip(
        EdgeVertices e1, Color w1, float index1,
        EdgeVertices e2, Color w2, float index2)
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);

        Vector3 indices;
        indices.x = indices.z = index1;
        indices.y = index2;
        terrain.AddQuadCellData(indices, w1, w2);
        terrain.AddQuadCellData(indices, w1, w2);
        terrain.AddQuadCellData(indices, w1, w2);
    }
    void TriangulateEdgeTerraces(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color w2 = HexSettings.TerraceLerp(weights1, weights2, 1);
        float index1 = beginCell.Index;
        float index2 = endCell.Index;

        TriangulateEdgeStrip(begin, weights1, index1, e2, w2, index2);


        for (int i = 2; i < HexSettings.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color w1 = w2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            w2 = HexSettings.TerraceLerp(weights1, weights2, i);
            TriangulateEdgeStrip(e1, w1, index1, e2, w2, index2);
        }

        TriangulateEdgeStrip(e2, w2, index1, end, weights2, index2);
    }

    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }
            if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell);
                return;
            }
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
            TriangulateCornerTerracesCliff(
                bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }
        if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
            TriangulateCornerCliffTerraces(
                bottom, bottomCell, left, leftCell, right, rightCell);
        }
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }

        Vector3 indices;
        indices.x = bottomCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;
        terrain.AddTriangle(bottom, left, right);
        terrain.AddTriangleCellData(indices, weights1, weights2, weights3);


    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexSettings.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexSettings.TerraceLerp(begin, right, 1);
        Color w3 = HexSettings.TerraceLerp(weights1, weights2, 1);
        Color w4 = HexSettings.TerraceLerp(weights1, weights3, 1);

        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        terrain.AddTriangle(begin, v3, v4);
        terrain.AddTriangleCellData(indices, weights1, w3, w4);



        for (int i = 2; i < HexSettings.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color w1 = w3;
            Color w2 = w4;
            v3 = HexSettings.TerraceLerp(begin, left, i);
            v4 = HexSettings.TerraceLerp(begin, right, i);
            w3 = HexSettings.TerraceLerp(weights1, weights2, i);
            w4 = HexSettings.TerraceLerp(weights1, weights3, i);
            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadCellData(indices, w1, w2, w3, w4);
        }

        terrain.AddQuad(v3, v4, left, right);
        terrain.AddQuadCellData(indices, w3, w4, weights2, weights3);
    }

    void TriangulateCornerTerracesCliff(
    Vector3 begin, HexCell beginCell,
    Vector3 left, HexCell leftCell,
    Vector3 right, HexCell rightCell
)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(
            HexSettings.Perturb(begin), HexSettings.Perturb(right), b
        );
        Color boundaryWeights = Color.Lerp(weights1, weights3, b);
        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(
            begin, weights1, left, weights2, boundary, boundaryWeights, indices
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, weights2, right, weights3,
                boundary, boundaryWeights, indices
            );
        }
        else
        {
            terrain.AddTriangleUnperturbed(
                 HexSettings.Perturb(left), HexSettings.Perturb(right), boundary
            );
            terrain.AddTriangleCellData(
                indices, weights2, weights3, boundaryWeights
            );
        }
    }

    void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(
             HexSettings.Perturb(begin), HexSettings.Perturb(left), b
        );
        Color boundaryWeights = Color.Lerp(weights1, weights2, b);
        Vector3 indices;
        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(
            right, weights3, begin, weights1, boundary, boundaryWeights, indices
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, weights2, right, weights3,
                boundary, boundaryWeights, indices
            );
        }
        else
        {
            terrain.AddTriangleUnperturbed(
                 HexSettings.Perturb(left), HexSettings.Perturb(right), boundary
            );
            terrain.AddTriangleCellData(
                indices, weights2, weights3, boundaryWeights
            );
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, Color beginWeights,
        Vector3 left, Color leftWeights,
        Vector3 boundary, Color boundaryWeights, Vector3 indices)
    {
        Vector3 v2 = HexSettings.Perturb(HexSettings.TerraceLerp(begin, left, 1));
        Color w2 = HexSettings.TerraceLerp(beginWeights, leftWeights, 1);

        terrain.AddTriangleUnperturbed(HexSettings.Perturb(begin), v2, boundary);
        terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

        for (int i = 2; i < HexSettings.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color w1 = w2;
            v2 = HexSettings.Perturb(HexSettings.TerraceLerp(begin, left, i));
            w2 = HexSettings.TerraceLerp(beginWeights, leftWeights, i);
            terrain.AddTriangleUnperturbed(v1, v2, boundary);
            terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
        }

        terrain.AddTriangleUnperturbed(v2, HexSettings.Perturb(left), boundary);
        terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
    }
}
