using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    [SerializeField]
    private static ComputeShader noiseShader;

    [SerializeField] static float noiseScale = 1f;
    [SerializeField] static float amplitude = 5f;
    [SerializeField] static float frequency = 0.005f;
    [SerializeField] static int octaves = 8;
    [SerializeField, Range(0f, 1f)] static float groundPercent = 0.2f;

    private static ComputeBuffer buffer;


    private static void CreateBuffers(int boundSize)
    {
        noiseShader = (ComputeShader)Resources.Load("NoiseCompute");
        buffer = new ComputeBuffer(boundSize*boundSize*boundSize, sizeof(float));
    }

    private static void ReleaseBuffers()
    {
        buffer.Release();
    }

    public static float[] GetNoise(int boundSize, int numThreads = 8)
    {
        if(boundSize <= 0) {
            throw new System.Exception("boundSize can't be 0");
        }

        CreateBuffers(boundSize);
        float[] noiseValues = new float[boundSize * boundSize * boundSize];

        noiseShader.SetBuffer(0, "_Values", buffer);
        int groups = boundSize / numThreads;

        noiseShader.SetFloat("_NoiseScale", noiseScale);
        noiseShader.SetFloat("_Amplitude", amplitude);
        noiseShader.SetFloat("_Frequency", frequency);
        noiseShader.SetFloat("_GroundPercent", groundPercent);

        noiseShader.SetInt("_BoundSize", boundSize);
        noiseShader.SetInt("_Octaves", octaves);


        noiseShader.Dispatch(0, groups, groups, groups);
        buffer.GetData(noiseValues);
        ReleaseBuffers();
        return noiseValues;
    }

}
