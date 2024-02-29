using Unity.Mathematics;
using UnityEngine;

public class MarchingCubesGPU : Marcher
{
    public int numThreads = 8;


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

    private void CreateBuffers()
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


    public MarchingCubesGPU(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method) : base(boundSize, resolution, interpolationThreshold, method)
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

    public override ProceduralMeshInfo March()
    {
        CreateBuffers();
        marchingCubesComputeShader.SetBuffer(0, "_Triangles", triangleBuffer);
        marchingCubesComputeShader.SetBuffer(0, "_Values", valueBuffer);

        marchingCubesComputeShader.SetInt("_BoundSize", boundSize);
        marchingCubesComputeShader.SetFloat("_Threshold", threshold);

        valueBuffer.SetData(values);
        triangleBuffer.SetCounterValue(0);

        int groups = boundSize / numThreads;
        marchingCubesComputeShader.Dispatch(0, groups, groups, groups);

        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        triangleBuffer.GetData(triangles);

        ReleaseBuffers();

        return new ProceduralMeshInfo(triangles);
    }
}
