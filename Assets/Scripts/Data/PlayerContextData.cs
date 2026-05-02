using System;
using TheDeveloperTrain.SciFiGuns;
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
        public Gun[] allGuns;
        public Gun activeGun;
        public float yaw;
    }
}