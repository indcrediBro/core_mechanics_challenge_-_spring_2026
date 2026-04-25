using System;
using IncredibleAttributes;
using UnityEngine;

public enum FireMode  { Single, Auto, Continuous }
public enum ShootType { Projectile, Hitscan }

[Serializable]
public class WeaponContextData
{
    [Title("Identity")]
    public string gunName;
    public FireMode fireMode;
    public ShootType shootType;

    [Title("Fire")]
    public float fireRate = 5f;
    public int pelletsPerShot = 1;

    [Title("Abilities")]
    public bool canPierce;
    [ShowIf("canPierce")]
    public int pierceCount;

    public bool canRicochet;
    [ShowIf("canRicochet")]
    public int ricochetCount;

    [Title("Damage")]
    public int damage = 25;
    [ShowIf("isHitscan")]
    public float range  = 100f;

    [Title("Accuracy")]
    [Range(0f, 45f)]
    public float spread = 0f;

    [Title("Projectile"), ShowIf("isProjectile")]
    public Rigidbody projectilePrefab;
    [ShowIf("isProjectile")]
    public float projectileSpeed = 20f;
    [ShowIf("isProjectile")]
    public float gravityScale = 1f;
    [ShowIf("isProjectile")]
    public bool isExplosive;
    [ShowIf("isProjectile")]
    public float explosionRadius;
    [ShowIf("isProjectile")]
    public int explosionDamage;

    [Title("Feel")]
    public float recoilForce = 3f;
    public float screenShakeStrength = 0.1f;
    public float screenShakeDuration = 0.1f;

    public bool isHitscan => shootType == ShootType.Hitscan;
    public bool isProjectile => shootType == ShootType.Projectile;
}