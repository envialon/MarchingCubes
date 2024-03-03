using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MarchingCubes : Marcher
{


    public MarchingCubes(int boundSize, float resolution, float threshold, InterpolationMethod method) : base(boundSize, resolution, threshold, method)
    {
    }

    public MarchingCubes(Marcher other) : base(other)
    {
    }


    [BurstCompile]
    private static void March(int boundSize, float resolution, float threshold, InterpolationMethod interpolationMethod,
       in NativeArray<float> values,
        ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
    {
        meshVerticesIndices.Clear();
        meshVertices.Clear();
        meshTriangles.Clear();

        float3[] window = new float3[8];
        float[] valueWindow = new float[8];



        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = -1; i < boundSize; i++)
        {
            for (int j = -1; j < boundSize; j++)
            {
                for (int k = -1; k < boundSize; k++)
                {
                    window[0] = new Vector3(i, j, k);
                    window[1] = new Vector3(i + 1, j, k);
                    window[2] = new Vector3(i + 1, j + 1, k);
                    window[3] = new Vector3(i, j + 1, k);
                    window[4] = new Vector3(i, j, k + 1);
                    window[5] = new Vector3(i + 1, j, k + 1);
                    window[6] = new Vector3(i + 1, j + 1, k + 1);
                    window[7] = new Vector3(i, j + 1, k + 1);

                    valueWindow[0] = GetValue(window[0], values);
                    valueWindow[1] = GetValue(window[1], values);
                    valueWindow[2] = GetValue(window[2], values);
                    valueWindow[3] = GetValue(window[3], values);
                    valueWindow[4] = GetValue(window[4], values);
                    valueWindow[5] = GetValue(window[5], values);
                    valueWindow[6] = GetValue(window[6], values);
                    valueWindow[7] = GetValue(window[7], values);

                    window[0] += resolution;
                    window[1] += resolution;
                    window[2] += resolution;
                    window[3] += resolution;
                    window[4] += resolution;
                    window[5] += resolution;
                    window[6] += resolution;
                    window[7] += resolution;


                    Poligonize(GenerateConfigurationIndexFromWindow(values, window, boundSize, resolution, threshold), window, valueWindow, threshold, interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
                }
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log("Marching cubes took " + sw.ElapsedMilliseconds + " ms");
    }


    [BurstCompile]
    protected static int GenerateConfigurationIndexFromWindow(in NativeArray<float> values, in float3[] window, int boundsize, float resolution, float threshold)
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

    public override ProceduralMeshInfo March()
    {
        March(boundSize, resolution, threshold, interpolationMethod, values, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
        return new ProceduralMeshInfo(meshVertices, meshTriangles);
    }
}
