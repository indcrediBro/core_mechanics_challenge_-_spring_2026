using System.Collections.Generic;
using UnityEngine;

public static class Groundchecker
{
    public static Collider[] GetGroundHits(Transform transform, float radius,LayerMask layerToCheck)
    {
        return Physics.OverlapSphere(transform.position, radius, layerToCheck);
    }

    public static Collider[] GetGroundHitsRaycast(Transform transform, float distance, LayerMask layerToCheck)
    {
        List<Collider> colliders = new List<Collider>();
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, distance, layerToCheck);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                colliders.Add(hits[i].collider);
            }
        }
        return colliders.ToArray();
    }
}