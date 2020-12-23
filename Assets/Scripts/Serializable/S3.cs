using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class S3
{
    float a, b, c;
    public S3(Color _c)
    {
        a = _c.r;
        b = _c.g;
        c = _c.b;
    }

    public S3(Vector3 _c)
    {
        a = _c.x;
        b = _c.y;
        c = _c.z;
    }

    public Color Color
    {
        get
        {
            return new Color(a,b,c);
        }
    }

    public Vector3 Vector3
    {
        get
        {
            return new Vector3(a, b, c);
        }
    }
}

[Serializable]
public class S2
{
    float a, b;
    public S2(Vector2 _c)
    {
        a = _c.x;
        b = _c.y;
    }

    public Vector2 Vector2
    {
        get
        {
            return new Vector2(a, b);
        }
    }
}
