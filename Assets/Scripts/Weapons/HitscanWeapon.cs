using UnityEngine;

namespace Weapons
{
    public class HitscanWeapon : WeaponController
    {
        [SerializeField] private float range = 100f;
        [SerializeField] private int damage = 25;
        [SerializeField] private LayerMask hitLayer;

        public override void Shoot(Vector3 direction)
        {
            if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, range, hitLayer))
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
            }
        }
    }
}