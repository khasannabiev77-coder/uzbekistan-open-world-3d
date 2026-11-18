using UnityEngine;
using UzOpenWorld.Core;
using UzOpenWorld.PlayerSystem;

namespace UzOpenWorld.CameraSystem
{
    /// <summary>
    /// Orbit tipidagi third-person kamera: yumshoq kuzatish, devorga kirib ketmaslik (SphereCast),
    /// sprintda FOV kengayishi. Phase 2'da mashina rejimi uchun alohida profil qo'shiladi.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        public Vector3 pivotOffset = new Vector3(0f, 1.55f, 0f);

        [Header("Orbit")]
        public float distance = 4.2f;
        public float minPitch = -35f;
        public float maxPitch = 70f;
        [Tooltip("Kuzatish tezligi (katta = qattiqroq)")]
        public float followSharpness = 18f;

        [Header("Collision")]
        public float collisionRadius = 0.25f;
        public float minDistance = 0.8f;
        [Tooltip("Default: hamma narsa, 'Ignore Raycast' layeridan tashqari (o'yinchi shu layerda)")]
        public LayerMask collisionMask = ~(1 << 2);

        [Header("FOV")]
        public float normalFov = 60f;
        public float sprintFov = 68f;
        public float fovSharpness = 4f;

        float _yaw, _pitch = 12f, _distance;
        Vector3 _pivot;
        Camera _cam;
        PlayerController _player;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _distance = distance;
        }

        public void SetTarget(Transform newTarget, PlayerController player = null)
        {
            target = newTarget;
            _player = player;
            if (target == null) return;
            _yaw = target.eulerAngles.y;
            _pivot = target.position + pivotOffset;
            _distance = distance;
            ApplyTransform(Quaternion.Euler(_pitch, _yaw, 0f));
        }

        void LateUpdate()
        {
            if (target == null) return;
            float dt = Time.deltaTime;

            var input = InputManager.Instance;
            if (input != null)
            {
                Vector2 look = input.LookDelta;
                _yaw += look.x;
                _pitch = Mathf.Clamp(_pitch - look.y, minPitch, maxPitch);
            }

            Vector3 desiredPivot = target.position + pivotOffset;
            _pivot = Vector3.Lerp(_pivot, desiredPivot, 1f - Mathf.Exp(-followSharpness * dt));

            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 back = rot * Vector3.back;

            float wanted = distance;
            if (Physics.SphereCast(_pivot, collisionRadius, back, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
                wanted = Mathf.Max(minDistance, hit.distance);

            // Devorga yaqinlashganda darhol, uzoqlashganda sekin
            _distance = wanted < _distance ? wanted : Mathf.Lerp(_distance, wanted, 1f - Mathf.Exp(-4f * dt));

            ApplyTransform(rot);

            if (_cam != null)
            {
                float fov = (_player != null && _player.State == MoveState.Sprint) ? sprintFov : normalFov;
                _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, fov, 1f - Mathf.Exp(-fovSharpness * dt));
            }
        }

        void ApplyTransform(Quaternion rot)
        {
            transform.SetPositionAndRotation(_pivot + rot * Vector3.back * _distance, rot);
        }
    }
}
