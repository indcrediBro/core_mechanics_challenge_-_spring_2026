using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Movement", menuName = "Player/Movement Ability")]
    public class PlayerMovementAbility : PlayerAbilityBase
    {
        [Title("Movement")]  private string name = "Movement";
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 moveInput;

        public override void OnUpdate(PlayerContext _ctx = null)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        public override void OnFixedUpdate(PlayerContext _ctx = null)
        {
            HandleMovement(_ctx);
        }

        private void HandleMovement(PlayerContext _ctx = null)
        {
            Vector3 move = _ctx.player.transform.right * moveInput.x + _ctx.camera.forward * moveInput.y;
            Vector3 moveVelocity = move * moveSpeed;

            _ctx.rigidbody.linearVelocity =
                new Vector3(moveVelocity.x, _ctx.rigidbody.linearVelocity.y, moveVelocity.z);
        }
    }
}
