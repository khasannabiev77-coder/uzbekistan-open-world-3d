using UnityEngine;

namespace UzOpenWorld.PlayerSystem
{
    /// <summary>
    /// PLACEHOLDER animatsiya: placeholder tanani tezlikka qarab tebratadi va oldinga egadi.
    /// Haqiqiy model + Animator Controller (idle/walk/run/sprint/jump) kelganda o'chiriladi.
    /// </summary>
    public class PlaceholderCharacterAnimator : MonoBehaviour
    {
        public PlayerController player;
        public float bobAmount = 0.06f;
        public float leanAngle = 9f;

        Vector3 _baseLocalPos;
        float _phase;

        void Start() => _baseLocalPos = transform.localPosition;

        void LateUpdate()
        {
            if (player == null) return;

            float speed01 = Mathf.Clamp01(player.CurrentSpeed / Mathf.Max(0.01f, player.sprintSpeed));
            _phase += Time.deltaTime * (2f + player.CurrentSpeed * 1.6f);

            float bob = player.IsGrounded ? Mathf.Abs(Mathf.Sin(_phase)) * bobAmount * Mathf.Clamp01(player.CurrentSpeed / 2f) : 0f;
            transform.localPosition = _baseLocalPos + Vector3.up * bob;

            float lean = speed01 * leanAngle;
            if (player.State == MoveState.Jump) lean = -6f;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(lean, 0f, 0f), 1f - Mathf.Exp(-10f * Time.deltaTime));
        }
    }
}
