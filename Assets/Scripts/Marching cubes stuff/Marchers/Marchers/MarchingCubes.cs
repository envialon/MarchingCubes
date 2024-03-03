using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MarchingCubes : Marcher
{
    #region MeshAttributes
    protected NativeHashMap<float3, int> meshVerticesIndices;
    public NativeList<float3> meshVertices;
    public NativeList<int> meshTriangles;
    #endregion

    private static TriangulationLookupTable triangulationTable;

    private void Initialize()
    {
        meshVerticesIndices = new NativeHashMap<float3, int>(0, Allocator.Persistent);
        meshVertices = new NativeList<float3>(0, Allocator.Persistent);
        meshTriangles = new NativeList<int>(0, Allocator.Persistent);
        triangulationTable = new TriangulationLookupTable();
    }
      
    public MarchingCubes()
    {
    }

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
    protected static float3 GetLinealInterpolation(float3 v1, float3 v2, float f1, float f2, float threshold)
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
        float3 returnVal = new float3(v1.x + t * (v2.x - v1.x),
                       v1.y + t * (v2.y - v1.y),
                       v1.z + t * (v2.z - v1.z));
        return returnVal;
    }
    #endregion

    #region Marching cubes core methods

    [BurstCompile]
    protected static float3 GetEdgeVertex(in float3 v1, in float3 v2, float f1, float f2, float threshold, InterpolationMethod interpolationMethod)
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
    protected static bool IsPositionValid(in int3 pos, int boundSize)
    {
        bool output = pos.x >= 0 && pos.x < boundSize &&
                      pos.y >= 0 && pos.y < boundSize &&
                      pos.z >= 0 && pos.z < boundSize;
        return output;
    }

    [BurstCompile]
    protected static float GetValue(in float3 pos, in NativeArray<float> values)
    {
        int3 index = (int3)pos; int boundSize = (int)math.pow(values.Length, 1f / 3f);
        if (IsPositionValid(index, boundSize))
        {
            return values[index.x + index.y * boundSize + index.z * boundSize * boundSize];
        }
        return 0;
    }

    [BurstCompile]
    protected static int GenerateConfigurationIndexFromWindow(in NativeArray<float> values, in float3[] window, float threshold)
    {
        int configurationIndex = 0;

        if (GetValue(window[0], values) > threshold) { configurationIndex |= 1; }
        if (GetValue(window[1], values) > threshold) { configurationIndex |= 2; }
        if (GetValue(window[2], values) > threshold) { configurationIndex |= 4; }
        if (GetValue(window[3], values) > threshold) { configurationIndex |= 8; }
        if (GetValue(window[4], values) > threshold) { configurationIndex |= 16; }
        if (GetValue(window[5], values) > threshold) { configurationIndex |= 32; }
        if (GetValue(window[6], values) > threshold) { configurationIndex |= 64; }
        if (GetValue(window[7], values) > threshold) { configurationIndex |= 128; }
        return configurationIndex;
    }
    
    [BurstCompile]
    protected static int Poligonize(int configurationIndex, in float3[] window, in float[] cornerValues,
                                    float threshold, InterpolationMethod interpolationMethod,
                                    ref NativeList<float3> meshVertices, ref NativeHashMap<float3, int> meshVerticesIndices, ref NativeList<int> meshTriangles)
    {
        float3[] edgeVertices = new float3[12];

        int edgeIndex = triangulationTable.edgeTable[configurationIndex];

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
        return CreateTriangles(configurationIndex, edgeVertices, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
    }

    [BurstCompile]
    protected static int CreateTriangles(int index, in float3[] vertices, ref NativeList<float3> meshVertices, ref NativeHashMap<float3, int> meshVerticesIndices, ref NativeList<int> meshTriangles)
    {
        int numberOfTriangles = 0;

        for (int i = 0; triangulationTable.GetTriTable(index, i) != -1; i += 3)
        {

            int index1 = triangulationTable.GetTriTable(index, i);
            int index2 = triangulationTable.GetTriTable(index, i + 1);
            int index3 = triangulationTable.GetTriTable(index, i + 2);

            if (!meshVerticesIndices.ContainsKey(vertices[index1]))
            {
                meshVertices.Add(vertices[index1]);
                meshVerticesIndices.Add(vertices[index1], meshVertices.Length - 1);
            }
            if (!meshVerticesIndices.ContainsKey(vertices[index2]))
            {
                meshVertices.Add(vertices[index2]);
                meshVerticesIndices.Add(vertices[index2], meshVertices.Length - 1);
            }
            if (!meshVerticesIndices.ContainsKey(vertices[index3]))
            {
                meshVertices.Add(vertices[index3]);
                meshVerticesIndices.Add(vertices[index3], meshVertices.Length - 1);
            }
            meshTriangles.Add(meshVerticesIndices[vertices[index3]]);
            meshTriangles.Add(meshVerticesIndices[vertices[index2]]);
            meshTriangles.Add(meshVerticesIndices[vertices[index1]]);

            numberOfTriangles++;
        }
        return numberOfTriangles;
    }


    #endregion

    [BurstCompile]
    public override ProceduralMeshInfo March(in NativeArray<float> values, MarcherParams parameters)
    {
        Initialize();

        float3[] window = new float3[8];
        float[] valueWindow = new float[8];

        Stopwatch sw = new Stopwatch();
        sw.Start();

        for (int i = -1; i < parameters.boundSize; i++)
        {
            for (int j = -1; j < parameters.boundSize; j++)
            {
                for (int k = -1; k < parameters.boundSize; k++)
                {
                    window[0] = new float3(i, j, k);
                    window[1] = new float3(i + 1, j, k);
                    window[2] = new float3(i + 1, j + 1, k);
                    window[3] = new float3(i, j + 1, k);
                    window[4] = new float3(i, j, k + 1);
                    window[5] = new float3(i + 1, j, k + 1);
                    window[6] = new float3(i + 1, j + 1, k + 1);
                    window[7] = new float3(i, j + 1, k + 1);

                    valueWindow[0] = GetValue(window[0], values);
                    valueWindow[1] = GetValue(window[1], values);
                    valueWindow[2] = GetValue(window[2], values);
                    valueWindow[3] = GetValue(window[3], values);
                    valueWindow[4] = GetValue(window[4], values);
                    valueWindow[5] = GetValue(window[5], values);
                    valueWindow[6] = GetValue(window[6], values);
                    valueWindow[7] = GetValue(window[7], values);

                    window[0] *= parameters.step;
                    window[1] *= parameters.step;
                    window[2] *= parameters.step;
                    window[3] *= parameters.step;
                    window[4] *= parameters.step;
                    window[5] *= parameters.step;
                    window[6] *= parameters.step;
                    window[7] *= parameters.step;

                    Poligonize(GenerateConfigurationIndexFromWindow(values, window, parameters.isoLevel), window, valueWindow, parameters.isoLevel, parameters.interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
                }
            }
        }

        sw.Stop();
        UnityEngine.Debug.Log("Marching cubes took " + sw.ElapsedMilliseconds + " ms");
        ProceduralMeshInfo meshInfo = new ProceduralMeshInfo(meshVertices, meshTriangles);
        return meshInfo;
    }


}
