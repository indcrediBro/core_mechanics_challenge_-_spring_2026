using System.Collections;
using UnityEngine;

namespace Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponContextData data;
        [SerializeField] private Transform[] muzzles; // Multiple = Akimbo

        private float nextFireTime = 0f;
        private bool isFiring = false;

        public WeaponContextData Data => data;

        public void TryShoot(Vector3 direction)
        {
            if (data.fireMode == FireMode.Continuous)
            {
                if (!isFiring) StartCoroutine(FireContinuous(direction));
            }
            else
            {
                if (Time.time >= nextFireTime) Fire(direction);
            }
        }

        public void StopShooting() => isFiring = false;

        private void Fire(Vector3 direction)
        {
            nextFireTime = Time.time + 1f / data.fireRate;

            foreach (Transform muzzle in muzzles)
            {
                for (int i = 0; i < data.pelletsPerShot; i++)
                {
                    Vector3 spreadDir = ApplySpread(direction);

                    if (data.isProjectile) SpawnProjectile(muzzle, spreadDir);
                    else FireHitscan(muzzle, spreadDir);
                }
            }
            Debug.Log("Fired");
            ApplyFeel(direction);
        }

        private IEnumerator FireContinuous(Vector3 direction)
        {
            isFiring = true;
            while (isFiring)
            {
                Fire(direction);
                yield return new WaitForSeconds(1f / data.fireRate);
            }
        }

        private void SpawnProjectile(Transform muzzle, Vector3 direction)
        {
            Rigidbody rb = Instantiate(data.projectilePrefab, muzzle.position, Quaternion.LookRotation(direction));
            rb.useGravity = false;

            if (rb.TryGetComponent(out Projectile projectile))
                projectile.Initialize(data, direction);
        }

        private void FireHitscan(Transform muzzle, Vector3 direction)
        {
            int pierceLeft = data.canPierce ? data.pierceCount : 0;
            RaycastHit[] hits = Physics.RaycastAll(muzzle.position, direction, data.range);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            Debug.Log(hits.Length > 0 ? $"Fired {data.gunName} at {hits[0].collider.name}" : $"Fired {data.gunName} — no hits");            int hitCount = 0;

            foreach (RaycastHit hit in hits)
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(data.damage);
                hitCount++;
                if (hitCount > pierceLeft) break;
            }
        }

        private Vector3 ApplySpread(Vector3 direction)
        {
            if (data.spread <= 0f) return direction;
            return Quaternion.Euler(
                Random.Range(-data.spread, data.spread),
                Random.Range(-data.spread, data.spread),
                0f) * direction;
        }

        private void ApplyFeel(Vector3 direction)
        {
            // GameEvents.RequestRecoil(-direction * data.recoilForce);
            // GameEvents.RequestScreenShake(data.screenShakeStrength, data.screenShakeDuration);
        }
    }
}