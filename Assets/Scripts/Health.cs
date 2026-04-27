using System;
using UnityEngine;
using Weapons;

namespace HealthSystem
{
    public abstract class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] protected int startingHealth = 100;
        [SerializeField] private bool shouldDestroyOnDeath = true;
        private int health;

        protected virtual void OnEnable()
        {
            health = startingHealth;
        }

        public virtual void TakeDamage(int damage)
        {
            Debug.Log("Damage Taken for " + damage + " on gameobject " + gameObject.name);
            health -= damage;

            if (health <= 0) TriggerDeath();
        }

        protected virtual void TriggerDeath()
        {
            if (shouldDestroyOnDeath)
            {
                Destroy(gameObject);
                return;
            }
            gameObject?.SetActive(false);
        }
    }
}