using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : Marcher
{
   

    Dictionary<Vector3, float> selectedVertices;
    Mesh mesh;


    protected override void Initialize()
    {
        base.Initialize();
    }


    void Start()
    {

    }
    public void AddSelectedVertex(Vector3 pos, float value)
    {
        if (IsPositionValid(pos))
        {
            selectedVertices.Add(pos, value);
        }
    }

    public void RemoveSelectedVertex(Vector3 pos)
    {
        if (IsPositionValid(pos))
        {
            selectedVertices.Remove(pos);
        }
    }

    public override void March()
    {
        meshVerticesIndices.Clear();
        meshVertices.Clear(); 
        meshTriangles.Clear();
        mesh.Clear();

        Vector3[] window = new Vector3[8];
        float[] values = new float[8];
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
                    Poligonize(window, values);
                }
            }
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();


        }

    }

    protected override bool VertexIsSelected(in Vector3 pos)
    {
       return selectedVertices.ContainsKey(pos);
    }
}
