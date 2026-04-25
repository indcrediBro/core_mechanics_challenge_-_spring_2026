using System.Collections;
using IncredibleAttributes;
using UnityEngine;

namespace PlayerAbility
{
    [CreateAssetMenu(fileName = "PlayerAbility Camera", menuName = "Player/Camera Ability")]
    public class PlayerCameraAbility : PlayerAbilityBase
    {
        [Title("Camera")] private string name = "Camera";
        [SerializeField] private float m_distance = 5f;
        [SerializeField] private float m_heightOffset = 1.5f;
        [SerializeField] private float m_minPitch = -80f;
        [SerializeField] private float m_maxPitch = 80f;
        [SerializeField] private float m_rotationSpeed = 5f;

        private float yaw = 0f;
        private float pitch = 20f;
        private bool  isRotating = false;

        public override void OnAwake(PlayerContext _ctx = null)
        {
            yaw = 0f;
            pitch = 20f;
            isRotating = false;
        }

        public override void OnUpdate(PlayerContext _ctx = null)
        {
            float sensitivity = GameManager.Instance.GameData.Sensitivity;

            yaw   += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch  = Mathf.Clamp(pitch, m_minPitch, m_maxPitch);

            OrbitCamera(_ctx);

            if (Input.GetMouseButtonDown(1) && !isRotating)
                _ctx.player.StartCoroutine(RotateCamera180(_ctx));
        }

        public override void OnFixedUpdate(PlayerContext _ctx = null)
        {
            Quaternion aimRotation = Quaternion.Euler(pitch, yaw, 0f);
            _ctx.rigidbody.MoveRotation(aimRotation);
        }

        private void OrbitCamera(PlayerContext _ctx)
        {
            Vector3 lookTarget = _ctx.player.transform.position + Vector3.up * m_heightOffset;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            _ctx.yaw = yaw;
            _ctx.camera.position = lookTarget + orbitRotation * new Vector3(0f, 0f, -m_distance);
            _ctx.camera.LookAt(lookTarget);
        }

        private IEnumerator RotateCamera180(PlayerContext _ctx)
        {
            isRotating = true;
            float targetYaw = yaw + 180f;

            while (Mathf.Abs(Mathf.DeltaAngle(yaw, targetYaw)) > 0.1f)
            {
                yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, m_rotationSpeed * Time.deltaTime * 180f);
                OrbitCamera(_ctx);
                yield return null;
            }

            yaw = targetYaw;
            _ctx.yaw = yaw;
            isRotating = false;
        }
    }
}