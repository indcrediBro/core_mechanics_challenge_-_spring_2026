using UnityEngine;
using GameData;
using TheDeveloperTrain.SciFiGuns;
using Weapons;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Gun", menuName = "Player/Gun Ability")]
    public class PlayerGunAbility : PlayerAbilityBase
    {
        public override void OnAwake(PlayerContextData _ctx = null)
        {
            base.OnAwake(_ctx);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;

            GameManager.OnEvery10Bounces += () => RandomWeaponSwitch(_ctx);
        }

        public override void OnStart(PlayerContextData _ctx = null)
        {
            base.OnStart(_ctx);
            _ctx.activeGun = _ctx.allGuns[0];
            _ctx.activeGun.gameObject.SetActive(true);
        }

        public override void OnUpdate(PlayerContextData _ctx = null)
        {
            if (_ctx.activeGun == null) return;

            if (Input.GetMouseButtonDown(0))
                _ctx.activeGun.Shoot();
        }

        // ─────────────────────────────────────────────────────────────
        // Weapon switching
        // ─────────────────────────────────────────────────────────────

        private void RandomWeaponSwitch(PlayerContextData _ctx)
        {
            if (_ctx == null || _ctx.allGuns == null || _ctx.allGuns.Length <= 1) return;

            Gun previousGun = _ctx.activeGun;
            previousGun?.gameObject.SetActive(false);

            // Pick a different gun at random
            int attempts = 0;
            int r;
            do
            {
                r = Random.Range(0, _ctx.allGuns.Length);
                attempts++;
            }
            while (_ctx.allGuns[r] == previousGun && attempts < 10);

            _ctx.activeGun = _ctx.allGuns[r];
            _ctx.activeGun.gameObject.SetActive(true);

            GameManager.SwitchWeapon(_ctx.activeGun.name);
        }

        private void OnDisable()
        {
            // Note: ScriptableObject OnDisable fires at domain-reload / app quit.
            // If you store the lambda, keep a reference to unsubscribe properly.
            // For simplicity the event is cleared by GameManager on game-start.
        }
    }
}