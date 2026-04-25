using System.Collections;
using GameData;
using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Camera", menuName = "Player/Camera Ability")]
    public class PlayerCameraAbility : PlayerAbilityBase
    {
        [Title("Camera")]
        [SerializeField] private float distance = 5f;
        [SerializeField] private float heightOffset = 1.5f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float rotationSpeed = 5f;

        private float yaw = 0f;
        private float pitch = 20f;
        private bool  isRotating = false;

        public override void OnAwake(PlayerContextData _ctx = null)
        {
            yaw = 0f;
            pitch = 20f;
            isRotating = false;
        }

        public override void OnUpdate(PlayerContextData _ctx = null)
        {
            float sensitivity = GameManager.Instance.GameData.Sensitivity;

            yaw   += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

            OrbitCamera(_ctx);

            if (Input.GetMouseButtonDown(1) && !isRotating)
                _ctx.player.StartCoroutine(RotateCamera180(_ctx));
        }

        public override void OnFixedUpdate(PlayerContextData _ctx = null)
        {
            Quaternion aimRotation = Quaternion.Euler(pitch, yaw, 0f);
            _ctx.rigidbody.MoveRotation(aimRotation);
        }

        private void OrbitCamera(PlayerContextData _ctx)
        {
            Vector3 lookTarget = _ctx.player.transform.position + Vector3.up * heightOffset;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            _ctx.yaw = yaw;
            _ctx.camera.position = lookTarget + orbitRotation * new Vector3(0f, 0f, -distance);
            _ctx.camera.LookAt(lookTarget);
        }

        private IEnumerator RotateCamera180(PlayerContextData _ctx)
        {
            isRotating = true;
            float targetYaw = yaw + 180f;

            while (Mathf.Abs(Mathf.DeltaAngle(yaw, targetYaw)) > 0.1f)
            {
                yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, rotationSpeed * Time.deltaTime * 180f);
                OrbitCamera(_ctx);
                yield return null;
            }

            yaw = targetYaw;
            _ctx.yaw = yaw;
            isRotating = false;
        }
    }
}