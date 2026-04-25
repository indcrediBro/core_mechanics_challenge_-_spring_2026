using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Collision", menuName = "Player/Collision Ability")]
    public class PlayerCollisionAbility: PlayerAbilityBase
    {
        [Title("Collisions")]  private string name = "Collisions";

        public override void OnCollisionColliderEnter(Collision other, PlayerContext _ctx = null)
        {
            if (other.collider.CompareTag("Dead"))
            {
                GameManager.Instance.GameOver();
            }
        }

        public override void OnTriggerColliderEnter(Collider other, PlayerContext _ctx = null)
        {
            if (other.CompareTag("Dead"))
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}