using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    private static CollisionManager instance;
    public static CollisionManager Instance
    {
        get {
            if (instance == null)
            {
                instance = new GameObject("CollisionManager").AddComponent<CollisionManager>();
            }
            return instance;
        }
    }

    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    
    List<CPCollider> bodies = new List<CPCollider>();
    List<ISolver> solvers = new List<ISolver>();

    List<CPRigidbody> rigidbodies = new List<CPRigidbody>();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
    }

    void FixedUpdate()
    {
        for (int i = 0; i < rigidbodies.Count; ++i)
        {
            CPRigidbody cPRigidbody = rigidbodies[i];
            Transform transform = cPRigidbody.transform;

            cPRigidbody.force += cPRigidbody.mass * gravity;
            cPRigidbody.velocity += cPRigidbody.force / cPRigidbody.mass * Time.fixedDeltaTime;
            transform.position += cPRigidbody.velocity * Time.fixedDeltaTime;

            cPRigidbody.force = Vector3.zero;
        }

        for (int i = 0; i < bodies.Count; ++i)
        {
            CPCollider firstCollider = bodies[i];
            if (!firstCollider.enabled) continue;
            for (int j = i + 1; j < bodies.Count; ++j)
            {
                CPCollider secondCollider = bodies[j];
                if (!secondCollider.enabled) continue;

                if (!firstCollider.CheckCollision(secondCollider)) continue;

                Debug.Log("Collision detected! " + firstCollider.gameObject.name + " " + secondCollider.gameObject.name);
            }
        }
    }

    public CPRay.CPRayHit Raycast(CPRay ray)
    {
        CPRay.CPRayHit hit = null;

        for (int i = 0; i < bodies.Count; ++i)
        {
            CPCollider firstCollider = bodies[i];

            CPRay.CPRayHit currHit = firstCollider.CheckCollision(ray);
            if(currHit == null) continue;

            if (hit == null || hit.Distance > currHit.Distance) 
                hit = currHit;
        }

        return hit ?? new CPRay.CPRayHit(false);
    }

    public CPRay.CPRayHit Raycast(Vector3 origin, Vector3 direction)
    {
        return Raycast(new CPRay(origin, direction));
    }

    public CPRay.CPRayHit Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        return Raycast(new CPRay(origin, direction, maxDistance));
    }

    public void AddBody(CPCollider collider)
    {
        bodies.Add(collider);
    }

    public void AddRigidbody(CPRigidbody body)
    {
        rigidbodies.Add(body);
    }
}
