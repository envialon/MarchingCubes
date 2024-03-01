using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public abstract class Marcher
{

    public struct ProceduralMeshInfo
    {
        public Vector3[] meshVertices;
        public int[] meshTriangles;

        public ProceduralMeshInfo(List<Vector3> meshVertices, List<int> meshTriangles)
        {
            this.meshVertices = meshVertices.ToArray();
            this.meshTriangles = meshTriangles.ToArray();
        }

        public ProceduralMeshInfo(MarchingCubesGPU.Triangle[] triangles)
        {
            List<Vector3> meshVerticesList = new List<Vector3>();
            Dictionary<Vector3, int> vertexIndex = new Dictionary<Vector3, int>();
            meshTriangles = new int[triangles.Length * 3];

            for (int i = 0; i < triangles.Length; i++)
            {
                MarchingCubesGPU.Triangle tri = triangles[i];

                if (!vertexIndex.ContainsKey(tri.a)) { meshVerticesList.Add(tri.a); vertexIndex.Add(tri.a, meshVerticesList.Count - 1); }
                if (!vertexIndex.ContainsKey(tri.b)) { meshVerticesList.Add(tri.b); vertexIndex.Add(tri.b, meshVerticesList.Count - 1); }
                if (!vertexIndex.ContainsKey(tri.c)) { meshVerticesList.Add(tri.c); vertexIndex.Add(tri.c, meshVerticesList.Count - 1); }

                int index = i * 3;
                meshTriangles[index] = vertexIndex[tri.a];
                meshTriangles[index + 1] = vertexIndex[tri.b];
                meshTriangles[index + 2] = vertexIndex[tri.c];

            }
            meshVertices = meshVerticesList.ToArray(); ;
        }

    }

    public enum InterpolationMethod
    {
        HalfPoint,
        Linear,
        Smoothstep,
    }

    #region MeshAttributes
    protected Dictionary<Vector3, int> meshVerticesIndices;
    public List<Vector3> meshVertices;
    public List<int> meshTriangles;
    #endregion

    public float opacity = 0.1f;
    public int boundSize;
    public float resolution;
    public float threshold;
    public InterpolationMethod interpolationMethod;
    public NativeArray<float> values;



    public void InitializeValues(float defaultValue = 0)
    {
        //values = new NativeArray<float>(boundSize * boundSize * boundSize, Allocator.Persistent);
        //for (int i = 0; i < boundSize; i++)
        //{
        //    for (int j = 0; j < boundSize; j++)
        //    {
        //        for (int k = 0; k < boundSize; k++)
        //        {
        //            float value = defaultValue == -1 ? UnityEngine.Random.Range(0f, 1f) : defaultValue;
        //            SetValue(i, j, k, value, ref values);
        //        }
        //    }
        //}
        values = new NativeArray<float>(NoiseGenerator.GetNoise(boundSize), Allocator.Persistent);
    }

    protected virtual void InitializeMeshAttributes()
    {
        meshVerticesIndices = new Dictionary<Vector3, int>();
        meshVertices = new List<Vector3>();
        meshTriangles = new List<int>();
    }

    public Marcher(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method)
    {
        InitializeMeshAttributes();
        this.boundSize = boundSize;
        this.resolution = resolution;
        this.threshold = interpolationThreshold;
        this.interpolationMethod = method;
        InitializeValues();
    }

    public Marcher(Marcher other)
    {
        InitializeMeshAttributes();
        this.values = new NativeArray<float>(other.values.Length, Allocator.Persistent);
        this.values.CopyFrom(other.values);
        this.boundSize = other.boundSize;
        this.resolution = other.resolution;
        this.threshold  = other.threshold;  
        this.interpolationMethod = other.interpolationMethod;

    }

    ~Marcher()
    {
        values.Dispose();
    }

    [BurstCompile]
    protected static bool IsPositionValid(in Vector3 pos, int boundSize)
    {
        bool output = pos.x >= 0 && pos.x < boundSize && pos.y >= 0 && pos.y < boundSize && pos.y >= 0 && pos.y < boundSize && pos.z >= 0 && pos.z < boundSize;
        return output;
    }

    [BurstCompile]
    protected static bool IsPositionValid(in int3 pos, int boundSize)
    {
        bool output = pos.x >= 0 && pos.x < boundSize && pos.y >= 0 && pos.y < boundSize && pos.y >= 0 && pos.y < boundSize && pos.z >= 0 && pos.z < boundSize;
        return output;
    }

    #region Getters and setters

    protected static void SetValue(int x, int y, int z, float value, ref NativeArray<float> values)
    {
        int boundSize = (int)math.pow(values.Length, 1f / 3f);
        values[x + y * boundSize + z * boundSize * boundSize] = value;
    }


    protected static void SetValue(int3 pos, float value, ref NativeArray<float> values)
    {
        int boundSize = (int)math.pow(values.Length, 1f / 3f);
        values[pos.x + pos.y * boundSize + pos.z * boundSize * boundSize] = value;
    }


    [BurstCompile]
    protected static float GetValue(in int3 pos, in NativeArray<float> values)
    {
        int boundSize = (int)math.pow(values.Length, 1f / 3f);
        if (IsPositionValid(pos, boundSize))
        {
            return values[pos.x + pos.y * boundSize + pos.z * boundSize * boundSize];
        }
        return 0;
    }

    [BurstCompile]
    protected static float GetValue(in float3 pos, in NativeArray<float> values)
    {
        int3 index = (int3)pos;
        return GetValue(index, values);
    }
    #endregion

    #region Interpolation 

    [BurstCompile]
    protected static Vector3 GetHalfPoint(Vector3 v1, Vector3 v2)
    {
        return v1 + (v2 - v1) * 0.5f;
    }

    [BurstCompile]
    protected static Vector3 GetSmoothstep(Vector3 v1, Vector3 v2, float f1, float f2, float threshold)
    {

        if (Mathf.Abs(threshold - f1) < 0.0001f)
        {
            return v1;
        }
        if (Mathf.Abs(threshold - f2) < 0.0001f)
        {
            return v2;
        }
        if (Mathf.Abs(f1 - f2) < 0.0001f)
        {
            return v1;
        }
        float t = (threshold - f1) / (f2 - f1);
        t = t * t * (3 - 2 * t);

        return new Vector3(v1.x + t * (v2.x - v1.x),
            v1.y + t * (v2.y - v1.y),
            v1.z + t * (v2.z - v1.z)
        );
    }

    [BurstCompile]
    protected static Vector3 GetLinealInterpolation(Vector3 v1, Vector3 v2, float f1, float f2, float threshold)
    {
        if (Mathf.Abs(threshold - f1) < 0.0001f)
        {
            return v1;
        }
        if (Mathf.Abs(threshold - f2) < 0.0001f)
        {
            return v2;
        }
        if (Mathf.Abs(f1 - f2) < 0.0001f)
        {
            return v1;
        }
        float t = (threshold - f1) / (f2 - f1);
        Vector3 returnVal = new Vector3(v1.x + t * (v2.x - v1.x),
                       v1.y + t * (v2.y - v1.y),
                       v1.z + t * (v2.z - v1.z));
        return returnVal;
    }
    #endregion

    #region Click and Value manipulation stuff
     protected Vector3Int[] GetBrushPoints(in Vector3 pos, float brushRadius = 2)
    {
        float squareRadius = brushRadius * brushRadius;
        HashSet<Vector3Int> output = new HashSet<Vector3Int>();
        Vector3[] offsets = new Vector3[8];

        for (int i = 0; i < brushRadius; i++)
        {
            for (int j = 0; j < brushRadius; j++)
            {
                for (int k = 0; k < brushRadius; k++)
                {
                    offsets[0] = new Vector3(i, j, k);
                    offsets[1] = new Vector3(-i, j, k);
                    offsets[2] = new Vector3(i, -j, k);
                    offsets[3] = new Vector3(i, j, -k);
                    offsets[4] = new Vector3(-i, -j, k);
                    offsets[5] = new Vector3(-i, j, -k);
                    offsets[6] = new Vector3(i, -j, -k);
                    offsets[7] = new Vector3(-i, -j, -k);

                    for (int l = 0; l < offsets.Length; l++)
                    {
                        Vector3Int point = Vector3Int.FloorToInt(pos + offsets[l]);
                        if (offsets[l].sqrMagnitude < squareRadius && IsPositionValid(point, boundSize))
                        {
                            output.Add(point);
                        }
                    }

                }
            }
        }
        return output.ToArray();
    }


    public virtual void AddSelectedVertex(in Vector3 pos, float opacity)
    {
        Vector3Int[] points = GetBrushPoints(pos);
        foreach (Vector3Int point in points)
        {
            int3 p = new int3(point.x, point.y, point.z);
            SetValue(p, GetValue(p, values) + opacity, ref values);
        }
    }
    public virtual void RemoveSelectedVertex(in Vector3 pos, float opacity)
    {
        Vector3Int[] points = GetBrushPoints(pos);
        foreach (Vector3Int point in points)
        {
            int3 p = new int3(point.x, point.y, point.z);
            SetValue(p, GetValue(p, values) - opacity, ref values);
        }
    }
    #endregion

    #region Marching cubes core methods
    [BurstCompile]
    protected static Vector3 GetEdgeVertex(in Vector3 v1, in Vector3 v2, float f1, float f2, float threshold, InterpolationMethod interpolationMethod)
    {
        switch (interpolationMethod)
        {
            case InterpolationMethod.HalfPoint:
                return GetHalfPoint(v1, v2);
            case InterpolationMethod.Linear:
                return GetLinealInterpolation(v1, v2, f1, f2, threshold);
            case InterpolationMethod.Smoothstep:
                return GetSmoothstep(v1, v2, f1, f2, threshold);
            default:
                return GetHalfPoint(v1, v2);
        }
    }

    [BurstCompile]
    protected static int Poligonize(int configurationIndex, in float3[] window, in float[] cornerValues,
                                    float threshold, InterpolationMethod interpolationMethod,
                                    ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
    {
        float3[] edgeVertices = new float3[12];

        int edgeIndex = TriangulationLookupTable.edgeTable[configurationIndex];

        // Its either full or empty
        if (edgeIndex == 0)
        {
            return 0;
        }

        if ((edgeIndex & 1) != 0)
        {
            edgeVertices[0] = GetEdgeVertex(window[0], window[1], cornerValues[0], cornerValues[1], threshold, interpolationMethod);
        }
        if ((edgeIndex & 2) != 0)
        {
            edgeVertices[1] = GetEdgeVertex(window[1], window[2], cornerValues[1], cornerValues[2], threshold, interpolationMethod);
        }
        if ((edgeIndex & 4) != 0)
        {
            edgeVertices[2] = GetEdgeVertex(window[2], window[3], cornerValues[2], cornerValues[3], threshold, interpolationMethod);
        }
        if ((edgeIndex & 8) != 0)
        {
            edgeVertices[3] = GetEdgeVertex(window[3], window[0], cornerValues[3], cornerValues[0], threshold, interpolationMethod);
        }
        if ((edgeIndex & 16) != 0)
        {
            edgeVertices[4] = GetEdgeVertex(window[4], window[5], cornerValues[4], cornerValues[5], threshold, interpolationMethod);
        }
        if ((edgeIndex & 32) != 0)
        {
            edgeVertices[5] = GetEdgeVertex(window[5], window[6], cornerValues[5], cornerValues[6], threshold, interpolationMethod);
        }
        if ((edgeIndex & 64) != 0)
        {
            edgeVertices[6] = GetEdgeVertex(window[6], window[7], cornerValues[6], cornerValues[7], threshold, interpolationMethod);
        }
        if ((edgeIndex & 128) != 0)
        {
            edgeVertices[7] = GetEdgeVertex(window[7], window[4], cornerValues[7], cornerValues[4], threshold, interpolationMethod);
        }
        if ((edgeIndex & 256) != 0)
        {
            edgeVertices[8] = GetEdgeVertex(window[0], window[4], cornerValues[0], cornerValues[4], threshold, interpolationMethod);
        }
        if ((edgeIndex & 512) != 0)
        {
            edgeVertices[9] = GetEdgeVertex(window[1], window[5], cornerValues[1], cornerValues[5], threshold, interpolationMethod);
        }
        if ((edgeIndex & 1024) != 0)
        {
            edgeVertices[10] = GetEdgeVertex(window[2], window[6], cornerValues[2], cornerValues[6], threshold, interpolationMethod);
        }
        if ((edgeIndex & 2048) != 0)
        {
            edgeVertices[11] = GetEdgeVertex(window[3], window[7], cornerValues[3], cornerValues[7], threshold, interpolationMethod);
        }
        return CreateTriangles(configurationIndex, edgeVertices, meshVertices, meshVerticesIndices, meshTriangles);
    }

    [BurstCompile]
    protected static int CreateTriangles(int index, in float3[] vertices, List<Vector3> meshVertices, Dictionary<Vector3, int> meshVerticesIndices, List<int> meshTriangles)
    {
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
    #endregion

    public abstract ProceduralMeshInfo March();
}
