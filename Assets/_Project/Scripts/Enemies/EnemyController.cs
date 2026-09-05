using UnityEngine;

namespace WebGLRescueArena
{
    [RequireComponent(typeof(EnemyAttack))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private LayerMask obstructionMask;
        private Transform selfTransform;
        private Transform target;
        private EnemyAttack attack;
        private Rigidbody body;
        private void Awake()
        {
            selfTransform = transform;
            attack = GetComponent<EnemyAttack>();
            body = GetComponent<Rigidbody>();
        }
        private void OnEnable()
        {
            target = null;
            if (body != null) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
        }
        private void Update()
        {
            if (target == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject == null) return;
                target = playerObject.transform;
            }
            Vector3 selfPosition = selfTransform.position;
            Vector3 targetPosition = target.position;
            Vector3 direction = targetPosition - selfPosition;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < 0.0001f) return;
            Vector3 forward = direction / distance;
            if (Physics.Raycast(selfPosition + Vector3.up * 0.4f, forward, distance, obstructionMask)) return;
            if (distance > 1.1f) selfTransform.position = selfPosition + forward * (moveSpeed * Time.deltaTime);
            selfTransform.rotation = Quaternion.LookRotation(forward);
            attack.Tick(target, distance);
        }
    }
}