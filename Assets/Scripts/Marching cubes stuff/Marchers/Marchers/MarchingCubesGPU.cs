using System.Diagnostics;
using System.Threading;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


public class MarchingCubesGPU : Marcher
{
    public int numThreads = 8;

    private static long msSum = 0;
    private static long marchCounts = 0;
    private static ComputeShader marchingCubesComputeShader;

    public struct Triangle
    {
        public float3 a;
        public float3 b;
        public float3 c;

        public static int SizeOf => sizeof(float) * 3 * 3;
    }

    ComputeBuffer triangleBuffer;
    ComputeBuffer triangleCountBuffer;
    ComputeBuffer valueBuffer;


    public MarchingCubesGPU()
    {
        LoadComputeShader();
    }

    private void CreateBuffers(int boundSize)
    {
        int boundSizeCubed = boundSize * boundSize * boundSize;
        triangleBuffer = new ComputeBuffer(5 * boundSizeCubed, Triangle.SizeOf, ComputeBufferType.Append);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        valueBuffer = new ComputeBuffer(boundSizeCubed, sizeof(float));
    }


    private void ReleaseBuffers()
    {
        triangleBuffer.Release();
        triangleCountBuffer.Release();
        valueBuffer.Release();
    }


    private void LoadComputeShader()
    {
        marchingCubesComputeShader = (ComputeShader)Resources.Load("MarchingCubesComputeShader");
        if (marchingCubesComputeShader == null)
        {
            throw new System.Exception("Failed to load the marching cubes compute shader.");
        }
    }
      
    private int ReadTriangleCount()
    {
        int[] triCount = { 0 };
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triCount);
        return triCount[0];
    }

    public override ProceduralMeshInfo March(in NativeArray<float> values, MarcherParams parameters)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        CreateBuffers(parameters.boundSize);
        marchingCubesComputeShader.SetBuffer(0, "_Triangles", triangleBuffer);
        marchingCubesComputeShader.SetBuffer(0, "_Values", valueBuffer);

        marchingCubesComputeShader.SetInt("_BoundSize", parameters.boundSize);
        marchingCubesComputeShader.SetFloat("_Threshold", parameters.isoLevel);
        marchingCubesComputeShader.SetFloat("_Step", parameters.step);
        valueBuffer.SetData(values);
        triangleBuffer.SetCounterValue(0);

        int groups = parameters.boundSize / numThreads;
        marchingCubesComputeShader.Dispatch(0, groups, groups, groups);

        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        triangleBuffer.GetData(triangles);

        ReleaseBuffers();

        sw.Stop();
        marchCounts++;
        msSum += sw.ElapsedMilliseconds;
        long avgMs = msSum / marchCounts;
        UnityEngine.Debug.ClearDeveloperConsole();
        UnityEngine.Debug.Log("Marching Cubes avg compute time " + avgMs + "ms");

        ReleaseBuffers();
        return new ProceduralMeshInfo(triangles);
    }
}
