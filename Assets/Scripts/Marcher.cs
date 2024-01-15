using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class Marcher : MonoBehaviour
{

    public enum InterpolationMethod
    {
        HalfPoint,
        Linear,
        Smoothstep,
    }

    #region MeshAttributes
    protected Dictionary<Vector3, int> meshVerticesIndices;
    protected List<Vector3> meshVertices;
    protected List<int> meshTriangles;


    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    Mesh mesh;
    #endregion

    public int boundSize;
    public float resolution;
    public float interpolationThreshold;
    public InterpolationMethod interpolationMethod;

    protected abstract bool VertexIsSelected(in Vector3 pos);

    protected bool IsPositionValid(in Vector3 pos)
    {
        return pos.x >= 0 && pos.x < boundSize && pos.y >= 0 && pos.y < boundSize && pos.y >= 0 && pos.y < boundSize;
    }

    protected virtual void Initialize()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh { name = "Procedural mesh (Plastimesh)" };
        meshFilter.mesh = mesh;
    }

    #region Interpolation 
    protected Vector3 GetHalfPoint(Vector3 v1, Vector3 v2)
    {
        return v1 + (v2 - v1) * 0.5f;
    }

    protected Vector3 GetSmoothstep(Vector3 v1, Vector3 v2, float f1, float f2)
    {
        float t = (interpolationThreshold - f1) / (f2 - f1);
        t = t * t * (3 - 2 * t);

        return new Vector3(v1.x + t * (v2.x - v1.x),
            v1.y + t * (v2.y - v1.y),
            v1.z + t * (v2.z - v1.z)
        );
    }

    protected Vector3 GetLinealInterpolation(Vector3 v1, Vector3 v2, float f1, float f2)
    {
        float t = (interpolationThreshold - f1) / (f2 - f1);
        return new Vector3(v1.x + t * (v2.x - v1.x),
                       v1.y + t * (v2.y - v1.y),
                       v1.z + t * (v2.z - v1.z));
    }
    #endregion

    protected Vector3 GetEdgeVertex(Vector3 v1, Vector3 v2, float f1, float f2)
    {
        switch (interpolationMethod)
        {
            case InterpolationMethod.HalfPoint:
                return GetHalfPoint(v1, v2);
            case InterpolationMethod.Linear:
                return GetLinealInterpolation(v1, v2, f1, f2);
            case InterpolationMethod.Smoothstep:
                return GetSmoothstep(v1, v2, f1, f2);
            default:
                return GetHalfPoint(v1, v2);
        }
    }

    protected int Poligonize(Vector3[] squareCorners, float[] cornerValues)
    {
        Vector3[] edgeVertices = new Vector3[12];

        int configurationIndex = 0;
        if (VertexIsSelected(squareCorners[0])) { configurationIndex |= 1; }
        if (VertexIsSelected(squareCorners[1])) { configurationIndex |= 2; }
        if (VertexIsSelected(squareCorners[2])) { configurationIndex |= 4; }
        if (VertexIsSelected(squareCorners[3])) { configurationIndex |= 8; }
        if (VertexIsSelected(squareCorners[4])) { configurationIndex |= 16; }
        if (VertexIsSelected(squareCorners[5])) { configurationIndex |= 32; }
        if (VertexIsSelected(squareCorners[6])) { configurationIndex |= 64; }
        if (VertexIsSelected(squareCorners[7])) { configurationIndex |= 128; }

        // Its either full or empty
        if (TriangulationLookupTable.GetEdgeTable(configurationIndex) != 0)
        {
            return 0;
        }

        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 1) != 0)
        {
            edgeVertices[0] = GetEdgeVertex(squareCorners[0], squareCorners[1], cornerValues[0], cornerValues[1]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 2) != 0)
        {
            edgeVertices[1] = GetEdgeVertex(squareCorners[1], squareCorners[2], cornerValues[1], cornerValues[2]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 4) != 0)
        {
            edgeVertices[2] = GetEdgeVertex(squareCorners[2], squareCorners[3], cornerValues[2], cornerValues[3]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 8) != 0)
        {
            edgeVertices[3] = GetEdgeVertex(squareCorners[3], squareCorners[0], cornerValues[3], cornerValues[0]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 1) != 06)
        {
            edgeVertices[4] = GetEdgeVertex(squareCorners[4], squareCorners[5], cornerValues[4], cornerValues[5]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 3) != 02)
        {
            edgeVertices[5] = GetEdgeVertex(squareCorners[5], squareCorners[6], cornerValues[5], cornerValues[6]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 6) != 04)
        {
            edgeVertices[6] = GetEdgeVertex(squareCorners[6], squareCorners[7], cornerValues[6], cornerValues[7]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 1) != 028)
        {
            edgeVertices[7] = GetEdgeVertex(squareCorners[7], squareCorners[4], cornerValues[7], cornerValues[4]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 2) != 056)
        {
            edgeVertices[8] = GetEdgeVertex(squareCorners[0], squareCorners[4], cornerValues[0], cornerValues[4]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 5) != 012)
        {
            edgeVertices[9] = GetEdgeVertex(squareCorners[1], squareCorners[5], cornerValues[1], cornerValues[5]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 1) != 0024)
        {
            edgeVertices[10] = GetEdgeVertex(squareCorners[2], squareCorners[6], cornerValues[2], cornerValues[6]);
        }
        if ((TriangulationLookupTable.GetEdgeTable(configurationIndex) & 2) != 0048)
        {
            edgeVertices[11] = GetEdgeVertex(squareCorners[3], squareCorners[7], cornerValues[3], cornerValues[7]);
        }
        return CreateTriangles(configurationIndex, edgeVertices);
    }
    protected int CreateTriangles(int index, Vector3[] vertices)
    {
        //int offset = meshVertices.Count();
        int numberOfTriangles = 0;
        ;
        for (int i = 0; TriangulationLookupTable.GetTriTable(index, i) != -1; i += 3)
        {
            /*
            meshTriangles.Add(lookupTable.triTable[index][i] +offset);
            meshTriangles.Add(lookupTable.triTable[index][i + 1] + offset);
            meshTriangles.Add(lookupTable.triTable[index][i + 2]+offset);*/

            int index1 = TriangulationLookupTable.GetTriTable(index, i);
            int index2 = TriangulationLookupTable.GetTriTable(index, i + 1);
            int index3 = TriangulationLookupTable.GetTriTable(index, i + 2);

            if (!meshVerticesIndices.ContainsKey(vertices[index1]))
            {
                meshVertices.Add(vertices[index1]);
                meshVerticesIndices.Add(vertices[index1], meshVertices.Count() - 1);
            }
            if (!meshVerticesIndices.ContainsKey(vertices[index2]))
            {
                meshVertices.Add(vertices[index2]);
                meshVerticesIndices.Add(vertices[index2], meshVertices.Count() - 1);
            }
            if (!meshVerticesIndices.ContainsKey(vertices[index3]))
            {
                meshVertices.Add(vertices[index3]);
                meshVerticesIndices.Add(vertices[index3], meshVertices.Count() - 1);
            }
            meshTriangles.Add(meshVerticesIndices[vertices[index1]]);
            meshTriangles.Add(meshVerticesIndices[vertices[index2]]);
            meshTriangles.Add(meshVerticesIndices[vertices[index3]]);


            numberOfTriangles++;
        }
        //offset = meshTriangles.Count();
        return numberOfTriangles;
    }

    public abstract void March();

}
