using System.Collections.Generic;
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

        // GameEvents.OnWeaponSwitched     += HandleWeaponSwitch;
        // GameEvents.OnRecoilRequested    += HandleRecoil;
        // GameEvents.OnExplosionKnockback += HandleExplosionKnockback;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStart(PlayerContextData _ctx = null)
    {
        base.OnStart(_ctx);
        _ctx.activeGun =  _ctx.allGuns[0];
        _ctx.activeGun.gameObject.SetActive(true);
    }

    public override void OnUpdate(PlayerContextData _ctx = null)
    {
        if (_ctx.activeGun == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            _ctx.activeGun.Shoot();
        }
    }

    private void RandomWeaponSwitch(PlayerContextData _ctx)
    {
        Gun lastGun = _ctx.activeGun;
        int r = Random.Range(0, _ctx.allGuns.Length);
        if (_ctx.allGuns[r] == lastGun) r += 1;
        if (r == _ctx.allGuns.Length) r -= 2;
        _ctx.activeGun =  _ctx.allGuns[r];
        _ctx.activeGun.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // GameEvents.OnWeaponSwitched     -= HandleWeaponSwitch;
        // GameEvents.OnRecoilRequested    -= HandleRecoil;
        // GameEvents.OnExplosionKnockback -= HandleExplosionKnockback;
    }
    }
}