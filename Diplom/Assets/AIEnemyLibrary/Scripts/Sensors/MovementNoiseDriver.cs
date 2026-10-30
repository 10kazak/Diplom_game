using UnityEngine;
using UnityEngine.AI;

namespace AIEnemyLibrary.Sensors
{
    public class MovementNoiseDriver : MonoBehaviour
    {
        public enum SpeedSourceMode
        {
            NavMeshAgent,
            Rigidbody,
            TransformDelta
        }

        [Header("MovementNoiseDriver - шумы движения")]

        [SerializeField] private SpeedSourceMode speedSourceMode = SpeedSourceMode.TransformDelta;
        [SerializeField] private ActionNoiseEmitter actionNoiseEmitter;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody rb;

        [Header("Ground Check (для игрока)")]
        [Tooltip("Если true — шумы движения отключаются в воздухе")]
        [SerializeField] private bool useGroundCheck = true;

        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckDistance = 0.25f;
        [SerializeField] private LayerMask groundMask;

        [Header("Скорости")]
        [SerializeField] private float walkThreshold = 0.2f;
        [SerializeField] private float runThreshold = 4f;

        [Header("Интервалы шума")]
        [SerializeField] private float walkNoiseInterval = 0.55f;
        [SerializeField] private float runNoiseInterval = 0.35f;

        [SerializeField] private bool debugLogs = false;

        private float noiseTimer = 0f;
        private Vector3 lastPosition;

        private bool wasMovingLastFrame = false;
        private bool wasRunningLastFrame = false;

        private void Awake()
        {
            if (actionNoiseEmitter == null)
                actionNoiseEmitter = GetComponent<ActionNoiseEmitter>();

            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            if (rb == null)
                rb = GetComponent<Rigidbody>();

            lastPosition = transform.position;
        }

        private void Update()
        {
            if (actionNoiseEmitter == null)
                return;

            // 🔥 ПРОВЕРКА: если в воздухе — отключаем всё
            if (useGroundCheck && !IsGrounded())
            {
                StopAllMovementAudio();
                return;
            }

            float speed = GetCurrentSpeed();

            bool isMoving = speed >= walkThreshold;
            bool isRunning = speed >= runThreshold;

            if (!isMoving)
            {
                noiseTimer = 0f;

                if (wasMovingLastFrame)
                    actionNoiseEmitter.StopLoopedMovementAudio();

                wasMovingLastFrame = false;
                wasRunningLastFrame = false;
                lastPosition = transform.position;
                return;
            }

            if (!wasMovingLastFrame || wasRunningLastFrame != isRunning)
                actionNoiseEmitter.StartLoopedMovementAudio(isRunning);

            noiseTimer += Time.deltaTime;

            float interval = isRunning ? runNoiseInterval : walkNoiseInterval;

            if (noiseTimer >= interval)
            {
                noiseTimer = 0f;
                actionNoiseEmitter.EmitMovementNoiseEvent(isRunning);

                if (debugLogs)
                    Debug.Log($"{name} movement noise emitted");
            }

            wasMovingLastFrame = true;
            wasRunningLastFrame = isRunning;
            lastPosition = transform.position;
        }

        private void StopAllMovementAudio()
        {
            if (wasMovingLastFrame)
                actionNoiseEmitter.StopLoopedMovementAudio();

            wasMovingLastFrame = false;
            wasRunningLastFrame = false;
            noiseTimer = 0f;
        }

        private bool IsGrounded()
        {
            if (groundCheckPoint == null)
                return true;

            return Physics.Raycast(
                groundCheckPoint.position,
                Vector3.down,
                groundCheckDistance,
                groundMask
            );
        }

        private float GetCurrentSpeed()
        {
            switch (speedSourceMode)
            {
                case SpeedSourceMode.NavMeshAgent:
                    return navMeshAgent != null ? navMeshAgent.velocity.magnitude : 0f;

                case SpeedSourceMode.Rigidbody:
                    return rb != null ? rb.linearVelocity.magnitude : 0f;

                case SpeedSourceMode.TransformDelta:
                    float distance = Vector3.Distance(transform.position, lastPosition);
                    return distance / Mathf.Max(Time.deltaTime, 0.0001f);
            }

            return 0f;
        }
    }
}