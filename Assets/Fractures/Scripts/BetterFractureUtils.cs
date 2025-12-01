using System.Collections.Generic;
using UnityEngine;



public struct SimpleCrackEdge
{
    public Vector3 sp;
    public Vector3 ep;
    public Vector3 sn;
    public Vector3 en;
    public float sd;
    public float ed;

    public SimpleCrackEdge(Vector3 p0, Vector3 p1, Vector3 n0, Vector3 n1)
    {
        sp = p0; ep = p1; sn = n0; en = n1;
        sd = ed = 0f;
    }
    public void SwapIfNeeded(Vector3 center)
    {
        sd = (sp - center).magnitude;
        ed = (ep - center).magnitude;
        if (sd > ed)
        {
            (sp, ep, sn, en, sd, ed) = (ep, sp, en, sn, ed, sd);
        }
    }
    public int CompareTo(SimpleCrackEdge other, Vector3 CenterPoint)
    {
        if (!sd.EpsEqual(other.sd)) { return sd.CompareTo(other.sd); }
        else if (!sp.EpsEqual(other.sp)) { return sp.V3CompareTo(other.sp); }
        else { return ep.V3CompareTo(other.ep); }
    }
    public bool ConnectTo(SimpleCrackEdge other)
    {
        return ep.EpsEqual(other.sp) &&
               en.EpsEqual(other.sn) &&
               (Vector3.Dot((ep - sp).normalized, (other.ep - other.sp).normalized) >= 0.999f);
    }
}

public static class BUFutils
{
    public const float DEFAULT_EPSILON = 0.0001f;
    public static bool EpsEqual(this Vector3 a, Vector3 b, float eps = DEFAULT_EPSILON * DEFAULT_EPSILON)
    {
        return (a - b).sqrMagnitude <= eps;
    }
    public static bool EpsEqual(this float a, float b, float eps = DEFAULT_EPSILON)
    {
        return Mathf.Abs(a - b) <= eps;
    }
    public static bool EpsEqual(this SimpleCrackEdge a, SimpleCrackEdge b, float eps = DEFAULT_EPSILON)
    {
        return a.ep.EpsEqual(b.ep) && a.sp.EpsEqual(b.sp) && a.en.EpsEqual(b.en) && a.sn.EpsEqual(b.sn);
    }

    public static int V3CompareTo(this Vector3 a, Vector3 b)
    {
        if (!a.x.EpsEqual(b.x)) { return a.x.CompareTo(b.x); }
        if (!a.y.EpsEqual(b.y)) { return a.y.CompareTo(b.y); }
        return a.z.CompareTo(b.z);
    }
    public static BlastConfiguration CreateSimpleBlastConf(Vector3 point, Vector3 norm)
    {
        BlastConfiguration conf;
        
        conf.blastPoint = point;
        conf.blastNormal = norm;
        conf.innerSites = 20;
        conf.innerRadius = 0.1f;
        conf.innerBias = 0.2f;
        conf.transitionSites = 20;
        conf.transitionRadius = 0.2f;
        conf.transitionBias = 0.3f;
        conf.outersites = 10;
        conf.radialRadius = 0.25f;
        conf.radialRadSteps = 3;
        conf.radialAngSteps = 6;
        conf.radialAngleOffset = 0.1f;
        conf.radialNormalOffset = 0.0f;
        conf.radialVariability = 0.01f;

        return conf;
    }

}

