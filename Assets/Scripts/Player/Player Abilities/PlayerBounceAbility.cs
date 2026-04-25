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

        public override void OnUpdate(PlayerContextData _ctx = null)
        {
            Bounce(_ctx);
        }

        private void Bounce(PlayerContextData _ctx)
        {
            if (IsGrounded3D(_ctx))
            {
                _ctx.rigidbody.linearVelocity = new Vector3(_ctx.rigidbody.linearVelocity.x, bounceForce, _ctx.rigidbody.linearVelocity.z);
            }
        }

        private bool IsGrounded3D(PlayerContextData _ctx)
        {
            return Groundchecker.GetGroundHits(_ctx.groundCheck, groundcheckRadius, groundLayer).Length>0;
        }
    }
}