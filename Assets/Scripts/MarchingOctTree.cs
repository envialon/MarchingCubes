using System;
using System.Collections.Generic;
using UnityEngine;

public class MarchingOctTree : Marcher
{
    Dictionary<Vector3, float> selectedVertices;
    OctTree octTree;



    protected override void Initialize()
    {
        base.Initialize();
    }


    void Start()
    {

    }

    public void MarchOctTree() { throw new NotImplementedException(); }

    public void AddSelectedVertex(Vector3 pos, float value)
    {
        if (IsPositionValid(pos))
        {
            selectedVertices.Add(pos, value);
            octTree.Insert(pos);
        }
    }

    public void RemoveSelectedVertex(Vector3 pos)
    {
        if (IsPositionValid(pos))
        {
            selectedVertices.Remove(pos);
            octTree.Remove(pos);
        }
    }

    public override void March()
    {
        throw new NotImplementedException();
    }

    protected override bool VertexIsSelected(in Vector3 pos)
    {
        throw new NotImplementedException();
    }
}
