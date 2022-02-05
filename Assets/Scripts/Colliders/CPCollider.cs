using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CPCollider : MonoBehaviour
{
    public Vector3 center = new Vector3(0, 0, 0);

    void Start()
    {
        CollisionManager.Instance.AddBody(this);
    }

    public abstract bool CheckCollision(CPCollider other);

    public abstract bool CheckCollision(CPAABBCollider other);

    public abstract bool CheckCollision(CPSphereCollider other);

    public abstract bool CheckCollision(CPOBBCollider other);

    public abstract bool CheckCollision(CPPlaneCollider other);
    public abstract bool CheckCollision(CPMeshCollider other);

    public abstract CPRay.CPRayHit CheckCollision(CPRay ray);

    public Vector3 GetPosition()
    {
        return gameObject.transform.position + center;
    }

    public static Vector3 ClosestPointOnAABB(Vector3 point, CPAABBCollider aabbCollider)
    {
        Vector3 result = new Vector3(point.x, point.y, point.z);

        Vector3 pos = aabbCollider.GetPosition();
        Vector3 size = aabbCollider.GetSize();

        if(result.x < pos.x - size.x) result.x = pos.x - size.x;
        if(result.x > pos.x + size.x) result.x = pos.x + size.x;

        if (result.y < pos.y - size.y) result.y = pos.y - size.y;
        if (result.y > pos.y + size.y) result.y = pos.y + size.y;

        if (result.z < pos.z - size.z) result.z = pos.z - size.z;
        if (result.z > pos.z + size.z) result.z = pos.z + size.z;

        return result;
    }

    protected static bool CheckCollision(CPAABBCollider aabbCollider, CPSphereCollider sphereCollider)
    {
        Vector3 closestPos = ClosestPointOnAABB(sphereCollider.GetPosition(), aabbCollider);

        float distance = (sphereCollider.GetPosition() - closestPos).sqrMagnitude;

        float r = sphereCollider.GetRadius();
        return distance < r * r;
    }

    public static bool RayTriangleIntersection(CPRay ray, Vector3 pos0, Vector3 pos1, Vector3 pos2, out float t, bool doubleSided = false)
    {
        t = float.MaxValue;
        Vector3 v0v1 = pos1 - pos0;
        Vector3 v0v2 = pos2 - pos0;

        Vector3 pvec = Vector3.Cross(ray.Direction, v0v2);
        float det = Vector3.Dot(v0v1, pvec);

        if(doubleSided)
        {
            if (Mathf.Abs(det) < float.Epsilon) return false;
        } else
        {
            if (det < float.Epsilon) return false;
        }

        float invDet = 1 / det;
        Vector3 tvec = ray.Origin - pos0;
        float u = Vector3.Dot(tvec, pvec) * invDet;
        if(u < 0 || u > 1) return false;

        Vector3 qvec = Vector3.Cross(tvec, v0v1);
        float v = Vector3.Dot(ray.Direction, qvec) * invDet;
        if (v < 0 || u + v > 1) return false;

        t = Vector3.Dot(v0v2, qvec) * invDet;
        return true;
    }
}
