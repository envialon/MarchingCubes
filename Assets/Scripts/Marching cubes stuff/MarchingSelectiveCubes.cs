using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Unity.Burst;

public class MarchingSelectiveCubes



    : Marcher
{
    HashSet<Vector3> selectedVertices;
    float[,,] values;

    public MarchingSelectiveCubes(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method) : base(boundSize, resolution, interpolationThreshold, method)
    {
        Initialize();
    }

    private void InitializeValues()
    {
        values = new float[boundSize, boundSize, boundSize];
        for (int i = 0; i < boundSize; i++)
        {
            for (int j = 0; j < boundSize; j++)
            {
                for (int k = 0; k < boundSize; k++)
                {
                    values[i, j, k] = UnityEngine.Random.Range(0, 1);
                }
            }
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        InitializeValues();
        selectedVertices = new HashSet<Vector3>();
    }

    public override void AddSelectedVertex(in Vector3 pos)
    {
       // if ((IsPositionValid(pos) && !selectedVertices.Contains(pos)))
       // {
       //     selectedVertices.Add(pos);
       //}
    }

    public override void RemoveSelectedVertex(in Vector3 pos)
    {
        //if (IsPositionValid(pos))
        //{
        //    selectedVertices.Remove(pos);
        //}
    }

    [BurstCompile]
    private static void March(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod interpolationMethod,
        in HashSet<Vector3> selectedVertices, in float[,,] values,
        ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
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

                    valueWindow[0] = values[0, 0, 0];
                    valueWindow[1] = values[0, 0, 0];
                    valueWindow[2] = values[0, 0, 0];
                    valueWindow[3] = values[0, 0, 0];
                    valueWindow[4] = values[0, 0, 0];
                    valueWindow[5] = values[0, 0, 0];
                    valueWindow[6] = values[0, 0, 0];
                    valueWindow[7] = values[0, 0, 0];

                    Poligonize(GenerateConfigurationIndexFromWindow(selectedVertices, window), window, valueWindow, interpolationThreshold, interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
                }
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log("Marching cubes took " + sw.ElapsedMilliseconds + " ms");
    }


    [BurstCompile]
    protected static int GenerateConfigurationIndexFromWindow(in HashSet<Vector3> selectedVertices, in Vector3[] window)
    {
        int configurationIndex = 0;
        if (selectedVertices.Contains(window[0])) { configurationIndex |= 1; }
        if (selectedVertices.Contains(window[1])) { configurationIndex |= 2; }
        if (selectedVertices.Contains(window[2])) { configurationIndex |= 4; }
        if (selectedVertices.Contains(window[3])) { configurationIndex |= 8; }
        if (selectedVertices.Contains(window[4])) { configurationIndex |= 16; }
        if (selectedVertices.Contains(window[5])) { configurationIndex |= 32; }
        if (selectedVertices.Contains(window[6])) { configurationIndex |= 64; }
        if (selectedVertices.Contains(window[7])) { configurationIndex |= 128; }
        return configurationIndex;
    }

    public override ProceduralMeshInfo March()
    { 
        March(boundSize, resolution, interpolationThreshold, interpolationMethod, selectedVertices, values, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
        return new ProceduralMeshInfo(meshVertices, meshTriangles);
    }
}
