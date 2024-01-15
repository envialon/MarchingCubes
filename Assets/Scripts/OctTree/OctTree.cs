using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTree
{
    OctTreeNode root;
    HashSet<Vector3> points;
    int maxDepth;

    public OctTree(CubicVolume volume, int depth = 5)
    {

    }

    #region private methods    
    private bool IsPositionValid(in Vector3 pos)
    {
        return root.GetVolume().Contains(pos);
    }
    #endregion

    public void Insert(in Vector3 point)
    {
        throw new NotImplementedException();
    }

    public void Remove(in Vector3 point)
    {
        throw new NotImplementedException();
    }


}