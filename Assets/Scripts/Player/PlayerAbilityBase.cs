using GameData;
using UnityEngine;

namespace PlayerAbility
{
    public abstract class PlayerAbilityBase : ScriptableObject
    {
        public virtual void OnAwake(PlayerContextData _ctx = null) { }
        public virtual void OnStart(PlayerContextData _ctx = null) { }
        public virtual void OnUpdate(PlayerContextData _ctx = null) { }
        public virtual void OnFixedUpdate(PlayerContextData _ctx = null) { }

        public virtual void OnTriggerColliderEnter(Collider other, PlayerContextData _ctx = null) { }
        public virtual void OnCollisionColliderEnter(Collision other, PlayerContextData _ctx = null) { }
    }
}