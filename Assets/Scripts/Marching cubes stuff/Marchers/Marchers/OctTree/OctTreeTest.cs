using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTreeTest : MonoBehaviour
{
    OctTree octTree;
    // Start is called before the first frame update
    void Start()
    {
        octTree = new OctTree();
        Unity.Mathematics.int3 value = new Unity.Mathematics.int3(1, 0, 0);
        octTree.Insert(value, 1f);
        octTree.Insert(value * 10, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
