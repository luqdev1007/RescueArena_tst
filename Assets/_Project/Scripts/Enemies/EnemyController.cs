using UnityEngine;

namespace WebGLRescueArena
{
    [RequireComponent(typeof(EnemyAttack))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private LayerMask obstructionMask;
        [SerializeField] private Rigidbody _rigidbody;

        private Transform target;
        private EnemyAttack attack;

        private void Awake() => attack = GetComponent<EnemyAttack>();

        private void OnEnable()
        {
            target = null;

            if (_rigidbody == null) 
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero; 
        }


        private void Update()
        {
            if (target == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject == null) 
                    return;

                target = playerObject.transform;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;

            if (Physics.Raycast(transform.position + Vector3.up * 0.4f, direction.normalized, distance, obstructionMask)) 
                return;

            if (distance > 1.1f) 
                transform.position += direction.normalized * (moveSpeed * Time.deltaTime);

            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            attack.Tick(target);
        }
    }
}