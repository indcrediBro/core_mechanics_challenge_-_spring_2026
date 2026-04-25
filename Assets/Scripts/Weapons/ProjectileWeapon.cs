using UnityEngine;

namespace Weapons
{
    public class ProjectileWeapon : WeaponController
    {
        [SerializeField] private Rigidbody projectilePrefab;
        [SerializeField] private float fireForce = 20f;

        public override void Shoot(Vector3 direction)
        {
            Rigidbody projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(direction));
            projectile.AddForce(direction * fireForce, ForceMode.Impulse);
        }
    }
}