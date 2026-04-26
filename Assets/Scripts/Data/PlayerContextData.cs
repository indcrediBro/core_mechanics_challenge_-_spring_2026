using System;
using UnityEngine;

namespace GameData
{
    [Serializable, ]
    public class PlayerContextData
    {
        public Player player;
        public Rigidbody rigidbody;
        public Transform camera;
        public Transform groundCheck;

        public float yaw;
    }
}