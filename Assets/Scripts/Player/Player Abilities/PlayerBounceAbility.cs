using System.Collections;
using GameData;
using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Bounce", menuName = "Player/Bounce Ability")]
    public class PlayerBounceAbility : PlayerAbilityBase
    {
        [Title("Physics")]
        [SerializeField] float bounceForce = 5f;
        [Title("Ground Check")]
        [SerializeField] private float groundcheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        private float cooldown;
        private bool canBreak;

        public override void OnStart(PlayerContextData _ctx = null)
        {
            base.OnStart(_ctx);
            cooldown = 0.5f;
            canBreak = true;
        }

        public override void OnUpdate(PlayerContextData _ctx = null)
        {
            Bounce(_ctx);
        }

        private void Bounce(PlayerContextData _ctx)
        {
            if (!canBreak) return;
            canBreak = false;
            Collider[] hits = IsGrounded3D(_ctx);
            if (hits.Length > 0)
            {
                _ctx.rigidbody.linearVelocity = new Vector3(_ctx.rigidbody.linearVelocity.x, bounceForce, _ctx.rigidbody.linearVelocity.z);
                _ctx.groundCheck.position = hits[0].ClosestPoint(_ctx.groundCheck.position);
                _ctx.player.onBounce.Invoke();
               hits[0].GetComponent<Platform>()?.HandleBounce();
            }

            EnableBreakness(_ctx);
        }

        private IEnumerator EnableBreaknessCO()
        {
            yield return new WaitForSeconds(cooldown);
            canBreak = true;
        }

        private void EnableBreakness(PlayerContextData _ctx)
        {
            _ctx.player.StartCoroutine(EnableBreaknessCO());
        }

        private Collider[] IsGrounded3D(PlayerContextData _ctx)
        {
            // return Groundchecker.GetGroundHitsRaycast(_ctx.player.transform, groundcheckRadius, groundLayer);
            return Groundchecker.GetGroundHits(_ctx.groundCheck, groundcheckRadius, groundLayer);
        }
    }
}