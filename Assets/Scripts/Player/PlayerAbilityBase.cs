using UnityEngine;

namespace PlayerAbility
{
    public abstract class PlayerAbilityBase : ScriptableObject
    {
        public virtual void OnAwake(PlayerContext _ctx = null) { }
        public virtual void OnStart(PlayerContext _ctx = null) { }
        public virtual void OnUpdate(PlayerContext _ctx = null) { }
        public virtual void OnFixedUpdate(PlayerContext _ctx = null) { }

        public virtual void OnTriggerColliderEnter(Collider other, PlayerContext _ctx = null) { }
        public virtual void OnCollisionColliderEnter(Collision other, PlayerContext _ctx = null) { }
    }
}