using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Unity.Burst;
using UnityEngine.UI;
using System;

public class MarchingSelectiveCubes : Marcher
{
    bool firstMarch = false;
    Dictionary<Vector3, float> selectedVertices;

    public MarchingSelectiveCubes(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method) : base(boundSize, resolution, interpolationThreshold, method)
    {
        Initialize();
    }




    protected override void Initialize()
    {
        base.Initialize();
        selectedVertices = new Dictionary<Vector3, float>();
    }

    public override void AddSelectedVertex(in Vector3 pos)
    {
        if ((IsPositionValid(pos, boundSize) && !selectedVertices.ContainsKey(pos)))
        {
            selectedVertices.Add(pos, GetValue(pos));
        }
    }

    public override void RemoveSelectedVertex(in Vector3 pos)
    {
        if (IsPositionValid(pos, boundSize)&& !selectedVertices.ContainsKey(pos))
        {
            selectedVertices.Remove(pos);
        }
    }
    private static float GetValue(in Vector3 pos)
    {
        return Mathf.PerlinNoise(pos.x, pos.z) + Mathf.PerlinNoise(pos.z, pos.y);
        //if (IsPositionValid(pos, values.GetLength(0)))
        //{
        //    Vector3Int index = Vector3Int.FloorToInt(pos / resolution);
        //    return values[index.x, index.y, index.z];
        //}
        //return 0;
    }

    /// <summary>
    /// Used to march though the values the first time
    /// </summary>
    private void MarchThroughValues()

    {
        meshVerticesIndices.Clear();
        meshVertices.Clear();
        meshTriangles.Clear();

        Vector3[] window = new Vector3[8];
        float[] valueWindow = new float[8];

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (float i = 0; i < boundSize; i += resolution)
        {
            for (float j = 0; j < boundSize; j += resolution)
            {
                for (float k = 0; k < boundSize; k += resolution)
                {
                    window[0] = new Vector3(i, j, k);
                    window[1] = new Vector3(i + resolution, j, k);
                    window[2] = new Vector3(i + resolution, j + resolution, k);
                    window[3] = new Vector3(i, j + resolution, k);
                    window[4] = new Vector3(i, j, k + resolution);
                    window[5] = new Vector3(i + resolution, j, k + resolution);
                    window[6] = new Vector3(i + resolution, j + resolution, k + resolution);
                    window[7] = new Vector3(i, j + resolution, k + resolution);

                    for (int l = 0; l < 8; l++)
                    {
                        valueWindow[l] = GetValue(window[l]);
                        //if (/*valueWindow[l] > interpolationThreshold &&*/ !selectedVertices.ContainsKey(window[l])) { selectedVertices.Add(window[l], valueWindow[l]); }
                    }

                    Poligonize(GenerateConfigurationIndexFromWindow(selectedVertices, window), window, valueWindow, interpolationThreshold, interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
                }
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log("Marching cubes took " + sw.ElapsedMilliseconds + " ms");

    }

    [BurstCompile]
    private static void GetWindowsAroundPoint(in Vector3 pos, float resolution, ref Vector3[][] posWindows, ref float[][] valueWindows)
    {
        int windowIndex = 0;
        for (float i = -resolution; i <= 0; i += resolution)
        {

            for (float j = -resolution; j <= 0; j += resolution)
            {

                for (float k = -resolution; k <= 0; k += resolution)
                {
                    Vector3 currentPos = pos + new Vector3(i, j, k);
                    posWindows[windowIndex][0] = currentPos;
                    posWindows[windowIndex][1] = currentPos + new Vector3(resolution, 0, 0);
                    posWindows[windowIndex][2] = currentPos + new Vector3(resolution, resolution, 0);
                    posWindows[windowIndex][3] = currentPos + new Vector3(0, resolution, 0);
                    posWindows[windowIndex][4] = currentPos + new Vector3(0, 0, resolution);
                    posWindows[windowIndex][5] = currentPos + new Vector3(resolution, 0, resolution);
                    posWindows[windowIndex][6] = currentPos + new Vector3(resolution, resolution, resolution);
                    posWindows[windowIndex][7] = currentPos + new Vector3(0, resolution, resolution);

                    valueWindows[windowIndex][0] = GetValue(posWindows[windowIndex][0]);
                    valueWindows[windowIndex][1] = GetValue(posWindows[windowIndex][1]);
                    valueWindows[windowIndex][2] = GetValue(posWindows[windowIndex][2]);
                    valueWindows[windowIndex][3] = GetValue(posWindows[windowIndex][3]);
                    valueWindows[windowIndex][4] = GetValue(posWindows[windowIndex][4]);
                    valueWindows[windowIndex][5] = GetValue(posWindows[windowIndex][5]);
                    valueWindows[windowIndex][6] = GetValue(posWindows[windowIndex][6]);
                    valueWindows[windowIndex][7] = GetValue(posWindows[windowIndex][7]);
                    windowIndex++;
                }
            }
        }
    }

    [BurstCompile]
    private static void March(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod interpolationMethod,
        Dictionary<Vector3, float> selectedVertices,
        ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
    {
        Vector3[][] posWindows = new Vector3[8][];

        float[][] valueWindows = new float[8][];

        for (int i = 0; i < 8; i++)
        {
            posWindows[i] = new Vector3[8];
            valueWindows[i] = new float[8];
        }

        foreach (KeyValuePair<Vector3, float> pair in selectedVertices)
        {
            GetWindowsAroundPoint(pair.Key, resolution, ref posWindows, ref valueWindows);

            for (int i = 0; i < 8; i++)
            {
                Poligonize(GenerateConfigurationIndexFromWindow(selectedVertices, posWindows[i]), posWindows[i], valueWindows[i], interpolationThreshold, interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
            }
        }
    }


    [BurstCompile]
    protected static int GenerateConfigurationIndexFromWindow(in Dictionary<Vector3, float> selectedVertices, in Vector3[] window)
    {
        int configurationIndex = 0;
        if (selectedVertices.ContainsKey(window[0])) { configurationIndex |= 1; }
        if (selectedVertices.ContainsKey(window[1])) { configurationIndex |= 2; }
        if (selectedVertices.ContainsKey(window[2])) { configurationIndex |= 4; }
        if (selectedVertices.ContainsKey(window[3])) { configurationIndex |= 8; }
        if (selectedVertices.ContainsKey(window[4])) { configurationIndex |= 16; }
        if (selectedVertices.ContainsKey(window[5])) { configurationIndex |= 32; }
        if (selectedVertices.ContainsKey(window[6])) { configurationIndex |= 64; }
        if (selectedVertices.ContainsKey(window[7])) { configurationIndex |= 128; }
        return configurationIndex;
    }

    public override ProceduralMeshInfo March()
    {
        if (!firstMarch)
        {
            MarchThroughValues();
            firstMarch = true;
        }
        else
        {
            March(boundSize, resolution, interpolationThreshold, interpolationMethod, selectedVertices, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
        }
        return new ProceduralMeshInfo(meshVertices, meshTriangles);
    }
}
