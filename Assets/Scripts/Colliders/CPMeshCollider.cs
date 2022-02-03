using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CPMeshCollider : CPCollider
{
    MeshFilter meshFilter;

    private void Start()
    {

        CollisionManager.Instance.AddBody(this);
        meshFilter = GetComponent<MeshFilter>();
    }

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
        Mesh mesh = meshFilter.mesh;
        
        if (mesh == null)
            return null;

        float tNear = ray.MaxDistance;
        int count = mesh.triangles.Length;
        bool intersected = false;
        CPRay ray2 = new CPRay(ray.Origin - transform.position, ray.Direction, ray.MaxDistance);
        for(int i = 0; i < count; i += 3)
        {
            Vector3 pos0 = mesh.vertices[mesh.triangles[i]];
            Vector3 pos1 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 pos2 = mesh.vertices[mesh.triangles[i + 2]];
            
            if(RayTriangleIntersection(ray2, pos0, pos1, pos2, out float t) && t < tNear)
            {
                tNear = t;
                intersected = true;
            }
        }

        if(!intersected) return null;

        return new CPRay.CPRayHit(true, this, ray.Origin + (ray.Direction * tNear), tNear);
    }
}
