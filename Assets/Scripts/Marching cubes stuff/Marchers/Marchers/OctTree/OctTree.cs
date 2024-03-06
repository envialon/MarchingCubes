using Codice.Client.BaseCommands;
using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class OctTree
{
    const int MaxDepth = 21; // max octant digits we can group in a ulong (64/3)
    uint dimension; // must be power of two
    int depth;
    OctTreeBranch root;

    public OctTree()
    {
        depth = 1;
        dimension = (1u << depth);

        root = new OctTreeBranch();
    }


    private bool PointIsContained(in int3 pos)
    {
        return Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y), Mathf.Abs(pos.z)) > dimension;
    }

    private uint3 NormalizePosition(in int3 pos)
    {
        uint minPos = (uint)-(1 << (int)dimension);
        return new uint3((uint)pos.x - minPos, (uint)pos.y - minPos, (uint)pos.z - minPos);
    }


    private OctTreeBranch CreateBranch()
    {
        return new OctTreeBranch();
    }

    private void GrowTree()
    {
        for (int i = 0; i < 8; ++i)
        {
            if (root[i] != null)
            {
                OctTreeBranch parent = new OctTreeBranch();
                parent[~i & 0b111] = root[i];
                root[i] = parent;
            }
        }
        depth++;
        dimension = 1u << depth;
    }

 
    public void Insert(in int3 pos, float value)
    {
        if (!PointIsContained(pos)) { GrowTree(); }

        ulong index = InterleaveVector(NormalizePosition(pos));

        ulong currentIndex = (index >> depth *3);

        if (root[(int)(currentIndex & 0b111)] is null)
        {
            root[(int)(currentIndex & 0b111)] = CreateBranch();
        }

        OctTreeBranch branch = (OctTreeBranch)root[(int)(currentIndex & 0b111)];

        for (int i = depth - 1; i > 1; i--)
        {
            currentIndex = (index >> i * 3);
            if (branch[(int)(currentIndex & 0b111)] is null)
            {
                branch[(int)(currentIndex & 0b111)] = CreateBranch();
            }
            branch = (OctTreeBranch)branch[(int)(currentIndex & 0b111)];
        }

        if(branch[(int)(index >> 3) & 0b111] == null) {
            branch[(int)(index >> 3) & 0b111] = new OctTreeLeaf();
        }

        OctTreeLeaf leaf = (OctTreeLeaf)branch[(int)(index >> 3) & 0b111];
        leaf[(int)(index >> 0) & 0b111] = value;
    }

    public void Remove(in Vector3Int point)
    {
        throw new NotImplementedException();
    }

    private ulong Interleave(uint input)
    {
        const uint numInputs = 3;
        ulong[] masks = {
            0x9249249249249249,
            0x30C30C30C30C30C3,
            0xF00F00F00F00F00F,
            0x00FF0000FF0000FF,
            0xFFFF00000000FFFF
        };

        ulong n = (ulong)input;
        for (int i = 4; i != 1; i--)
        {
            int shift = (int)((numInputs - 1) * (1 << i));
            n |= n << shift;
            n &= masks[i];

        }

        return n;
    }

    private ulong InterleaveVector(uint3 position)
    {
        return (Interleave(position.x) << 2) | (Interleave(position.y) << 1) | (Interleave(position.z));
    }

}