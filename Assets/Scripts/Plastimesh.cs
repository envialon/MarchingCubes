using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plastimesh : MonoBehaviour
{

    #region MeshAttributes
    private Dictionary<Vector3, int> meshVerticesIndices;
    private List<Vector3> meshVertices;
    private List<int> meshTriangles;
    #endregion
    
    Vector3 boundSize;
    float resolution;

    double interpolationThreshold;
    Dictionary<Vector3, float> selectedVertices;

    OctTree octTree;
    TriangulationLookupTable lookupTable;

    Mesh mesh;

 
    private int CreateTriangles(int index, List<Vector3> vertices)
    {
        throw new NotImplementedException();
    }
    private int Poligonize(Vector3[] cubeVertices)
    {
        throw new NotImplementedException();
    }

    void Start()
    {

    }

    public void MarchOctTree() { throw new NotImplementedException(); }

    public void MarchCubes() { throw new NotImplementedException(); }

    public void AddSelectedVertex(Vector3 pos, float value) { throw new NotImplementedException(); }

    public void RemoveSelectedVertex(Vector3 pos) { throw new NotImplementedException(); }
       
}
