using System;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class Marcher : MonoBehaviour
{

    public enum InterpolationMethod
    {
        HalfPoint,
        Linear,
        Smoothstep,
    }

    protected MeshCollider meshCollider;
    #region MeshAttributes
    protected Dictionary<Vector3, int> meshVerticesIndices;
    protected List<Vector3> meshVertices;
    protected List<int> meshTriangles;


    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected Mesh mesh;
    public Material material;
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
        meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = material;

        mesh = new Mesh { name = "Procedural mesh (Plastimesh)" };
        meshFilter.mesh = mesh;

        meshVerticesIndices = new Dictionary<Vector3, int>();
        meshVertices = new List<Vector3>();
        meshTriangles = new List<int>();

        ClickOnScene.OnClickOnScene += ReactToClick;
    }

    #region Interpolation 
    protected static Vector3 GetHalfPoint(Vector3 v1, Vector3 v2)
    {
        return v1 + (v2 - v1) * 0.5f;
    }

    protected static Vector3 GetSmoothstep(Vector3 v1, Vector3 v2, float f1, float f2, float interpolationThreshold)
    {
        float t = (interpolationThreshold - f1) / (f2 - f1);
        t = t * t * (3 - 2 * t);

        return new Vector3(v1.x + t * (v2.x - v1.x),
            v1.y + t * (v2.y - v1.y),
            v1.z + t * (v2.z - v1.z)
        );
    }

    protected static Vector3 GetLinealInterpolation(Vector3 v1, Vector3 v2, float f1, float f2, float interpolationThreshold)
    {
        float t = (interpolationThreshold - f1) / (f2 - f1);
        return new Vector3(v1.x + t * (v2.x - v1.x),
                       v1.y + t * (v2.y - v1.y),
                       v1.z + t * (v2.z - v1.z));
    }
    #endregion

    private void ReactToClick(object sender, EventArgs e)
    {
        // Debug.Log("Reacting to click");
        ClickEventArgs clickEventArgs = (ClickEventArgs)e;
        if (clickEventArgs.clickType == ClickEventArgs.ClickType.LeftClick)
        {
            AddSelectedVertex(clickEventArgs.pos);
        }
        else if (clickEventArgs.clickType == ClickEventArgs.ClickType.RightClick)
        {
            RemoveSelectedVertex(clickEventArgs.pos);
        }
    }

    protected abstract int GenerateConfigurationIndexFromWindow(in Vector3[] window);

    public abstract void AddSelectedVertex(in Vector3 pos);
    public abstract void RemoveSelectedVertex(in Vector3 pos);

    protected static Vector3 GetEdgeVertex(in Vector3 v1, in Vector3 v2, float f1, float f2, float interpolationThreshold, InterpolationMethod interpolationMethod)
    {
        switch (interpolationMethod)
        {
            case InterpolationMethod.HalfPoint:
                return GetHalfPoint(v1, v2);
            case InterpolationMethod.Linear:
                return GetLinealInterpolation(v1, v2, f1, f2, interpolationThreshold);
            case InterpolationMethod.Smoothstep:
                return GetSmoothstep(v1, v2, f1, f2, interpolationThreshold);
            default:
                return GetHalfPoint(v1, v2);
        }
    }





    protected static int Poligonize(int configurationIndex, in Vector3[] window, in float[] cornerValues,
                                    float interpolationThreshold, InterpolationMethod interpolationMethod,
                                    ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
    {
        Vector3[] edgeVertices = new Vector3[12];



        int edgeIndex = TriangulationLookupTable.edgeTable[configurationIndex];

        // Its either full or empty
        if (edgeIndex == 0)
        {
            return 0;
        }

        if ((edgeIndex & 1) != 0)
        {
            edgeVertices[0] = GetEdgeVertex(window[0], window[1], cornerValues[0], cornerValues[1], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 2) != 0)
        {
            edgeVertices[1] = GetEdgeVertex(window[1], window[2], cornerValues[1], cornerValues[2], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 4) != 0)
        {
            edgeVertices[2] = GetEdgeVertex(window[2], window[3], cornerValues[2], cornerValues[3], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 8) != 0)
        {
            edgeVertices[3] = GetEdgeVertex(window[3], window[0], cornerValues[3], cornerValues[0], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 16) != 0)
        {
            edgeVertices[4] = GetEdgeVertex(window[4], window[5], cornerValues[4], cornerValues[5], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 32) != 0)
        {
            edgeVertices[5] = GetEdgeVertex(window[5], window[6], cornerValues[5], cornerValues[6], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 64) != 0)
        {
            edgeVertices[6] = GetEdgeVertex(window[6], window[7], cornerValues[6], cornerValues[7], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 128) != 0)
        {
            edgeVertices[7] = GetEdgeVertex(window[7], window[4], cornerValues[7], cornerValues[4], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 256) != 0)
        {
            edgeVertices[8] = GetEdgeVertex(window[0], window[4], cornerValues[0], cornerValues[4], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 512) != 0)
        {
            edgeVertices[9] = GetEdgeVertex(window[1], window[5], cornerValues[1], cornerValues[5], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 1024) != 0)
        {
            edgeVertices[10] = GetEdgeVertex(window[2], window[6], cornerValues[2], cornerValues[6], interpolationThreshold, interpolationMethod);
        }
        if ((edgeIndex & 2048) != 0)
        {
            edgeVertices[11] = GetEdgeVertex(window[3], window[7], cornerValues[3], cornerValues[7], interpolationThreshold, interpolationMethod);
        }
        return CreateTriangles(configurationIndex, edgeVertices, meshVertices, meshVerticesIndices, meshTriangles);
    }

    protected static int CreateTriangles(int index, in Vector3[] vertices, List<Vector3> meshVertices, Dictionary<Vector3, int> meshVerticesIndices, List<int> meshTriangles)
    {
        //int offset = meshVertices.Count();
        int numberOfTriangles = 0;
        ;
        for (int i = 0; TriangulationLookupTable.GetTriTable(index, i) != -1; i += 3)
        {

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
            meshTriangles.Add(meshVerticesIndices[vertices[index3]]);
            meshTriangles.Add(meshVerticesIndices[vertices[index2]]);
            meshTriangles.Add(meshVerticesIndices[vertices[index1]]);

            numberOfTriangles++;
        }
        return numberOfTriangles;
    }

    public abstract void March();

}
