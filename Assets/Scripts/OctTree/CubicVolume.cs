using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicVolume : MonoBehaviour
{
    int size;
    Vector3 position;

    CubicVolume(Vector3 position, int size)
    {
        this.position = position;
        this.size = size;
    }

    public Vector3[] GetCorners()
    {
        Vector3[] window = new Vector3[8];
        window[0] = position;
        window[1] = position + new Vector3(size, 0, 0);
        window[2] = position + new Vector3(size, size, 0);
        window[3] = position + new Vector3(0, size, 0);
        window[4] = position + new Vector3(0, 0, size);
        window[5] = position + new Vector3(size, 0, size);
        window[6] = position + new Vector3(size, size, size);
        window[7] = position + new Vector3(0, size, size);
        return window;
    }

    public void GetCorners(ref Vector3[] window)
    {
        if (window.Length != 8) window = new Vector3[8]; // (Vector3[]
        window[0] = position;
        window[1] = position + new Vector3(size, 0, 0);
        window[2] = position + new Vector3(size, size, 0);
        window[3] = position + new Vector3(0, size, 0);
        window[4] = position + new Vector3(0, 0, size);
        window[5] = position + new Vector3(size, 0, size);
        window[6] = position + new Vector3(size, size, size);
        window[7] = position + new Vector3(0, size, size);
    }

    public bool Contains(in Vector3 point)
    {
        return point.x >= position.x && point.x < position.x + size &&
            point.y >= position.y && point.y < position.y + size &&
            point.z >= position.z && point.z < position.z + size;
    }
}
