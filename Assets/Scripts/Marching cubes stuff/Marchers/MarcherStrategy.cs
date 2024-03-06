using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.NotBurstCompatible;

public struct ProceduralMeshInfo
{
    public NativeArray<float3> meshVertices;
    public NativeArray<int> meshTriangles;

    public ProceduralMeshInfo(NativeList<float3> meshVertices, NativeList<int> meshTriangles)
    {
        this.meshVertices = meshVertices.AsArray();
        this.meshTriangles = meshTriangles.AsArray();
    }

    public ProceduralMeshInfo(MarchingCubesGPU.Triangle[] triangles)
    {
        List<Vector3> meshVerticesList = new List<Vector3>();
        Dictionary<Vector3, int> vertexIndex = new Dictionary<Vector3, int>();
        meshTriangles = new NativeArray<int>(triangles.Length * 3, Allocator.Persistent);

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
        NativeArray<Vector3> vert = new NativeArray<Vector3>(meshVerticesList.ToArray(), Allocator.Persistent);
        meshVertices = vert.Reinterpret<float3>();
    }

    public void DealocateMemory()
    {
        meshVertices.Dispose();
        meshTriangles.Dispose();
    }

}

public class MarcherStrategy
{
    public enum MarcherType
    {
        MarchingCubes,
        MarchingCubesGPU
    }

    public MarcherType currentMarchertype = MarcherType.MarchingCubes;
    Marcher[] marchers;

    NativeArray<float> values;


    public int boundSize;
    public int step;
    public float isoLevel;


    public float opacity = 0.1f;

    public void InitializeValues()
    {
        if (boundSize == 0) { throw new System.Exception("BoundSize is set to 0"); }
        values = new NativeArray<float>(NoiseGenerator.GetNoise(boundSize), Allocator.Persistent);
    }

    private void Initialize()
    {
        InitializeValues();

        marchers = new Marcher[]{
            new MarchingCubes(),
            new MarchingCubesGPU()
        };
    }

    public MarcherStrategy(int boundSize, int step, float isoLevel, MarcherType marcherType = MarcherType.MarchingCubes)
    {
        UpdateAttributes(boundSize, step, isoLevel, marcherType);
        Initialize();
    }

    ~MarcherStrategy()
    {
        values.Dispose();
    }

    #region Getters and setters

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

    public void UpdateAttributes(int boundSize = 16, int step = 1, float isoLevel = .5f, MarcherType marcherType = MarcherType.MarchingCubes)
    {

        if (step <= 0) throw new System.Exception("step can't be set to values < 0.");
        if (boundSize <= 0) throw new System.Exception("boundSize can't be set to values < 0.");
        this.boundSize = boundSize;
        this.step = step;
        this.isoLevel = isoLevel;
        this.currentMarchertype = marcherType;
    }

    public ProceduralMeshInfo March()
    {
        return marchers[(int)currentMarchertype].March(in values,
            new Marcher.MarcherParams
            {
                boundSize = boundSize,
                step = step,
                isoLevel = isoLevel,
                interpolationMethod = Marcher.InterpolationMethod.HalfPoint
            });
    }

}
