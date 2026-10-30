using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    /// <summary>
    /// Хранит информацию о цели, последней видимой позиции и последнем услышанном шуме.
    /// </summary>
    public class TargetMemory : MonoBehaviour
    {
        [Header("TargetMemory - память о цели")]

        [Tooltip("Сколько секунд враг помнит цель после потери видимости.")]
        [SerializeField] private float memoryDuration = 4f;

        [Tooltip("Сколько секунд враг помнит шум.")]
        [SerializeField] private float heardMemoryDuration = 4f;

        private Transform currentTarget;
        private Vector3 lastKnownPosition;
        private float lastSeenTime = -999f;

        private bool hasHeardPosition = false;
        private Vector3 lastHeardPosition;
        private float lastHeardTime = -999f;

        public Transform CurrentTarget => currentTarget;
        public Vector3 LastKnownPosition => lastKnownPosition;
        public bool HasTarget => currentTarget != null;
        public bool HasFreshMemory => Time.time - lastSeenTime <= memoryDuration;

        public bool HasHeardMemory => hasHeardPosition && (Time.time - lastHeardTime <= heardMemoryDuration);
        public Vector3 LastHeardPosition => lastHeardPosition;

        public float LastSeenTime => lastSeenTime;
        public float LastHeardTime => lastHeardTime;

        public void RememberTarget(Transform target)
        {
            currentTarget = target;
            lastKnownPosition = target.position;
            lastSeenTime = Time.time;
            ClearHeardMemory();
        }

        public void UpdateLastKnownPosition(Vector3 position)
        {
            lastKnownPosition = position;
            lastSeenTime = Time.time;
        }

        public void RememberHeardPosition(Vector3 position)
        {
            if (HasTarget)
                return;

            hasHeardPosition = true;
            lastHeardPosition = position;
            lastHeardTime = Time.time;
        }

        /// <summary>
        /// Потеря текущей цели, но последняя известная позиция остаётся в памяти.
        /// </summary>
        public void ForgetTarget()
        {
            currentTarget = null;
        }

        public void ClearVisualMemory()
        {
            currentTarget = null;
            lastSeenTime = -999f;
        }

        public void ClearHeardMemory()
        {
            hasHeardPosition = false;
        }

        public void ClearAllMemory()
        {
            currentTarget = null;
            lastSeenTime = -999f;
            hasHeardPosition = false;
        }

        public float TimeSinceLastSeen()
        {
            return Time.time - lastSeenTime;
        }
    }
}