using System.Collections.Generic;
using UnityEngine;
using GameData;
using Weapons;

namespace PlayerAbility
{

    [CreateAssetMenu(fileName = "PlayerAbility Weapon", menuName = "Player/Weapon Ability")]
    public class PlayerWeaponAbility : PlayerAbilityBase
    {
        private List<string> guns;
        private string activeGun;

    public override void OnAwake(PlayerContextData _ctx = null)
    {
        guns = new List<string>();
        activeGun = string.Empty;

        WeaponController[] weapons =_ctx.player.GetComponentsInChildren<WeaponController>(includeInactive: true);
        foreach (var weapon in weapons)
        {
            guns.Add(weapon.Data.gunName);
        }

        foreach (WeaponController gun in weapons)
            gun.gameObject.SetActive(false);

        // GameEvents.OnWeaponSwitched     += HandleWeaponSwitch;
        // GameEvents.OnRecoilRequested    += HandleRecoil;
        // GameEvents.OnExplosionKnockback += HandleExplosionKnockback;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStart(PlayerContextData _ctx = null)
    {
        HandleWeaponSwitch(_ctx,"Pistol");
        // base.OnStart(_ctx);
    }

    public override void OnUpdate(PlayerContextData _ctx = null)
    {
        if (_ctx.selectedGun == null) return;

        if (_ctx.selectedGun.Data.fireMode == FireMode.Continuous)
        {
            if (Input.GetMouseButton(0))
            {
                Debug.Log("Attempted Fire Continuous!");
                _ctx.selectedGun.TryShoot(_ctx.player.transform.forward);
            }
            else
            {
                _ctx.selectedGun.StopShooting();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                _ctx.selectedGun.TryShoot(_ctx.player.transform.forward);
            }
        }
    }

    private void HandleWeaponSwitch(PlayerContextData _ctx, string gunName)
    {
        activeGun = gunName;
        _ctx.selectedGun = GetGun(_ctx, gunName);
    }

    private WeaponController GetGun(PlayerContextData _ctx, string gunName)
    {
        WeaponController[] weapons = null;
        weapons=_ctx.player.GetComponentsInChildren<WeaponController>(includeInactive: true);

        foreach (var gun in weapons)
        {
            if (gun.Data.gunName == gunName)
            {
                return gun;
            }
        }
        return null;
    }

    private void HandleRecoil(PlayerContextData _ctx, Vector3 force)
    {
        _ctx.rigidbody.AddForce(force, ForceMode.Impulse);
    }

    private void HandleExplosionKnockback(PlayerContextData _ctx,Vector3 origin, float radius, float force)
    {
        float distance = Vector3.Distance(_ctx.player.transform.position, origin);
        if (distance > radius) return;

        float falloff  = 1f - (distance / radius); // Full force at center, zero at edge
        Vector3 dir    = (_ctx.player.transform.position - origin).normalized;
        _ctx.rigidbody.AddForce(dir * force * falloff, ForceMode.Impulse);
    }

    private void OnDisable()
    {
        // GameEvents.OnWeaponSwitched     -= HandleWeaponSwitch;
        // GameEvents.OnRecoilRequested    -= HandleRecoil;
        // GameEvents.OnExplosionKnockback -= HandleExplosionKnockback;
    }
    }
}