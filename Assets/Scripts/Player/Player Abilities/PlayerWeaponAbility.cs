using UnityEngine;
using GameData;
using Weapons;

namespace PlayerAbility
{

    [CreateAssetMenu(fileName = "PlayerAbility Weapon", menuName = "Player/Weapon Ability")]
    public class PlayerWeaponAbility : PlayerAbilityBase
    {
        private WeaponController[] guns;
        private WeaponController activeGun;

        public override void OnAwake(PlayerContextData ctx = null)
        {
            guns = ctx.player.GetComponentsInChildren<WeaponController>(includeInactive: true);

            // Disable all guns, wait for event to assign active one
            foreach (WeaponController gun in guns)
                gun.gameObject.SetActive(false);

            GameManager.OnWeaponSwitched += HandleWeaponSwitch;
        }

        public override void OnUpdate(PlayerContextData ctx = null)
        {
            if (activeGun == null) return;

            if (Input.GetMouseButtonDown(0))
                activeGun.Shoot(ctx.player.transform.forward);
        }

        private void HandleWeaponSwitch(string gunName)
        {
            foreach (WeaponController gun in guns)
            {
                bool isTarget = gun.gameObject.name == gunName;
                gun.gameObject.SetActive(isTarget);
                if (isTarget) activeGun = gun;
            }
        }

        // Called when ability is destroyed or game ends
        private void OnDisable()
        {
            GameManager.OnWeaponSwitched -= HandleWeaponSwitch;
        }
    }
}