using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPRigidbody : MonoBehaviour
{
    void Start()
    {
        CollisionManager.Instance.AddRigidbody(this);
    }

    public Vector3 velocity;
    public float mass = 1;
    public Vector3 force;
}
