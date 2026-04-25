using GameData;
using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Collision", menuName = "Player/Collision Ability")]
    public class PlayerCollisionAbility: PlayerAbilityBase
    {
        public override void OnCollisionColliderEnter(Collision other, PlayerContextData _ctx = null)
        {
            if (other.collider.CompareTag("Dead"))
            {
                GameManager.Instance.GameOver();
            }
        }

        public override void OnTriggerColliderEnter(Collider other, PlayerContextData _ctx = null)
        {
            if (other.CompareTag("Dead"))
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}