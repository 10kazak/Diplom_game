using UnityEngine;
using AIEnemyLibrary.Core;

namespace AIEnemyLibrary.Behaviours
{
    public class SearchBehaviour : AIBehaviour
    {
        [Header("SearchBehaviour - поиск цели")]

        [SerializeField] private float searchMoveSpeed = 2.5f;
        [SerializeField] private float arriveDistance = 0.7f;
        [SerializeField] private float searchDuration = 3f;

        [Header("Осмотр")]
        [SerializeField] private float lookAroundRotationSpeed = 120f;
        [SerializeField] private float lookAroundAngle = 60f;
        [SerializeField] private bool lookAroundAfterArrive = true;

        [Header("Debug (визуальная точка)")]
        [Tooltip("Префаб точки последней позиции")]
        [SerializeField] private GameObject debugPointPrefab;

        [Tooltip("Удалять точку после выхода")]
        [SerializeField] private bool destroyPointOnExit = true;

        private GameObject spawnedPoint;

        private bool hasArrived = false;
        private float searchTimer = 0f;
        private Quaternion baseRotation;
        private bool rotateRight = true;

        public override bool CanRun()
        {
            if (controller.TargetMemory == null)
                return false;

            if (controller.TargetMemory.HasTarget)
                return false;

            return controller.TargetMemory.HasFreshMemory;
        }

        public override void OnEnterState()
        {
            hasArrived = false;
            searchTimer = 0f;
            rotateRight = true;

            Vector3 targetPos = controller.TargetMemory.LastKnownPosition;

            // 🔥 СОЗДАЁМ ТОЧКУ
            if (debugPointPrefab != null)
            {
                if (spawnedPoint != null)
                    Destroy(spawnedPoint);

                spawnedPoint = Instantiate(debugPointPrefab, targetPos, Quaternion.identity);
            }

            if (controller.Agent != null)
            {
                controller.Agent.speed = searchMoveSpeed;
                controller.Agent.isStopped = false;
                controller.Agent.stoppingDistance = 0f;
                controller.Agent.SetDestination(targetPos);
            }
        }

        public override void OnUpdateState()
        {
            if (controller.Agent == null || controller.TargetMemory == null)
                return;

            if (controller.TargetMemory.HasTarget)
                return;

            if (!hasArrived)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    controller.TargetMemory.LastKnownPosition
                );

                if (distance <= arriveDistance)
                {
                    hasArrived = true;
                    baseRotation = transform.rotation;

                    controller.Agent.isStopped = true;
                    controller.Agent.ResetPath();
                }

                return;
            }

            searchTimer += Time.deltaTime;

            if (lookAroundAfterArrive)
                DoLookAround();

            if (searchTimer >= searchDuration)
            {
                controller.TargetMemory.ClearVisualMemory();
            }
        }

        public override void OnExitState()
        {
            if (controller.Agent != null)
                controller.Agent.isStopped = false;

            hasArrived = false;
            searchTimer = 0f;

            // 🔥 УДАЛЯЕМ ТОЧКУ
            if (destroyPointOnExit && spawnedPoint != null)
            {
                Destroy(spawnedPoint);
            }
        }

        private void DoLookAround()
        {
            float step = lookAroundRotationSpeed * Time.deltaTime;

            if (rotateRight)
            {
                transform.Rotate(Vector3.up, step);

                if (Quaternion.Angle(baseRotation, transform.rotation) >= lookAroundAngle)
                    rotateRight = false;
            }
            else
            {
                transform.Rotate(Vector3.up, -step);

                if (Quaternion.Angle(baseRotation, transform.rotation) >= lookAroundAngle)
                    rotateRight = true;
            }
        }
    }
}