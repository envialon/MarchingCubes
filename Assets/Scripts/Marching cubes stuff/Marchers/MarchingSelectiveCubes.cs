using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;

/// <summary>
/// This Version of the Marcher only generates the mesh around the selected vertices
/// </summary>
public class MarchingSelectiveCubes : Marcher
{
    HashSet<Vector3> selectedVertices;

    public MarchingSelectiveCubes(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method) : base(boundSize, resolution, interpolationThreshold, method)
    {
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        selectedVertices = new HashSet<Vector3>();
    }

    public override void AddSelectedVertex(in Vector3 pos)
    {
        if ((IsPositionValid(pos, boundSize) && !selectedVertices.Contains(pos)))
        {
            selectedVertices.Add(pos);
        }
    }

    public override void RemoveSelectedVertex(in Vector3 pos)
    {
        if (IsPositionValid(pos, boundSize)&& !selectedVertices.Contains(pos))
        {
            selectedVertices.Remove(pos);
        }
    }

    [BurstCompile]
    private static float GetValue(in Vector3 pos)
    {
        return (float)Random.Range(0.1f, 1);
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
        HashSet<Vector3> selectedVertices,
        ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
    {
        Vector3[][] posWindows = new Vector3[8][];
        float[][] valueWindows = new float[8][];

        for (int i = 0; i < 8; i++)
        {
            posWindows[i] = new Vector3[8];
            valueWindows[i] = new float[8];
        }

        foreach (Vector3 point in selectedVertices)
        {
            GetWindowsAroundPoint(point, resolution, ref posWindows, ref valueWindows);

            for (int i = 0; i < 8; i++)
            {
                Poligonize(GenerateConfigurationIndexFromWindow(selectedVertices, posWindows[i]), posWindows[i], valueWindows[i], interpolationThreshold, interpolationMethod, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
            }
        }
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
        March(boundSize, resolution, interpolationThreshold, interpolationMethod, selectedVertices, ref meshVertices, ref meshVerticesIndices, ref meshTriangles);
        return new ProceduralMeshInfo(meshVertices, meshTriangles);
    }
}
