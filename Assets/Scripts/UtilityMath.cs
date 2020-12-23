using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityMath
{
    public static float DegreeToBetweenNeg1To1FromTwoPoints(Vector2 From, Vector2 To, float AppendDegree)
    {
        Vector2 Diff = To - From;
        float Degree = Mathf.Rad2Deg * Mathf.Atan2(Diff.y, Diff.x);
        //Debug.Log(Diff+" "+To+" "+From+" "+Degree+" " + Mathf.Atan2(Diff.y, Diff.x)+" "+ DegreeToBetweenNeg1To1(Degree));
        return DegreeToBetweenNeg1To1(Degree + AppendDegree);
    }

    public static float DegreeToBetweenNeg1To1(float ZDegree) {
        float ZDegree360 = Clamp0To360(ZDegree);
        if (ZDegree360 <= 180f) return (ZDegree360 / 180f);
        return -(1f - ((ZDegree360-180) / 180f));
    }

    public static float Clamp0To360(float degree)
    {
        float result = degree % 360f;
        if (result < 0)
        {
            result += 360f;
        }
        return result;
    }

}
