using UnityEngine;

namespace JungleRun
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -5.5f);
        [SerializeField] private float smoothXSpeed = 12.0f;
        [SerializeField] private float pitchAngle = 14.0f;

        [Header("Dynamic FOV")]
        [SerializeField] private Camera cam;
        [SerializeField] private float baseFOV = 60f;
        [SerializeField] private float maxFOV = 74f;

        [Header("Screen Shake")]
        [SerializeField] private float defaultShakeDuration = 0.3f;
        [SerializeField] private float defaultShakeMagnitude = 0.25f;

        private float shakeDuration = 0f;
        private float shakeMagnitude = 0f;
        private Vector3 shakeOffset = Vector3.zero;

        private void Awake()
        {
            if (cam == null)
                cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            FollowTarget();
            UpdateFOV();
            ProcessShake();
        }

        private void FollowTarget()
        {
            // Follow Z and Y directly, smoothly interpolate X along lanes
            float currentX = Mathf.Lerp(transform.position.x, target.position.x * 0.65f + offset.x, smoothXSpeed * Time.deltaTime);
            float currentY = target.position.y + offset.y;
            float currentZ = target.position.z + offset.z;

            Vector3 desiredPosition = new Vector3(currentX, currentY, currentZ) + shakeOffset;
            transform.position = desiredPosition;

            // Lock rotation straight forward with a fixed pitch looking down the track (prevents diagonal tilt)
            transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        }

        private void UpdateFOV()
        {
            if (cam == null) return;
            if (GameManager.Instance == null || GameManager.Instance.Player == null) return;

            float speed = GameManager.Instance.Player.CurrentSpeed;
            float t = Mathf.InverseLerp(12f, 28f, speed);
            cam.fieldOfView = Mathf.Lerp(baseFOV, maxFOV, t);
        }

        private void ProcessShake()
        {
            if (shakeDuration > 0f)
            {
                shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.deltaTime;
            }
            else
            {
                shakeOffset = Vector3.zero;
                shakeDuration = 0f;
            }
        }

        public void TriggerShake(float duration = -1f, float magnitude = -1f)
        {
            shakeDuration = duration > 0f ? duration : defaultShakeDuration;
            shakeMagnitude = magnitude > 0f ? magnitude : defaultShakeMagnitude;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}