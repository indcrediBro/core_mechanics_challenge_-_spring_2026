using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Bounce", menuName = "Player/Bounce Ability")]
    public class PlayerBounceAbility : PlayerAbilityBase
    {
        [Title("Physics")]  private string name = "Bounce Physics";
        [SerializeField] float bounceForce = 5f;
        [Title("Ground Check")]
        [SerializeField] private float groundcheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        public override void OnUpdate(PlayerContext _ctx = null)
        {
            Bounce(_ctx);
        }

        private void Bounce(PlayerContext _ctx)
        {
            if (IsGrounded3D(_ctx))
            {
                _ctx.rigidbody.linearVelocity = new Vector3(_ctx.rigidbody.linearVelocity.x, bounceForce, _ctx.rigidbody.linearVelocity.z);
            }
        }

        private bool IsGrounded3D(PlayerContext _ctx)
        {
            return Groundchecker.GetGroundHits(_ctx.groundCheck, groundcheckRadius, groundLayer).Length>0;
        }
    }
}