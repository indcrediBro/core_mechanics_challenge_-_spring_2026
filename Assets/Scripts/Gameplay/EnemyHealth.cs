using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem
{
    public class EnemyHealth: Health
    {
        [SerializeField] private UnityEvent onDamageTaken;

        public override void TakeDamage(int damage)
        {
            onDamageTaken?.Invoke();
            base.TakeDamage(damage);
        }
    }
}