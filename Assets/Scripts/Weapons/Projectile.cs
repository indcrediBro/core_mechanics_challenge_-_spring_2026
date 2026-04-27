using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private WeaponContextData data;
    private Rigidbody rb;
    private int remainingPierce;
    private int remainingRicochet;
    private HashSet<Collider> piercedColliders = new HashSet<Collider>();

    public void Initialize(WeaponContextData _data, Vector3 direction)
    {
        data             = _data;
        rb               = GetComponent<Rigidbody>();
        remainingPierce  = data.canPierce    ? data.pierceCount    : 0;
        remainingRicochet = data.canRicochet ? data.ricochetCount  : 0;

        rb.useGravity    = false;
        rb.linearVelocity = direction * data.projectileSpeed;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity += Vector3.down * (data.gravityScale * 9.81f * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (piercedColliders.Contains(collision.collider)) return;

        bool hitEnemy = collision.collider.TryGetComponent(out IDamageable damageable);

        if (hitEnemy)
        {
            damageable.TakeDamage(data.damage);

            if (data.canPierce && remainingPierce > 0)
            {
                piercedColliders.Add(collision.collider);
                Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
                remainingPierce--;
                return; // Keep going through enemy
            }
        }
        else if (data.canRicochet && remainingRicochet > 0)
        {
            Vector3 reflected = Vector3.Reflect(rb.linearVelocity.normalized, collision.contacts[0].normal);
            rb.linearVelocity = reflected * data.projectileSpeed;
            remainingRicochet--;
            return; // Bounce off wall
        }

        if (data.isExplosive) Explode();
        else                  Destroy(gameObject);
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, data.explosionRadius);

        foreach (Collider hit in hits)
            hit.GetComponent<IDamageable>()?.TakeDamage(data.explosionDamage);

        // GameEvents.RequestExplosionKnockback(transform.position, data.explosionRadius, data.recoilForce);
        Destroy(gameObject);
    }
}
}