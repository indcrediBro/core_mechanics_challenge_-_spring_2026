using System;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField] private LayerMask layersToDestroy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == layersToDestroy)
        {
            Destroy(other.gameObject);
        }
    }
}
