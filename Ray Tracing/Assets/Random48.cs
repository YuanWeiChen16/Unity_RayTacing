using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Random48
{
    static long seed = 1;
    public static float Get()
    {
        seed = (0x5DEECE66DL * seed + 0xB16) & 0xFFFFFFFFFFFFL;
        return (seed >> 16) / (float)0x100000000L;
    }
    public static Vector3 random_cosine_direction()
    {
        float r1 = Random48.Get();
        float r2 = Random48.Get();
        float z = Mathf.Sqrt(1 - r2);
        float phi = 2 * 3.141592f * r1;
        float x = Mathf.Cos(phi) * Mathf.Sqrt(r2);
        float y = Mathf.Sin(phi) * Mathf.Sqrt(r2);
        return new Vector3(x, y, z);
    }
}