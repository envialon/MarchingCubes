using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using UnityEngine;

public class MarchingCubes : Marcher
{
    public float opacity = 4f;
    public float brushRadius = 2;

    public MarchingCubes(int boundSize, float resolution, float threshold,InterpolationMethod method) : base(boundSize, resolution, threshold, method)
    {
        Initialize();
        this.threshold = threshold;
    }

    protected override void Initialize()
    {
        base.Initialize();
       InitializeValues(0);
    }

    public override void AddSelectedVertex(in Vector3 pos)
    {
        Vector3Int[] points = GetBrushPoints(pos);
        foreach (Vector3Int point in points)
        {
            values[point.x, point.y, point.z] += opacity;
        }
    }

    public override void RemoveSelectedVertex(in Vector3 pos)
    {
        Vector3Int[] points = GetBrushPoints(pos);
        foreach (Vector3Int point in points)
        {
            values[(int)point.x, (int)point.y, (int)point.z] -= opacity;
        }
    }

    private static float GetValue(in Vector3 pos, float resolution, in float[,,] values)
    {
        if (IsPositionValid(pos, values.GetLength(0)))
        {
            Vector3Int index = Vector3Int.FloorToInt(pos / resolution);
            return values[index.x, index.y, index.z];
        }
        return 0;
    }

    [BurstCompile]
    private static void March(int boundSize, float resolution, float threshold, InterpolationMethod interpolationMethod,
       in float[,,] values,
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

                    valueWindow[0] = GetValue(window[0], resolution, values);
                    valueWindow[1] = GetValue(window[1], resolution, values);
                    valueWindow[2] = GetValue(window[2], resolution, values);
                    valueWindow[3] = GetValue(window[3], resolution, values);
                    valueWindow[4] = GetValue(window[4], resolution, values);
                    valueWindow[5] = GetValue(window[5], resolution, values);
                    valueWindow[6] = GetValue(window[6], resolution, values);
                    valueWindow[7] = GetValue(window[7], resolution, values);


                    Poligonize(GenerateConfigurationIndexFromWindow(values, window, boundSize, resolution, threshold), window, valueWindow, threshold, interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
                }
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log("Marching cubes took " + sw.ElapsedMilliseconds + " ms");
    }


    [BurstCompile]
    protected static int GenerateConfigurationIndexFromWindow(in float[,,] values, in Vector3[] window, int boundsize, float resolution, float threshold)
    {
        int configurationIndex = 0;


        if (GetValue(window[0], resolution, values) > threshold) { configurationIndex |= 1; }
        if (GetValue(window[1], resolution, values) > threshold) { configurationIndex |= 2; }
        if (GetValue(window[2], resolution, values) > threshold) { configurationIndex |= 4; }
        if (GetValue(window[3], resolution, values) > threshold) { configurationIndex |= 8; }
        if (GetValue(window[4], resolution, values) > threshold) { configurationIndex |= 16; }
        if (GetValue(window[5], resolution, values) > threshold) { configurationIndex |= 32; }
        if (GetValue(window[6], resolution, values) > threshold) { configurationIndex |= 64; }
        if (GetValue(window[7], resolution, values) > threshold) { configurationIndex |= 128; }
        return configurationIndex;
    }

    public override ProceduralMeshInfo March()
    {
        March(boundSize, resolution, threshold, interpolationMethod, values, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
        return new ProceduralMeshInfo(meshVertices, meshTriangles);
    }
}
