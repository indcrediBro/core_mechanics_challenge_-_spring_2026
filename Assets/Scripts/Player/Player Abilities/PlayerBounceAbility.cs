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

        public override void OnFixedUpdate(PlayerContextData _ctx = null)
        {
            Bounce(_ctx);
        }

        private bool wasGrounded = false;

        private void Bounce(PlayerContextData _ctx)
        {
            Collider[] hits = IsGrounded3D(_ctx);
            bool isGrounded = hits.Length > 0;

            if (isGrounded && !wasGrounded)
            {
                _ctx.rigidbody.linearVelocity = new Vector3(
                    _ctx.rigidbody.linearVelocity.x,
                    bounceForce,
                    _ctx.rigidbody.linearVelocity.z);

                _ctx.groundCheck.position = hits[0].ClosestPoint(_ctx.groundCheck.position);
                _ctx.player.onBounce.Invoke();
                hits[0].GetComponent<Platform>()?.HandleBounce();
            }

            wasGrounded = isGrounded;
        }

        private Collider[] IsGrounded3D(PlayerContextData _ctx)
        {
            return Groundchecker.GetGroundHits(_ctx.groundCheck, groundcheckRadius, groundLayer);
        }
    }
}