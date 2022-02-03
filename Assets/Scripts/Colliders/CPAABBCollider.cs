using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPAABBCollider : CPCollider
{
    public Vector3 size = new Vector3(1, 1, 1);

    public override bool CheckCollision(CPCollider other)
    {
        return other.CheckCollision(this);
    }

    public override bool CheckCollision(CPAABBCollider otherCollider)
    {
        Vector3 pos = GetPosition();
        Vector3 otherPos = otherCollider.GetPosition();

        Vector3 size = GetSize();
        Vector3 otherSize = otherCollider.GetSize();

        if (Mathf.Abs(pos.x - otherPos.x) > size.x + otherSize.x) return false;
        if (Mathf.Abs(pos.y - otherPos.y) > size.y + otherSize.y) return false;
        if (Mathf.Abs(pos.z - otherPos.z) > size.z + otherSize.z) return false;

        return true;
    }

    public override bool CheckCollision(CPSphereCollider other)
    {
        return CheckCollision(this, other);
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
		float tmin= 0;
        float tmax = ray.MaxDistance;

		Vector3 min = GetMin();
		Vector3 max = GetMax();

		for(int i = 0; i < 3; ++i)
        {
            if(Mathf.Abs(ray.Direction[i]) < Mathf.Epsilon)
            {
                if (ray.Origin[i] < min[i] || ray.Origin[i] > max[i])
                    return null;
            } else
            {
                float ood = 1f / ray.Direction[i];
                float t1 = (min[i] - ray.Origin[i]) * ood;
                float t2 = (max[i] - ray.Origin[i]) * ood;

                if (t1 > t2)
                {
                    float t3 = t1;
                    t1 = t2;
                    t2 = t3;
                }

                tmin = Mathf.Max(tmin, t1);
                tmax = Mathf.Min(tmax, t2);

                if (tmin > tmax)
                    return null;
            }
        }

        return new CPRay.CPRayHit(true, this, ray.Origin + (ray.Direction * tmin), tmin);
	}

    public override bool CheckCollision(CPOBBCollider other)
    {
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(GetPosition(), GetSize() * 2);
        Gizmos.color = Color.white;
    }

	public Vector3 GetMin()
    {
		return GetPosition() - GetSize();
	}
	public Vector3 GetMax()
	{
		return GetPosition() + GetSize();
	}

	public Vector3 GetSize()
    {
        return new Vector3(transform.localScale.x * size.x / 2, transform.localScale.y * size.y / 2, transform.localScale.z * size.z / 2);
    }
}
