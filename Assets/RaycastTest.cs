using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    public float maxDistance = float.MaxValue;

    void Update()
    {
        Vector3 rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
        Vector3 rayDirection = Camera.main.ScreenPointToRay(Input.mousePosition).direction;

        CPRay.CPRayHit hit = CollisionManager.Instance.Raycast(rayOrigin, rayDirection, maxDistance);

        if(hit.Hit)
        {
            Debug.Log("Raycast has hit " + hit.ColliderHit.gameObject.name + " at " + hit.Position);
        }
    }
}
