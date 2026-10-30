using UnityEngine;
using UnityEngine.AI;
using AIEnemyLibrary.Core;

namespace AIEnemyLibrary.Behaviours
{
    /// <summary>
    /// RandomPatrolBehaviour - случайное патрулирование по NavMesh.
    /// ¬раг сам выбирает случайные точки в заданном радиусе и перемещаетс€ к ним.
    /// </summary>
    public class RandomPatrolBehaviour : AIBehaviour
    {
        [Header("RandomPatrolBehaviour - случайное патрулирование по области")]

        [Tooltip("–адиус вокруг врага, в котором выбираетс€ случайна€ точка патрул€.")]
        [SerializeField] private float patrolRadius = 10f;

        [Tooltip("—корость движени€ во врем€ патрулировани€.")]
        [SerializeField] private float patrolSpeed = 2f;

        [Tooltip("–ассто€ние до точки, при котором считаетс€, что враг дошЄл до неЄ.")]
        [SerializeField] private float reachDistance = 0.7f;

        [Tooltip("Ќужно ли ждать на точке перед выбором следующей.")]
        [SerializeField] private bool waitOnPoint = true;

        [Tooltip("ћинимальное врем€ ожидани€ на точке.")]
        [SerializeField] private float minWaitTime = 1f;

        [Tooltip("ћаксимальное врем€ ожидани€ на точке.")]
        [SerializeField] private float maxWaitTime = 2.5f;

        [Tooltip("—колько попыток сделать при поиске случайной точки на NavMesh.")]
        [SerializeField] private int maxPointSearchAttempts = 10;

        [Tooltip("ќтносительно какой точки искать патруль: текуща€ позици€ врага или стартова€ позици€.")]
        [SerializeField] private bool useStartPositionAsCenter = true;

        private Vector3 patrolCenter;
        private Vector3 currentTargetPoint;
        private bool hasTargetPoint = false;

        private bool isWaiting = false;
        private float waitTimer = 0f;
        private float currentWaitDuration = 0f;

        protected override void Awake()
        {
            base.Awake();
            patrolCenter = transform.position;
        }

        public override bool CanRun()
        {
            if (controller.TargetMemory != null)
            {
                if (controller.TargetMemory.HasTarget)
                    return false;

                if (controller.TargetMemory.HasFreshMemory)
                    return false;
            }

            return true;
        }

        public override void OnEnterState()
        {
            if (controller.Agent == null)
                return;

            controller.Agent.speed = patrolSpeed;
            controller.Agent.stoppingDistance = 0f;
            controller.Agent.isStopped = false;

            if (!hasTargetPoint)
            {
                PickNextRandomPoint();
            }
            else
            {
                controller.Agent.SetDestination(currentTargetPoint);
            }
        }

        public override void OnUpdateState()
        {
            if (controller.Agent == null)
                return;

            if (isWaiting)
            {
                waitTimer += Time.deltaTime;

                if (waitTimer >= currentWaitDuration)
                {
                    isWaiting = false;
                    PickNextRandomPoint();
                }

                return;
            }

            if (!hasTargetPoint)
            {
                PickNextRandomPoint();
                return;
            }

            float distanceToPoint = Vector3.Distance(transform.position, currentTargetPoint);

            if (distanceToPoint <= reachDistance)
            {
                if (waitOnPoint)
                {
                    StartWaiting();
                }
                else
                {
                    PickNextRandomPoint();
                }
            }
        }

        public override void OnExitState()
        {
            isWaiting = false;
            waitTimer = 0f;

            if (controller.Agent != null)
            {
                controller.Agent.isStopped = false;
            }
        }

        private void StartWaiting()
        {
            isWaiting = true;
            waitTimer = 0f;
            currentWaitDuration = Random.Range(minWaitTime, maxWaitTime);

            if (controller.Agent != null)
            {
                controller.Agent.isStopped = true;
                controller.Agent.ResetPath();
            }
        }

        private void PickNextRandomPoint()
        {
            Vector3 center = useStartPositionAsCenter ? patrolCenter : transform.position;

            for (int i = 0; i < maxPointSearchAttempts; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
                Vector3 randomPoint = new Vector3(
                    center.x + randomCircle.x,
                    center.y,
                    center.z + randomCircle.y
                );

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
                {
                    currentTargetPoint = hit.position;
                    hasTargetPoint = true;

                    if (controller.Agent != null)
                    {
                        controller.Agent.isStopped = false;
                        controller.Agent.SetDestination(currentTargetPoint);
                    }

                    return;
                }
            }

            hasTargetPoint = false;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying
                ? (useStartPositionAsCenter ? patrolCenter : transform.position)
                : transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, patrolRadius);

            if (hasTargetPoint)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(currentTargetPoint, 0.25f);
                Gizmos.DrawLine(transform.position, currentTargetPoint);
            }
        }
    }
}