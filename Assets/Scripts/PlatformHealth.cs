using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem
{
    public class PlatformHealth : Health
    {
        [SerializeField] private UnityEvent onDamageTaken;

        public override void TakeDamage(int damage)
        {
            onDamageTaken?.Invoke();
            base.TakeDamage(damage);
        }
    }
}