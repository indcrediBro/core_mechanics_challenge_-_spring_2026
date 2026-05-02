using System;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

namespace Weapons
{
    // ─────────────────────────────────────────────────────────────
    //  Recoil profile: one asset per gun type (or shared presets)
    // ─────────────────────────────────────────────────────────────
    [CreateAssetMenu(fileName = "RecoilProfile", menuName = "Weapons/Recoil Profile")]
    public class RecoilProfile : ScriptableObject
    {
        [Header("Kick — applied instantly on each shot")]
        public Vector3 kickRotation     = new Vector3(-4f, 1f, 0.5f);   // pitch, yaw, roll
        public Vector3 kickPosition     = new Vector3(0f, 0f, -0.08f);  // local-space push
        public float   kickRotationRng  = 0.3f;   // 0–1 randomness multiplier on yaw/roll
        public float   kickPositionRng  = 0.1f;

        [Header("Spring — controls how the gun settles back")]
        [Range(1f, 60f)]  public float rotationStiffness  = 18f;
        [Range(1f, 60f)]  public float rotationDamping    = 6f;
        [Range(1f, 60f)]  public float positionStiffness  = 22f;
        [Range(1f, 60f)]  public float positionDamping    = 7f;

        [Header("Accumulation — continuous-fire sway buildup")]
        [Range(0f, 1f)] public float accumulationFactor   = 0.25f;
        [Range(0f, 1f)] public float accumulationDecay    = 0.8f;
        public float maxAccumulation  = 2.5f;

        [Header("Camera Recoil (separate from gun model)")]
        public Vector2 cameraKick         = new Vector2(-1.5f, 0.4f);  // pitch, yaw
        public float   cameraKickRng      = 0.25f;
        [Range(1f, 60f)] public float cameraStiffness = 14f;
        [Range(1f, 60f)] public float cameraDamping   = 5f;
    }

    // ─────────────────────────────────────────────────────────────
    //  Simple spring for a single float
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public class SpringFloat
    {
        public float value;
        public float velocity;

        public void AddForce(float force) => velocity += force;

        public void Update(float stiffness, float damping, float dt)
        {
            float springForce  = -stiffness * value;
            float dampingForce = -damping   * velocity;
            velocity += (springForce + dampingForce) * dt;
            value    += velocity * dt;
        }

        public void Reset() { value = 0f; velocity = 0f; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Convenience wrapper for Vector3 springs
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public class SpringVector3
    {
        public SpringFloat x = new SpringFloat();
        public SpringFloat y = new SpringFloat();
        public SpringFloat z = new SpringFloat();

        public Vector3 Value => new Vector3(x.value, y.value, z.value);

        public void AddForce(Vector3 force)
        {
            x.AddForce(force.x);
            y.AddForce(force.y);
            z.AddForce(force.z);
        }

        public void Update(float stiffness, float damping, float dt)
        {
            x.Update(stiffness, damping, dt);
            y.Update(stiffness, damping, dt);
            z.Update(stiffness, damping, dt);
        }

        public void Reset() { x.Reset(); y.Reset(); z.Reset(); }
    }

    // ─────────────────────────────────────────────────────────────
    //  Built-in preset library — no ScriptableObject required
    // ─────────────────────────────────────────────────────────────
    public enum GunRecoilPreset
    {
        Pistol, Revolver, SMG, AssaultRifle, BattleRifle,
        Shotgun, SniperRifle, LMG, Akimbo, RocketLauncher
    }

    public static class RecoilPresets
    {
        public static RecoilProfile CreateRuntime(GunRecoilPreset preset)
        {
            var p = ScriptableObject.CreateInstance<RecoilProfile>();
            switch (preset)
            {
                case GunRecoilPreset.Pistol:
                    p.kickRotation = new Vector3(-5f, 1.2f, 0.6f);
                    p.kickPosition = new Vector3(0f, 0f, -0.06f);
                    p.kickRotationRng = 0.3f; p.kickPositionRng = 0.05f;
                    p.rotationStiffness = 20f; p.rotationDamping = 7f;
                    p.positionStiffness = 25f; p.positionDamping = 8f;
                    p.accumulationFactor = 0.15f; p.maxAccumulation = 1.5f; p.accumulationDecay = 0.9f;
                    p.cameraKick = new Vector2(-1.8f, 0.5f); p.cameraKickRng = 0.25f;
                    p.cameraStiffness = 16f; p.cameraDamping = 5f;
                    break;

                case GunRecoilPreset.Revolver:
                    p.kickRotation = new Vector3(-10f, 1.5f, 1f);
                    p.kickPosition = new Vector3(0f, 0f, -0.12f);
                    p.kickRotationRng = 0.2f; p.kickPositionRng = 0.04f;
                    p.rotationStiffness = 14f; p.rotationDamping = 5f;
                    p.positionStiffness = 18f; p.positionDamping = 6f;
                    p.accumulationFactor = 0.05f; p.maxAccumulation = 0.8f; p.accumulationDecay = 1.2f;
                    p.cameraKick = new Vector2(-3.5f, 0.6f); p.cameraKickRng = 0.2f;
                    p.cameraStiffness = 12f; p.cameraDamping = 4f;
                    break;

                case GunRecoilPreset.SMG:
                    p.kickRotation = new Vector3(-3f, 0.8f, 0.3f);
                    p.kickPosition = new Vector3(0f, 0f, -0.04f);
                    p.kickRotationRng = 0.5f; p.kickPositionRng = 0.08f;
                    p.rotationStiffness = 25f; p.rotationDamping = 9f;
                    p.positionStiffness = 30f; p.positionDamping = 10f;
                    p.accumulationFactor = 0.35f; p.maxAccumulation = 3.5f; p.accumulationDecay = 0.7f;
                    p.cameraKick = new Vector2(-1f, 0.35f); p.cameraKickRng = 0.4f;
                    p.cameraStiffness = 20f; p.cameraDamping = 7f;
                    break;

                case GunRecoilPreset.AssaultRifle:
                    p.kickRotation = new Vector3(-5.5f, 1f, 0.4f);
                    p.kickPosition = new Vector3(0f, 0f, -0.07f);
                    p.kickRotationRng = 0.35f; p.kickPositionRng = 0.05f;
                    p.rotationStiffness = 18f; p.rotationDamping = 6f;
                    p.positionStiffness = 22f; p.positionDamping = 7f;
                    p.accumulationFactor = 0.28f; p.maxAccumulation = 3f; p.accumulationDecay = 0.8f;
                    p.cameraKick = new Vector2(-2f, 0.5f); p.cameraKickRng = 0.3f;
                    p.cameraStiffness = 15f; p.cameraDamping = 5f;
                    break;

                case GunRecoilPreset.BattleRifle:
                    p.kickRotation = new Vector3(-8f, 1.8f, 0.7f);
                    p.kickPosition = new Vector3(0f, 0f, -0.1f);
                    p.kickRotationRng = 0.25f; p.kickPositionRng = 0.04f;
                    p.rotationStiffness = 15f; p.rotationDamping = 5f;
                    p.positionStiffness = 19f; p.positionDamping = 6f;
                    p.accumulationFactor = 0.22f; p.maxAccumulation = 2.5f; p.accumulationDecay = 0.85f;
                    p.cameraKick = new Vector2(-3f, 0.7f); p.cameraKickRng = 0.2f;
                    p.cameraStiffness = 13f; p.cameraDamping = 4.5f;
                    break;

                case GunRecoilPreset.Shotgun:
                    p.kickRotation = new Vector3(-14f, 2f, 1.5f);
                    p.kickPosition = new Vector3(0f, 0.02f, -0.18f);
                    p.kickRotationRng = 0.15f; p.kickPositionRng = 0.06f;
                    p.rotationStiffness = 12f; p.rotationDamping = 4.5f;
                    p.positionStiffness = 16f; p.positionDamping = 5.5f;
                    p.accumulationFactor = 0.05f; p.maxAccumulation = 0.6f; p.accumulationDecay = 1.5f;
                    p.cameraKick = new Vector2(-5f, 0.8f); p.cameraKickRng = 0.15f;
                    p.cameraStiffness = 11f; p.cameraDamping = 4f;
                    break;

                case GunRecoilPreset.SniperRifle:
                    p.kickRotation = new Vector3(-12f, 0.5f, 0.2f);
                    p.kickPosition = new Vector3(0f, 0f, -0.22f);
                    p.kickRotationRng = 0.1f; p.kickPositionRng = 0.02f;
                    p.rotationStiffness = 10f; p.rotationDamping = 4f;
                    p.positionStiffness = 14f; p.positionDamping = 5f;
                    p.accumulationFactor = 0.02f; p.maxAccumulation = 0.4f; p.accumulationDecay = 2f;
                    p.cameraKick = new Vector2(-6f, 0.3f); p.cameraKickRng = 0.1f;
                    p.cameraStiffness = 9f; p.cameraDamping = 3.5f;
                    break;

                case GunRecoilPreset.LMG:
                    p.kickRotation = new Vector3(-4f, 1.2f, 0.5f);
                    p.kickPosition = new Vector3(0f, 0f, -0.05f);
                    p.kickRotationRng = 0.6f; p.kickPositionRng = 0.1f;
                    p.rotationStiffness = 16f; p.rotationDamping = 6f;
                    p.positionStiffness = 20f; p.positionDamping = 7f;
                    p.accumulationFactor = 0.4f; p.maxAccumulation = 5f; p.accumulationDecay = 0.5f;
                    p.cameraKick = new Vector2(-1.5f, 0.6f); p.cameraKickRng = 0.5f;
                    p.cameraStiffness = 13f; p.cameraDamping = 5f;
                    break;

                case GunRecoilPreset.Akimbo:
                    p.kickRotation = new Vector3(-3.5f, 2.5f, 1.2f);
                    p.kickPosition = new Vector3(0.04f, 0f, -0.05f);
                    p.kickRotationRng = 0.6f; p.kickPositionRng = 0.12f;
                    p.rotationStiffness = 26f; p.rotationDamping = 9f;
                    p.positionStiffness = 28f; p.positionDamping = 9f;
                    p.accumulationFactor = 0.2f; p.maxAccumulation = 2f; p.accumulationDecay = 0.9f;
                    p.cameraKick = new Vector2(-1.2f, 0.7f); p.cameraKickRng = 0.5f;
                    p.cameraStiffness = 18f; p.cameraDamping = 6f;
                    break;

                case GunRecoilPreset.RocketLauncher:
                    p.kickRotation = new Vector3(-18f, 3f, 2f);
                    p.kickPosition = new Vector3(0f, 0.04f, -0.3f);
                    p.kickRotationRng = 0.08f; p.kickPositionRng = 0.02f;
                    p.rotationStiffness = 8f;  p.rotationDamping = 3.5f;
                    p.positionStiffness = 11f; p.positionDamping = 4f;
                    p.accumulationFactor = 0f; p.maxAccumulation = 0f; p.accumulationDecay = 2f;
                    p.cameraKick = new Vector2(-8f, 1f); p.cameraKickRng = 0.08f;
                    p.cameraStiffness = 7f; p.cameraDamping = 3f;
                    break;
            }
            return p;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Main component — attach to the gun model's pivot Transform
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Procedural spring-based recoil animator.
    ///
    /// SETUP:
    ///   1. Attach this to your weapon view-model pivot GameObject.
    ///   2. Assign cameraTransform (the Camera child of your player).
    ///   3. Either assign a RecoilProfile asset, or choose a GunRecoilPreset
    ///      — the component will build a runtime profile automatically.
    ///   4. Call RegisterWeapon() with the active WeaponController, or let
    ///      autoRegister handle it on Awake.
    ///   5. On weapon switch: call RegisterWeapon(newWeapon) and optionally
    ///      SetPreset() or SetProfile() to swap the recoil feel.
    /// </summary>
    public class ProceduralRecoilAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Camera that receives rotational camera-recoil.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Profile — assign asset OR choose a preset")]
        [SerializeField] private RecoilProfile profile;
        [SerializeField] private GunRecoilPreset preset = GunRecoilPreset.AssaultRifle;
        [Tooltip("When true the preset is used even if a profile asset is assigned.")]
        [SerializeField] private bool usePreset = false;
        [Tooltip("Auto-find and register the WeaponController on this GameObject.")]
        [SerializeField] private bool autoRegister = true;

        [Header("Global Scale")]
        [Range(0f, 2f)] [SerializeField] private float recoilScale = 1f;

        // ── Runtime state ─────────────────────────────────────
        private RecoilProfile    activeProfile;
        private WeaponController registeredWeapon;

        private SpringVector3 rotationSpring = new SpringVector3();
        private SpringVector3 positionSpring = new SpringVector3();
        private SpringVector3 cameraSpring   = new SpringVector3(); // x=pitch, y=yaw

        private float     accumulation = 0f;
        private Vector3   originLocalPos;
        private Quaternion originLocalRot;

        // ── Public API ────────────────────────────────────────

        /// <summary>Hook into a WeaponController's onShoot event.</summary>
        public void RegisterWeapon(WeaponController weapon)
        {
            if (registeredWeapon != null)
                registeredWeapon.onShoot.RemoveListener(OnShoot);

            registeredWeapon = weapon;
            if (weapon != null)
                weapon.onShoot.AddListener(OnShoot);
        }

        /// <summary>Swap the recoil profile at runtime (e.g. on weapon switch).</summary>
        public void SetProfile(RecoilProfile newProfile)
        {
            activeProfile = newProfile;
            ResetSprings();
        }

        /// <summary>Swap to a built-in preset at runtime.</summary>
        public void SetPreset(GunRecoilPreset newPreset)
        {
            preset = newPreset;
            activeProfile = RecoilPresets.CreateRuntime(preset);
            ResetSprings();
        }

        // ── Unity Lifecycle ───────────────────────────────────

        private void Awake()
        {
            originLocalPos = transform.localPosition;
            originLocalRot = transform.localRotation;

            activeProfile = (usePreset || profile == null)
                ? RecoilPresets.CreateRuntime(preset)
                : profile;

            if (autoRegister)
                RegisterWeapon(GetComponentInChildren<WeaponController>());
        }

        private void OnDestroy()
        {
            if (registeredWeapon != null)
                registeredWeapon.onShoot.RemoveListener(OnShoot);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            var   p  = activeProfile;

            // Decay continuous-fire accumulation when not shooting
            accumulation = Mathf.MoveTowards(accumulation, 0f, p.accumulationDecay * dt);

            // Step springs
            rotationSpring.Update(p.rotationStiffness, p.rotationDamping, dt);
            positionSpring.Update(p.positionStiffness, p.positionDamping, dt);
            cameraSpring  .Update(p.cameraStiffness,   p.cameraDamping,   dt);

            // Apply to gun model (local-space)
            transform.localRotation = originLocalRot *
                Quaternion.Euler(rotationSpring.Value * recoilScale);
            transform.localPosition = originLocalPos +
                positionSpring.Value * recoilScale;

            // Apply camera pitch / yaw
            if (cameraTransform != null)
            {
                Vector3 cam = cameraSpring.Value * recoilScale;
                cameraTransform.Rotate(cam.x, cam.y, 0f, Space.Self);
            }
        }

        // ── Internal ──────────────────────────────────────────

        private void OnShoot()
        {
            var p = activeProfile;

            accumulation = Mathf.Min(accumulation + 1f, p.maxAccumulation);
            float accMul = 1f + accumulation * p.accumulationFactor;

            // Gun rotation kick
            Vector3 rotKick = p.kickRotation * accMul;
            rotKick.y += RandomBipolar() * p.kickRotationRng * Mathf.Abs(p.kickRotation.y);
            rotKick.z += RandomBipolar() * p.kickRotationRng * Mathf.Abs(p.kickRotation.z);
            rotationSpring.AddForce(rotKick);

            // Gun position kick
            Vector3 posKick = p.kickPosition * accMul;
            posKick.x += RandomBipolar() * p.kickPositionRng * Mathf.Abs(p.kickPosition.z);
            positionSpring.AddForce(posKick);

            // Camera kick
            Vector3 camKick = new Vector3(
                p.cameraKick.x * accMul,
                p.cameraKick.y * accMul + RandomBipolar() * p.cameraKickRng,
                0f);
            cameraSpring.AddForce(camKick);
        }

        private void ResetSprings()
        {
            rotationSpring.Reset();
            positionSpring.Reset();
            cameraSpring.Reset();
            accumulation = 0f;
        }

        private static float RandomBipolar() => UnityEngine.Random.Range(-1f, 1f);
    }
}
