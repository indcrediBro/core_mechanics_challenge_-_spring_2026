using System;
using UnityEngine;
using Weapons;

public class EnemyProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }

        other.TryGetComponent(out IDamageable damageable);
        if(damageable != null)
            damageable.TakeDamage(5);

        Destroy(gameObject);
    }
}
