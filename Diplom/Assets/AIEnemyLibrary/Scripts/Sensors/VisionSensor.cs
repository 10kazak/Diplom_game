using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    public class VisionSensor : MonoBehaviour
    {
        [Header("VisionSensor - обнаружение цели в поле зрения")]

        [Tooltip("Угол обзора врага в градусах.")]
        [Range(0f, 360f)]
        [SerializeField] private float fieldOfView = 120f;

        [Tooltip("Максимальная дистанция обзора.")]
        [SerializeField] private float viewDistance = 12f;

        [Tooltip("Дистанция ближнего удержания цели. Внутри неё цель можно считать замеченной даже при выходе из малого угла обзора.")]
        [SerializeField] private float closeAwarenessDistance = 3f;

        [Tooltip("Слои объектов, которые считаются целями.")]
        [SerializeField] private LayerMask targetMask;

        [Tooltip("Слои препятствий, перекрывающих обзор.")]
        [SerializeField] private LayerMask obstacleMask;

        [Tooltip("Точка зрения врага.")]
        [SerializeField] private Transform eyePoint;

        [Tooltip("Как часто выполнять сканирование.")]
        [SerializeField] private float scanInterval = 0.2f;

        private float nextScanTime;

        public Transform VisibleTarget { get; private set; }

        private void Awake()
        {
            if (eyePoint == null)
                eyePoint = transform;
        }

        public Transform GetVisibleTarget()
        {
            if (Time.time >= nextScanTime)
            {
                Scan();
                nextScanTime = Time.time + scanInterval;
            }

            return VisibleTarget;
        }

        private void Scan()
        {
            VisibleTarget = null;

            Collider[] hits = Physics.OverlapSphere(eyePoint.position, viewDistance, targetMask);
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                Transform target = hit.transform;

                Vector3 targetPoint = target.position;
                Collider targetCollider = hit.GetComponent<Collider>();
                if (targetCollider != null)
                    targetPoint = targetCollider.bounds.center;

                Vector3 directionToTarget = (targetPoint - eyePoint.position).normalized;
                float distance = Vector3.Distance(eyePoint.position, targetPoint);

                // Если цель очень близко, разрешаем удержание даже при малом FOV
                bool isInsideCloseAwareness = distance <= closeAwarenessDistance;

                if (!isInsideCloseAwareness)
                {
                    float angle = Vector3.Angle(eyePoint.forward, directionToTarget);
                    if (angle > fieldOfView * 0.5f)
                        continue;
                }

                if (Physics.Raycast(eyePoint.position, directionToTarget, distance, obstacleMask))
                    continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    VisibleTarget = target;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = eyePoint != null ? eyePoint : transform;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin.position, viewDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin.position, closeAwarenessDistance);

            Vector3 leftBoundary = Quaternion.Euler(0f, -fieldOfView * 0.5f, 0f) * origin.forward;
            Vector3 rightBoundary = Quaternion.Euler(0f, fieldOfView * 0.5f, 0f) * origin.forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin.position, leftBoundary * viewDistance);
            Gizmos.DrawRay(origin.position, rightBoundary * viewDistance);
            Gizmos.DrawRay(origin.position, origin.forward * viewDistance);
        }
    }
}