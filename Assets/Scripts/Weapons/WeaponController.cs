using UnityEngine;

namespace Weapons
{
    public abstract class WeaponController : MonoBehaviour
    {
        [SerializeField] protected Transform muzzle;

        public abstract void Shoot(Vector3 direction);
    }
}