using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MarchingCubesGPU : Marcher
{
    [SerializeField]
    private ComputeShader marchingCubesComputeShader;

    [SerializeField] 


    public MarchingCubesGPU(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method) : base(boundSize, resolution, interpolationThreshold, method)
    {
    }

    public override ProceduralMeshInfo March()
    {
        throw new System.NotImplementedException();
    }
}
