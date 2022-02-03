using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPPlaneCollider : CPCollider
{
    public Vector2 size = new Vector2(1, 1);

    public override bool CheckCollision(CPCollider other)
    {
        return other.CheckCollision(this);
    }

    public override bool CheckCollision(CPAABBCollider other)
    {
        return false;
    }

    public override bool CheckCollision(CPSphereCollider other)
    {
        return false;
    }

    public override bool CheckCollision(CPOBBCollider other)
    {
        return false;
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
        Vector3 normal = -transform.up;

        float denom = Vector3.Dot(normal, ray.Direction);
        if(denom > 1e-6)
        {
            float t = Vector3.Dot(GetPosition() - ray.Origin, normal) / denom;
            if (t < 0) 
                return null;

            Vector3 hitPos = ray.Origin + (ray.Direction * t);

            Vector3 s = GetSize() / 2;
            Vector3 min = GetPosition() - (transform.forward * s.x) - (transform.right * s.z);
            Vector3 max = GetPosition() + (transform.forward * s.x) + (transform.right * s.z);

            if (hitPos.x < min.x || hitPos.x > max.x) return null;
            if (hitPos.y < min.y || hitPos.y > max.y) return null;
            if (hitPos.z < min.z || hitPos.z > max.z) return null;

            return new CPRay.CPRayHit(true, this, hitPos, t);
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.TRS(GetPosition(), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, GetSize());
    }

    private Vector3 GetSize()
    {
        return new Vector3(size.x * transform.localScale.x, 0.00001f, size.y * transform.localScale.z);
    }
}
