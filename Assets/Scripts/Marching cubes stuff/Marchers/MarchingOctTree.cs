using Codice.CM.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MarchingOctTree : Marcher
{
    Dictionary<Vector3, float> selectedVertices;
    OctTree octTree;

    public MarchingOctTree(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod method) : base(boundSize, resolution, interpolationThreshold, method)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
    }


    public void MarchOctTree() { throw new NotImplementedException(); }

    public override void AddSelectedVertex(in Vector3 pos)
    {
        //if (IsPositionValid(pos) && !selectedVertices.ContainsKey(pos))
        //{
        //   // selectedVertices.Add(pos, value[]);
        //    octTree.Insert(pos);
        //}
    }

    public override void RemoveSelectedVertex(in  Vector3 pos)
    {
        //if (IsPositionValid(pos))
        //{
        //    selectedVertices.Remove(pos);
        //    octTree.Remove(pos);
        //}
    }

    private static void March(int boundSize, float resolution, float interpolationThreshold, InterpolationMethod interpolationMethod,
        in HashSet<Vector3> selectedVertices, in float[,,] values,
        ref List<Vector3> meshVertices, ref Dictionary<Vector3, int> meshVerticesIndices, ref List<int> meshTriangles)
    {
        throw new NotImplementedException();
    }

    public override ProceduralMeshInfo March()
    {
        throw new NotImplementedException();
    }
}
