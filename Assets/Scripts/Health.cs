using System;
using UnityEngine;
using Weapons;

namespace Weapons
{
    public class Health : MonoBehaviour, IDamageable
    {
        int startingHealth = 100;
        private int health;

        private void OnEnable()
        {
            health = startingHealth;
        }

        public void TakeDamage(int damage)
        {
            Debug.Log("Damage Taken for " + damage + " on gameobject " + gameObject.name);
            health -= damage;

            if (health <= 0) Dead();
        }

        private void Dead()
        {
            Destroy(gameObject);
        }
    }
}