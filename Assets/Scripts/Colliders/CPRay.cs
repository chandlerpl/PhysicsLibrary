using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPRay
{
    public class CPRayHit
    {
        public bool Hit { get; private set; }
        public CPCollider ColliderHit { get; private set; }
        public Vector3 Position { get; private set; }
        public float Distance { get; private set; }

        public CPRayHit(bool hit)
        {
            Hit = hit;
        }

        public CPRayHit(bool hit, CPCollider colliderHit, Vector3 position, float distance)
        {
            Hit = hit;
            ColliderHit = colliderHit;
            Position = position;
            Distance = distance;
        }
    }

    public Vector3 Origin { get; private set; }
    public Vector3 Direction { get; private set; }
    public Vector3 InvDirection { get; private set; }
    public int[] Signs { get; private set; }
    public float MaxDistance { get; private set; }

    public CPRay(Vector3 origin, Vector3 direction) : this(origin, direction, float.MaxValue) { }

    public CPRay(Vector3 origin, Vector3 direction, float maxDistance)
    {
        Origin = origin;
        Direction = direction;

        InvDirection = new Vector3(-1 / direction.x, -1 / direction.y, -1 / direction.z);
        Signs = new int[] { InvDirection.x < 0 ? 1 : 0, InvDirection.y < 0 ? 1 : 0, InvDirection.z < 0 ? 1 : 0 };

        MaxDistance = maxDistance;
    }
}
