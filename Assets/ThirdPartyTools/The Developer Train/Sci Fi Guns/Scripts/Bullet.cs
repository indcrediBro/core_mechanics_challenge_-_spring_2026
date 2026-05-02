using System;
using HealthSystem;
using UnityEngine;
using Weapons;

namespace TheDeveloperTrain.SciFiGuns
{

    public class Bullet : MonoBehaviour
    {
        [HideInInspector]
        public float speed = 1.5f;
        public int damage;

        void Start()
        {
            transform.localScale = new Vector3(0.01f, 0.01f, 0.05f);
            Destroy(gameObject, 5f);
        }

        // Update is called once per frame
        void Update()
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            bool hitEnemy = other.TryGetComponent(out IDamageable damageable);

            if (hitEnemy)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}