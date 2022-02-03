using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPSphereCollider : CPCollider
{
    public float radius = 1f;

    public override bool CheckCollision(CPCollider other)
    {
        return other.CheckCollision(this);
    }

    public override bool CheckCollision(CPAABBCollider other)
    {
        return CheckCollision(other, this);
    }

    public override bool CheckCollision(CPSphereCollider other)
    {
        Vector3 pos = GetPosition() - other.GetPosition();

        float dot = Vector3.Dot(pos, pos);
        float r = GetRadius() + other.GetRadius();

        return dot < r * r;
    }

    public override bool CheckCollision(CPPlaneCollider other)
    {
        return false;
    }

    public override bool CheckCollision(CPMeshCollider other)
    {
        return false;
    }

    public override CPRay.CPRayHit CheckCollision(CPRay ray)
    {
        Vector3 m = ray.Origin - GetPosition();

        float b = Vector3.Dot(m, ray.Direction);
        float c = Vector3.Dot(m, m) - GetRadius() * GetRadius();

        if (c > 0f && b > 0f) 
            return null;

        float discr = b * b - c;
        if (discr < 0f) 
            return null;

        float tmin = -b - Mathf.Sqrt(discr);
        if(tmin < 0f)
            tmin = 0f;

        if (tmin > ray.MaxDistance)
            return null;

        return new CPRay.CPRayHit(true, this, ray.Origin + (ray.Direction * tmin), tmin);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetPosition(), GetRadius());
        Gizmos.color = Color.white;
    }

    public float GetRadius()
    {
        Vector3 size = transform.localScale;

        float r = Mathf.Max(size.x, size.y, size.z);

        return r * radius;
    }

    public override bool CheckCollision(CPOBBCollider other)
    {
        return false;
    }
}
