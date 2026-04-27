using System;
using UnityEngine;
using Weapons;

namespace GameData
{
    [Serializable, ]
    public class PlayerContextData
    {
        public Player player;
        public Rigidbody rigidbody;
        public Transform camera;
        public Transform groundCheck;
        public WeaponController selectedGun;

        public float yaw;
    }
}