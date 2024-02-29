using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class NoiseVisualization : MonoBehaviour
{
    float[] values;

    private void Start()
    {
        //values = NoiseGenerator.GetNoise(16);
    }

    private void OnDrawGizmos()
    {
        if (values == null || values.Length == 0) return;

        for (int i = 0; i < GridMetrics.PointsPerChunk; i++)
        {
            for (int j = 0; j < GridMetrics.PointsPerChunk; j++)
            {
                for (int k = 0; k < GridMetrics.PointsPerChunk; k++)
                {
                    int index = i + GridMetrics.PointsPerChunk * (j + GridMetrics.PointsPerChunk * k);
                    float val = values[index];
                    Gizmos.color = Color.Lerp(Color.red, Color.green, val);
                    Gizmos.DrawCube(new Vector3(i, j, k), Vector3.one * .2f);
                }
            }
        }
    }
}
