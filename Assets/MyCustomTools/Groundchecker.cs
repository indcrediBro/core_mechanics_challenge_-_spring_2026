using UnityEngine;

public static class Groundchecker
{
    public static Collider[] GetGroundHits(Transform transform, float radius,LayerMask layerToCheck)
    {
        return Physics.OverlapSphere(transform.position, radius, layerToCheck);
    }
}