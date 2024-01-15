using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MarchingCubes : Marcher
{
   

    Dictionary<Vector3, float> selectedVertices;
    float[,,] values;

    private void InitializeValues()
    {
        values = new float[boundSize, boundSize, boundSize];
        for(int i = 0; i < boundSize; i++)
        {
            for(int j = 0; j < boundSize; j++)
            {
                for(int k = 0; k < boundSize; k++)
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
        selectedVertices = new Dictionary<Vector3, float>(); 
    }


    void Start()
    {
        Initialize();
    }

    public override void AddSelectedVertex(in Vector3 pos)
    {
        if ((IsPositionValid(pos) && !selectedVertices.ContainsKey(pos)))
        {
            selectedVertices.Add(pos, values[(int)pos.x, (int)pos.y, (int)pos.z]);
            March();
        }
    }

    public override void RemoveSelectedVertex(in Vector3 pos)
    {
        if (IsPositionValid(pos))
        {
            selectedVertices.Remove(pos);
            March();
        }
    }

    public override void March()
    {
        meshVerticesIndices.Clear();
        meshVertices.Clear(); 
        meshTriangles.Clear();

        Vector3[] window = new Vector3[8];
        float[] valueWindow = new float[8];   
        
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
                    
                    for(int h = 0; h < 8; h++)
                    {
                        //this wont work with resolutions other than 1
                        valueWindow[h] = values[(int)window[h].x % boundSize, (int)window[h].y % boundSize, (int)window[h].z % boundSize];
                    }
                    
                    Poligonize(window, valueWindow);
                }
            }
        }
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }

    protected override bool VertexIsSelected(in Vector3 pos)
    {
       return selectedVertices.ContainsKey(pos);
    }
}
