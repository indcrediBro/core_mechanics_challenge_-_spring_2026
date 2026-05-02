using System;
using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem
{
    /// <summary>
    /// Sits on the Player GameObject alongside the Player component.
    /// Fires C# events so PlayerUI (and anything else) can react without coupling.
    /// Calls Player.onDeath when HP hits zero so existing ability/VFX hooks still fire.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class PlayerHealth : Health
    {
        /// <summary>Fires with normalized health [0,1] after every hit. Used by the health bar.</summary>
        public static event Action<float> OnHealthChanged;
        public UnityEvent onDamageTaken;

        /// <summary>Fires once when the player dies. PlayerUI listens to trigger Game Over.</summary>
        public static event Action OnPlayerDied;

        private Player _player;

        protected override void OnEnable()
        {
            base.OnEnable(); // resets health to startingHealth
            _player = GetComponent<Player>();
        }

        public override void TakeDamage(int damage)
        {
            onDamageTaken.Invoke();
            base.TakeDamage(damage);

            // If still alive, broadcast updated health ratio
            if (CurrentHealth > 0)
                OnHealthChanged?.Invoke(NormalizedHealth);

        }

        protected override void TriggerDeath()
        {
            // Broadcast zero health to snap the bar empty before destruction
            OnHealthChanged?.Invoke(0f);

            // Invoke the Player's inspector-wired death event (VFX, audio, etc.)
            _player?.onDeath.Invoke();

            // Notify listeners (PlayerUI → GameManager.GameOver)
            OnPlayerDied?.Invoke();

            base.TriggerDeath();
        }
    }
}