using GameData;
using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem
{
    public class EnemyHealth : Health
    {
        [SerializeField] private UnityEvent onDamageTaken;

        public override void TakeDamage(int damage)
        {
            onDamageTaken?.Invoke();
            base.TakeDamage(damage);
        }

        protected override void TriggerDeath()
        {
            // Award kill score before destroying
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.RegisterKill();
            }

            base.TriggerDeath();
        }
    }
}