using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem
{
    public class PlatformHealth : Health
    {
        // Fires with normalized health (1 = full, 0 = dead) after each hit
        [SerializeField] public UnityEvent<float> onHealthChanged;

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            // Only invoke if still alive (base may have destroyed the GO)
            if (gameObject != null)
                onHealthChanged?.Invoke(NormalizedHealth);
        }
    }
}